﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Statistics.Api;
using Statistics.Configuration;
using Statistics.Enum;
using Statistics.Models;
using Statistics.ViewModel;

namespace Statistics.Helpers
{
    public class Calculator : BaseCalculator
    {
        

        public Calculator(User user,IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager)
            :base(userManager, libraryManager, userDataManager)
        {
            User = user;
        }

        #region TopYears

        public ValueGroup CalculateFavoriteYears()
        {
            var movieList = User == null
                ? GetAllMovies().Where(m => UserManager.Users.Any(m.IsPlayed))
                : GetAllMovies().Where(m => m.IsPlayed(User)).ToList();
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
                ValueLineOne = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList()),
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserToMovieYears : null,
                Size = "half"
            };
        }

        #endregion

        #region LastSeen

        public ValueGroup CalculateLastSeenShows()
        {
            var viewedEpisodes = GetAllViewedEpisodesByUser()
                .OrderByDescending(
                    m =>
                        UserDataManager.GetUserData(
                                User ??
                                UserManager.Users.First(
                                    u => m.IsPlayed(u) && UserDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(8).ToList();

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
                ValueLineOne = string.Join("<br/>", lastSeenList),
                ValueLineTwo = "",
                Size = "large"
            };
        }

        public ValueGroup CalculateLastSeenMovies()
        {
            var viewedMovies = GetAllViewedMoviesByUser()
                .OrderByDescending(
                    m =>
                        UserDataManager.GetUserData(
                                User ??
                                UserManager.Users.First(
                                    u => m.IsPlayed(u) && UserDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(8).ToList();

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
                ValueLineOne = string.Join("<br/>", lastSeenList),
                ValueLineTwo = "",
                Size = "large"
            };
        }

        #endregion

        #region TopGenres

        public ValueGroup CalculateFavoriteMovieGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllMovies().Where(m => m.IsVisible(User)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllMovies().Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.FavoriteMovieGenres,
                ValueLineOne = string.Join(", ", result.OrderByDescending(g => g.Value).Take(3).Select(g => g.Key).ToList()),
                ExtraInformation = User != null ? Constants.HelpUserTopMovieGenres : null,
                ValueLineTwo = "",
                Size = "half"
            };
        }

        public ValueGroup CalculateFavoriteShowGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllSeries().Where(m => m.IsVisible(User)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllSeries().Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.FavoriteShowGenres,
                ValueLineOne = string.Join(", ", result.OrderByDescending(g => g.Value).Take(3).Select(g => g.Key).ToList()),
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTopShowGenres : null,
                Size = "mediumThin"
            };
        }

        #endregion

        #region PlayedViewTime

        public ValueGroup CalculateMovieTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var movies = User == null
                ? GetAllMovies().Where(m => UserManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllMovies().Where(m => (m.IsPlayed(User) || !onlyPlayed) && m.IsVisible(User));
            foreach (var movie in movies)
            {
                runTime.Add(movie.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = onlyPlayed ? Constants.TotalWatched : Constants.TotalWatchableTime,
                ValueLineOne = runTime.ToLongString(),
                ValueLineTwo = "",
                Size = "half"
            };
        }

        public ValueGroup CalculateShowTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var shows = User == null
                ? GetAllOwnedEpisodes().Where(m => UserManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllOwnedEpisodes().Where(m => (m.IsPlayed(User) || !onlyPlayed) && m.IsVisible(User));
            foreach (var show in shows)
            {
                runTime.Add(show.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = onlyPlayed ? Constants.TotalWatched : Constants.TotalWatchableTime,
                ValueLineOne = runTime.ToLongString(),
                ValueLineTwo = "",
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
                ValueLineOne = runTime.ToLongString(),
                ValueLineTwo = "",
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
                ValueLineOne = $"{GetOwnedCount(typeof(Movie))}",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalMovies : null
            };
        }

        public ValueGroup CalculateTotalShows()
        {
            return new ValueGroup
            {
                Title = Constants.TotalShows,
                ValueLineOne = $"{GetOwnedCount(typeof(Series))}",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalShows : null
            };
        }

        public ValueGroup CalculateTotalOwnedEpisodes()
        {
            return new ValueGroup
            {
                Title = Constants.TotalEpisodes,
                ValueLineOne = $"{GetOwnedCount(typeof(Episode))}",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalEpisode : null
            };
        }

        public ValueGroup CalculateTotalBoxsets()
        {
            return new ValueGroup
            {
                Title = Constants.TotalCollections,
                ValueLineOne = $"{GetBoxsets().Count()}",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalCollections : null
            };
        }

        public ValueGroup CalculateTotalMoviesWatched()
        {
            var viewedMoviesCount = GetAllViewedMoviesByUser().Count();
            var totalMoviesCount = GetOwnedCount(typeof(Movie));

            var percentage = decimal.Zero;
            if (totalMoviesCount > 0)
                percentage = Math.Round(viewedMoviesCount / (decimal)totalMoviesCount * 100, 1);


            return new ValueGroup
            {
                Title = Constants.TotalMoviesWatched,
                ValueLineOne = $"{viewedMoviesCount} ({percentage}%)",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalMoviesWatched : null
            };
        }

        public ValueGroup CalculateTotalEpiosodesWatched()
        {
            var seenEpisodesCount = GetAllSeries().ToList().Sum(GetPlayedEpisodeCount);
            var totalEpisodes = GetOwnedCount(typeof(Episode));

            var percentage = decimal.Zero;
            if (totalEpisodes > 0)
                percentage = Math.Round(seenEpisodesCount / (decimal) totalEpisodes * 100, 1);

            return new ValueGroup
            {
                Title = Constants.TotalEpisodesWatched,
                ValueLineOne = $"{seenEpisodesCount} ({percentage}%)",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalEpisodesWatched : null
            };
        }

        public ValueGroup CalculateTotalFinishedShows(UpdateModel tvdbData)
        {
            var showList = GetAllSeries();
            var count = 0;

            foreach (var show in showList)
            {
                var totalEpisodes = tvdbData.IdList.FirstOrDefault(x => x.ShowId == show.GetProviderId(MetadataProviders.Tvdb))?.Count ?? 0;
                var seenEpisodes = GetPlayedEpisodeCount(show);

                if (seenEpisodes > totalEpisodes)
                    totalEpisodes = seenEpisodes;

                if (totalEpisodes > 0 && totalEpisodes == seenEpisodes)
                    count++;
            }

            return new ValueGroup
            {
                Title = Constants.TotalShowsFinished,
                ValueLineOne = $"{count}",
                ValueLineTwo = "",
                ExtraInformation = User != null ? Constants.HelpUserTotalShowsFinished : null
            };
        }

        #endregion

        #region MostActiveUsers

        public ValueGroup CalculateMostActiveUsers(Dictionary<string, RunTime> users)
        {
            var mostActiveUsers = users.OrderByDescending(x => x.Value).Take(6);

            var tempList = mostActiveUsers.Select(x => $"<tr><td>{x.Key}</td>{x.Value.ToString()}</tr>");

            return new ValueGroup
            {
                Title = Constants.MostActiveUsers,
                ValueLineOne =$"<table><tr><td></td><td>Days</td><td>Hours</td><td>Minutes</td></tr>{string.Join("", tempList)}</table>",
                ValueLineTwo = "",
                Size = "medium",
                ExtraInformation = Constants.HelpMostActiveUsers
            };
        }

        #endregion

        #region Quality

        public ValueGroup CalculateMovieQualities()
        {
            var movies = GetAllMovies();
            var episodes = GetAllOwnedEpisodes();

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
                ValueLineOne = $"<table><tr><td></td><td>Movies</td><td>Episodes</td></tr>{string.Join("", qualityList)}</table>" ,
                ValueLineTwo = "",
                Size = "medium"
            };
        }

        #endregion

        #region Size

        public ValueGroup CalculateBiggestMovie()
        {
            var movies = GetAllMovies();

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
            var valueLineOne = CheckMaxLength($"{maxSize:F1} Gb");
            var valueLineTwo = CheckMaxLength($"{biggestMovie.Name}");

            return new ValueGroup
            {
                Title = Constants.BiggestMovie,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateBiggestShow()
        {
            var shows = GetAllSeries();

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
            var valueLineOne = CheckMaxLength($"{maxSize:F1} Gb");
            var valueLineTwo = CheckMaxLength($"{biggestShow.Name}");

            return new ValueGroup
            {
                Title = Constants.BiggestShow,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        #endregion

        #region Period

        public ValueGroup CalculateLongestMovie()
        {
            var movies = GetAllMovies();

            var maxMovie = movies.Where(x => x.RunTimeTicks != null).OrderByDescending(x => x.RunTimeTicks).First();
            var valueLineOne = CheckMaxLength(new TimeSpan(maxMovie.RunTimeTicks ?? 0).ToString(@"hh\:mm\:ss"));
            var valueLineTwo = CheckMaxLength($"{maxMovie.Name}");
            return new ValueGroup
            {
                Title = Constants.LongestMovie,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateLongestShow()
        {
            var shows = GetAllSeries();

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

            var time = new TimeSpan(maxTime).ToString(@"hh\:mm\:ss");

            var days = CheckForPlural("day", new TimeSpan(maxTime).Days, "", "and");

            var valueLineOne = CheckMaxLength($"{days} {time}");
            var valueLineTwo = CheckMaxLength($"{maxShow.Name}");
            return new ValueGroup
            {
                Title = Constants.LongestShow,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        #endregion

        #region Release Date

        public ValueGroup CalculateOldestMovie()
        {
            var movies = GetAllMovies();
            var oldest = movies.Where(x => x.PremiereDate.HasValue && x.PremiereDate.Value > DateTime.MinValue).Aggregate((curMin, x) => (curMin == null || (x.PremiereDate ?? DateTime.MaxValue) < curMin.PremiereDate ? x : curMin));

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (oldest != null && oldest.PremiereDate.HasValue)
            {
                var oldestDate = oldest.PremiereDate.Value;
                var numberOfTotalMonths = (DateTime.Now.Year - oldestDate.Year) * 12 + DateTime.Now.Month - oldestDate.Month;
                var numberOfYears = Math.Floor(numberOfTotalMonths / (decimal)12);
                var numberOfMonth = Math.Floor((numberOfTotalMonths / (decimal)12 - numberOfYears) * 12);

                valueLineOne = CheckMaxLength($"{CheckForPlural("year", numberOfYears, "", "", false)} {CheckForPlural("month", numberOfMonth, "and")} ago");
                valueLineTwo = CheckMaxLength($"{oldest.Name}");
            }
            
            return new ValueGroup()
            {
                Title = Constants.OldesPremieredtMovie,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateNewestMovie()
        {
            var movies = GetAllMovies();
            var youngest = movies.Where(x => x.PremiereDate.HasValue).Aggregate((curMax, x) => (curMax == null || (x.PremiereDate ?? DateTime.MinValue) > curMax.PremiereDate ? x : curMax));

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (youngest != null)
            {
                var numberOfTotalDays = DateTime.Now.Date - youngest.PremiereDate.Value;
                valueLineOne = CheckMaxLength(numberOfTotalDays.Days == 0
                        ? $"Today"
                        : $"{CheckForPlural("day", numberOfTotalDays.Days, "", "", false)} ago");
                
                valueLineTwo = CheckMaxLength($"{youngest.Name}");
            }

            return new ValueGroup()
            {
                Title = Constants.NewestPremieredMovie,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateNewestAddedMovie()
        {
            var movies = GetAllMovies();
            var youngest = movies.Aggregate((curMax, x) => (curMax == null || x.DateCreated > curMax.DateCreated ? x : curMax));

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (youngest != null)
            {
                var numberOfTotalDays = DateTime.Now - youngest.DateCreated;

                valueLineOne =
                    CheckMaxLength(numberOfTotalDays.Days == 0
                        ? $"Today"
                        : $"{CheckForPlural("day", numberOfTotalDays.Days, "", "", false)} ago");

                valueLineTwo = CheckMaxLength($"{youngest.Name}");
            }

            return new ValueGroup()
            {
                Title = Constants.NewestAddedMovie,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateNewestAddedEpisode()
        {
            var episodes = GetAllOwnedEpisodes();
            var youngest = episodes.Aggregate((curMax, x) => (curMax == null || x.DateCreated > curMax.DateCreated ? x : curMax));

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (youngest != null)
            {
                var numberOfTotalDays = DateTime.Now.Date - youngest.DateCreated;

                valueLineOne =
                    CheckMaxLength(numberOfTotalDays.Days == 0
                        ? $"Today"
                        : $"{CheckForPlural("day", numberOfTotalDays.Days, "", "", false)} ago");


                valueLineTwo = CheckMaxLength($"{youngest.Series.Name} S{youngest.AiredSeasonNumber} E{youngest.DvdEpisodeNumber} ");
            }

            return new ValueGroup()
            {
                Title = Constants.NewestAddedEpisode,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        #endregion

        #region Ratings

        public ValueGroup CalculateHighestRating()
        {
            var movies = GetAllMovies();
            var highestRatedMovie = movies.Where(x => x.CommunityRating.HasValue).OrderByDescending(x => x.CommunityRating).FirstOrDefault();

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (highestRatedMovie != null) {
                valueLineOne = CheckMaxLength($"{highestRatedMovie.CommunityRating} / 10");
                valueLineTwo = CheckMaxLength($"{highestRatedMovie.Name}");
            }

            return new ValueGroup()
            {
                Title = Constants.HighestMovieRating,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        public ValueGroup CalculateLowestRating()
        {
            var movies = GetAllMovies();
            var lowestRatedMovie = movies.Where(x => x.CommunityRating.HasValue && x.CommunityRating != 0).OrderBy(x => x.CommunityRating).FirstOrDefault();

            var valueLineOne = Constants.NoData;
            var valueLineTwo = "";
            if (lowestRatedMovie != null)
            {
                valueLineOne = CheckMaxLength($"{lowestRatedMovie.CommunityRating} / 10");
                valueLineTwo = CheckMaxLength($"{lowestRatedMovie.Name}");
            }

            return new ValueGroup()
            {
                Title = Constants.LowestMovieRating,
                ValueLineOne = valueLineOne,
                ValueLineTwo = valueLineTwo,
                Size = "half"
            };
        }

        #endregion

        private string CheckMaxLength(string value)
        {
            if (value.Length > 43)
                return value.Substring(0, 40) + "...";
            return value;;
        }

        private string CheckForPlural(string value, decimal number, string starting = "", string ending = "", bool removeZero = true)
        {
            if(number == 1)
                return $" {starting} {number} {value} {ending}";
            if (number == 0 && removeZero)
                return "";
            return $" {starting} {number} {value}s {ending}";
        }
    }
}