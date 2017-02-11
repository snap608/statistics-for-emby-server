using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
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
    public class Calculator
    {
        private IEnumerable<Movie> _cachedMovieList;
        private IEnumerable<Episode> _cachedEpisodeList;
        private IEnumerable<Series> _cachedSerieList;

        private readonly IUserManager _userManager;
        private readonly ILibraryManager _libraryManager;
        private readonly IUserDataManager _userDataManager;
        private User _user;

        public Calculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager)
        {
            _userManager = userManager;
            _libraryManager = libraryManager;
            _userDataManager = userDataManager;
        }

        public void SetUser(User user)
        {
            _user = user;
            _cachedEpisodeList = null;
            _cachedMovieList = null;
            _cachedSerieList = null;
        }

        #region TopYears

        public ValueGroup CalculateTopYears()
        {
            var movieList = _user == null
                ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed))
                : GetAllMovies(_user).Where(m => m.IsPlayed(_user)).ToList();
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
                Title = Constants.Topyears,
                Value = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };
        }

        #endregion

        #region LastSeen

        public ValueGroup CalculateLastSeenShows()
        {
            var viewedEpisodes = GetAllViewedEpisodesByUser(_user)
                .OrderByDescending(
                    m =>
                        _userDataManager.GetUserData(
                                _user ??
                                _userManager.Users.First(
                                    u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(5).ToList();

            var lastSeenList = viewedEpisodes
                .Select(item => new LastSeenModel
                {
                    Name = item.SeriesName + " - " + item.Name,
                    Played = _userDataManager.GetUserData(_user, item).LastPlayedDate ?? DateTime.MinValue,
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
            var list = new List<LastSeenModel>();
            var viewedMovies = GetAllViewedMoviesByUser(_user)
                .OrderByDescending(
                    m =>
                        _userDataManager.GetUserData(
                                _user ??
                                _userManager.Users.First(
                                    u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m)
                            .LastPlayedDate)
                .Take(5).ToList();

            var lastSeenList = viewedMovies
                .Select(item => new LastSeenModel
                {
                    Name = item.Name,
                    Played = _userDataManager.GetUserData(_user, item).LastPlayedDate ?? DateTime.MinValue,
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

        public ValueGroup CalculateTopMovieGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllMovies(_user).Where(m => m.IsVisible(_user)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllMovies(_user).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.TopMovieGenres,
                Value = string.Join(", ", result.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };
        }

        public ValueGroup CalculateTopShowGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllSeries(_user).Where(m => m.IsVisible(_user)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllSeries(_user).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return new ValueGroup
            {
                Title = Constants.TopShowGenres,
                Value = string.Join(", ", result.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };
        }

        #endregion

        #region PlayedViewTime

        public ValueGroup CalculateMovieTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var movies = _user == null
                ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllMovies(_user).Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var movie in movies)
            {
                runTime.Add(movie.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = Constants.TotalMoviesTime,
                Value = runTime.ToString()
            };
        }

        public ValueGroup CalculateShowTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var shows = _user == null
                ? GetAllOwnedEpisodes().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllOwnedEpisodes(_user).Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var show in shows)
            {
                runTime.Add(show.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = Constants.TotalShowTime,
                Value = runTime.ToString()
            };
        }

        public ValueGroup CalculateOverallTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var items = _user == null
                ? GetAllBaseItems().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllBaseItems().Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var item in items)
            {
                runTime.Add(item.RunTimeTicks);
            }

            return new ValueGroup
            {
                Title = Constants.TotalTime,
                Value = runTime.ToString(),
                Raw = runTime.Ticks
            };
        }

        #endregion

        #region TotalMedia

        public ValueGroup CalculateTotalMovies()
        {
            return new ValueGroup
            {
                Title = Constants.TotalMovies,
                Value = $"{GetAllMovies(_user).Count()}"
            };
        }

        public ValueGroup CalculateTotalShows()
        {
            return new ValueGroup
            {
                Title = Constants.TotalShows,
                Value = $"{GetAllSeries(_user).Count()}"
            };
        }

        public ValueGroup CalculateTotalEpisodes()
        {
            return new ValueGroup
            {
                Title = Constants.TotalEpisodes,
                Value = $"{GetAllOwnedEpisodes(_user).Count()}"
            };
        }

        #endregion

        #region MostActiveUsers

        public ValueGroup CalculateMostActiveUsers(Dictionary<string, RunTime> users)
        {
            var mostActiveUsers = users.OrderByDescending(x => x.Value).Take(5);

            return new ValueGroup
            {
                Title = Constants.MostActiveUsers,
                Value = string.Join("<br/>", mostActiveUsers.Select(x => $"{x.Key} - {x.Value.ToString()}")),
                Size = "medium"
            };
        }

        #endregion

        #region Quality

        public ValueGroup CalculateMovieQualities()
        {
            var movies = GetAllMovies(_user);
            var episodes = GetAllOwnedEpisodes(_user);

            var moWidths =
                movies.Select(
                        movie => movie.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video)?.Width)
                    .ToList();
            var epWidths =
                episodes.Select(
                        episode => episode.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video)?.Width)
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

        #region Helpers

        private IEnumerable<Movie> GetAllMovies(User user = null)
        {
            return _cachedMovieList ??
                   (_cachedMovieList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Movie>());
        }

        private IEnumerable<Series> GetAllSeries(User user = null)
        {
            return _cachedSerieList ??
                   (_cachedSerieList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Series>());
        }

        private IEnumerable<Episode> GetAllEpisodes(User user = null)
        {
            return _cachedEpisodeList ??
                   (_cachedEpisodeList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Episode>());
        }

        private IEnumerable<Episode> GetAllOwnedEpisodes(User user = null)
        {
            return GetAllEpisodes(user).Where(e => e.GetMediaStreams().Any());
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return
                GetAllEpisodes(user)
                    .Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return
                GetAllMovies(user)
                    .Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<BaseItem> GetAllBaseItems()
        {
            return GetAllMovies().Union(GetAllEpisodes().Cast<BaseItem>());
        }

        #endregion
    }
}