using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Statistics.Configuration;
using Statistics.Enum;
using Statistics.Models;
using Statistics.ViewModel;

namespace Statistics.Helpers
{
    public class Calculator : BaseCalculator
    {
        

        public Calculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager)
            :base(userManager, libraryManager, userDataManager)
        {
            
        }

        #region TopYears

        public ValueGroup CalculateFavoriteYears()
        {
            var movieList = User == null
                ? GetAllMovies().Where(m => UserManager.Users.Any(m.IsPlayed))
                : GetAllMovies(User).Where(m => m.IsPlayed(User)).ToList();
            var list = movieList.Select(m => m.ProductionYear ?? 0).Distinct().ToList();
            var source = new Dictionary<int, int>();
            foreach (var num1 in list)
            {
                var year = num1;
                var num2 = movieList.Count(m => (m.ProductionYear ?? 0) == year);
                source.Add(year, num2);
            }

            return new ValueGroup
            {
                Title = Constants.FavoriteYears,
                Value = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList()),
                ExtraInformation = User != null ? Constants.HelpUserToMovieYears : null
            };
        }

        #endregion

        #region LastSeen

        public ValueGroup CalculateLastSeenShows()
        {
            var viewedEpisodes = GetAllViewedEpisodesByUser(User)
                .OrderByDescending(
                    m =>
                        UserDataManager.GetUserData(
                                User ??
                                UserManager.Users.First(
                                    u => m.IsPlayed(u) && UserDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(6).ToList();

            var lastSeenList = viewedEpisodes
                .Select(item => new LastSeenModel
                {
                    Name = item.SeriesName + " - " + item.Name,
                    Played = UserDataManager.GetUserData(User, item).LastPlayedDate ?? DateTime.MinValue,
                    UserName = null
                }.ToString()).ToList();

            return new ValueGroup
            {
                Title = Constants.LastSeenShows,
                Value = string.Join("<br/>", lastSeenList),
                Size = "large"
            };
        }

        public ValueGroup CalculateLastSeenMovies()
        {
            var viewedMovies = GetAllViewedMoviesByUser(User)
                .OrderByDescending(
                    m =>
                        UserDataManager.GetUserData(
                                User ??
                                UserManager.Users.First(
                                    u => m.IsPlayed(u) && UserDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(6).ToList();

            var lastSeenList = viewedMovies
                .Select(item => new LastSeenModel
                {
                    Name = item.Name,
                    Played = UserDataManager.GetUserData(User, item).LastPlayedDate ?? DateTime.MinValue,
                    UserName = null
                }.ToString()).ToList();

            return new ValueGroup
            {
                Title = Constants.LastSeenMovies,
                Value = string.Join("<br/>", lastSeenList),
                Size = "large"
            };
        }

        #endregion

        #region TopGenres

        public ValueGroup CalculateFavoriteMovieGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllMovies(User).Where(m => m.IsVisible(User)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllMovies(User).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.FavoriteMovieGenres,
                Value = string.Join(", ", result.OrderByDescending(g => g.Value).Take(3).Select(g => g.Key).ToList()),
                ExtraInformation = User != null ? Constants.HelpUserTopMovieGenres : null

            };
        }

        public ValueGroup CalculateFavoriteShowGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllSeries(User).Where(m => m.IsVisible(User)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllSeries(User).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.favoriteShowGenres,
                Value = string.Join(", ", result.OrderByDescending(g => g.Value).Take(3).Select(g => g.Key).ToList()),
                ExtraInformation = User != null ? Constants.HelpUserTopShowGenres : null
            };
        }

        #endregion

        #region PlayedViewTime

        public ValueGroup CalculateMovieTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var movies = User == null
                ? GetAllMovies().Where(m => UserManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllMovies(User).Where(m => (m.IsPlayed(User) || !onlyPlayed) && m.IsVisible(User));
            foreach (var movie in movies)
            {
                runTime.Add(movie.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = onlyPlayed ? Constants.TotalWatched : Constants.TotalWatchableTime,
                Value = runTime.ToString(),
                Size = "half"
            };
        }

        public ValueGroup CalculateShowTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var shows = User == null
                ? GetAllOwnedEpisodes().Where(m => UserManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllOwnedEpisodes(User).Where(m => (m.IsPlayed(User) || !onlyPlayed) && m.IsVisible(User));
            foreach (var show in shows)
            {
                runTime.Add(show.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = onlyPlayed ? Constants.TotalWatched : Constants.TotalWatchableTime,
                Value = runTime.ToString(),
                Size = "half"
            };
        }

        public ValueGroup CalculateOverallTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var items = User == null
                ? GetAllBaseItems().Where(m => UserManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllBaseItems().Where(m => (m.IsPlayed(User) || !onlyPlayed) && m.IsVisible(User));
            foreach (var item in items)
            {
                runTime.Add(item.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = onlyPlayed ? Constants.TotalWatched : Constants.TotalWatchableTime,
                Value = runTime.ToString(),
                Raw = runTime.Ticks,
                Size = "half"
            };
        }

        #endregion

        #region TotalMedia

        public ValueGroup CalculateTotalMovies()
        {
            return new ValueGroup
            {
                Title = Constants.TotalMovies,
                Value = $"{GetAllMovies(User).Count()}",
                ExtraInformation = User != null ? Constants.HelpUserTotalMovies : null
            };
        }

        public ValueGroup CalculateTotalShows()
        {
            return new ValueGroup
            {
                Title = Constants.TotalShows,
                Value = $"{GetAllSeries(User).Count()}",
                ExtraInformation = User != null ? Constants.HelpUserTotalShows : null
            };
        }

        public ValueGroup CalculateTotalEpisodes()
        {
            return new ValueGroup
            {
                Title = Constants.TotalEpisodes,
                Value = $"{GetAllOwnedEpisodes(User).Count()}",
                ExtraInformation = User != null ? Constants.HelpUserTotalEpisode : null
            };
        }

        #endregion

        #region MostActiveUsers

        public ValueGroup CalculateMostActiveUsers(Dictionary<string, RunTime> users)
        {
            var mostActiveUsers = users.OrderByDescending(x => x.Value).Take(6);

            return new ValueGroup
            {
                Title = Constants.MostActiveUsers,
                Value = string.Join("<br/>", mostActiveUsers.Select(x => $"{x.Key}: {x.Value.ToString()}")),
                Size = "medium",
                ExtraInformation = Constants.HelpMostActiveUsers
            };
        }

        #endregion

        #region Quality

        public ValueGroup CalculateMovieQualities()
        {
            var movies = GetAllMovies(User);
            var episodes = GetAllOwnedEpisodes(User);

            var moWidths = movies
                    .Select(x => x.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video))
                    .Select(x => x != null ? x.Width : 0)
                    .ToList();
            var epWidths = episodes
                    .Select(x => x.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video))
                    .Select(x => x != null ? x.Width : 0)
                    .ToList();

            var ceilings = new[] { 3800, 2500, 1900, 1260, 700 };
            var moGroupings = moWidths.GroupBy(item => ceilings.FirstOrDefault(ceiling => ceiling < item)).ToList();
            var epGroupings = epWidths.GroupBy(item => ceilings.FirstOrDefault(ceiling => ceiling < item)).ToList();

            var qualityList = new List<VideoQualityModel>
            {
                new VideoQualityModel
                {
                    Quality = VideoQuality.DVD,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 0)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 0)?.Count() ?? 0
                },
                new VideoQualityModel
                {
                    Quality = VideoQuality.Q700,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 700)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 700)?.Count() ?? 0
                },
                new VideoQualityModel
                {
                    Quality = VideoQuality.Q1260,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 1260)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 1260)?.Count() ?? 0
                },
                new VideoQualityModel
                {
                    Quality = VideoQuality.Q1900,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 1900)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 1900)?.Count() ?? 0
                },
                new VideoQualityModel
                {
                    Quality = VideoQuality.Q2500,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 2500)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 2500)?.Count() ?? 0
                },
                new VideoQualityModel
                {
                    Quality = VideoQuality.Q3800,
                    Movies = moGroupings.FirstOrDefault(x => x.Key == 3800)?.Count() ?? 0,
                    Episodes = epGroupings.FirstOrDefault(x => x.Key == 3800)?.Count() ?? 0
                }
            };

            return new ValueGroup
            {
                Title = Constants.MediaQualities,
                Value = string.Join("<br/>", qualityList),
                Size = "medium"
            };
        }

        #endregion

        #region Size

        public ValueGroup CalculateBiggestMovie()
        {
            var movies = GetAllMovies(User);

            var biggestMovie = new Movie();
            double maxSize = 0;
            foreach (var movie in movies)
            {
                try
                {
                    var f = new FileInfo(movie.Path);
                    if (maxSize >= f.Length) continue;

                    maxSize = f.Length;
                    biggestMovie = movie;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            maxSize = maxSize / 1073741824; //Byte to Gb

            return new ValueGroup
            {
                Title = Constants.BiggestShow,
                Value = $"{maxSize:F1} Gb  - {biggestMovie.OriginalTitle}",
                Size = "half"
            };
        }

        public ValueGroup CalculateBiggestShow()
        {
            var shows = GetAllSeries(User);

            var biggestShow = new Series();
            double maxSize = 0;
            foreach (var show in shows)
            {
                var episodes = GetAllEpisodes().Where(x => x.SeriesId == show.Id && x.Path != null);
                try
                {
                    var showSize = episodes.Sum(x =>
                    {
                        var f = new FileInfo(x.Path);
                        return f.Length;
                    });

                    if (maxSize >= showSize) continue;

                    maxSize = showSize;
                    biggestShow = show;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            maxSize = maxSize / 1073741824; //Byte to Gb

            return new ValueGroup
            {
                Title = Constants.BiggestShow,
                Value = $"{maxSize:F1} Gb - {biggestShow.Name}",
                Size = "half"
            };
        }

        #endregion

        #region Period

        public ValueGroup CalculateLongestMovie()
        {
            var movies = GetAllMovies(User);

            var maxMovie = movies.Where(x => x.RunTimeTicks != null).OrderByDescending(x => x.RunTimeTicks).First();

            return new ValueGroup
            {
                Title = Constants.LongestMovie,
                Value = new TimeSpan(maxMovie.RunTimeTicks ?? 0).ToString(@"hh\:mm\:ss") + " - " + maxMovie.OriginalTitle,
                Size = "half"
            };
        }

        public ValueGroup CalculateLongestShow()
        {
            var shows = GetAllSeries(User);

            var maxShow = new Series();
            long maxTime = 0;
            foreach (var show in shows)
            {
                var episodes = GetAllEpisodes().Where(x => x.SeriesId == show.Id && x.Path != null);
                var showSize = episodes.Sum(x => x.RunTimeTicks ?? 0);

                if (maxTime >= showSize) continue;

                maxTime = showSize;
                maxShow = show;

            }
            
            return new ValueGroup
            {
                Title = Constants.LongestShow,
                Value = new TimeSpan(maxTime).ToString(@"d\ hh\:mm\:ss") + " - " + maxShow.Name,
                Size = "half"
            };
        }

        #endregion
    }
}