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

        public List<int> CalculateTopYears()
        {
            var movieList = _user == null ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed)) : GetAllMovies(_user).Where(m => m.IsPlayed(_user)).ToList();
            var list = movieList.Select(m => m.ProductionYear ?? 0).Distinct().ToList();
            var source = new Dictionary<int, int>();
            foreach (var num1 in list)
            {
                var year = num1;
                var num2 = movieList.Count(m => (m.ProductionYear ?? 0) == year);
                source.Add(year, num2);
            }

            return source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();

        }

        #endregion

        #region LastSeen

        public List<string> CalculateLastSeenShows()
        {
            var viewedEpisodes = GetAllViewedEpisodesByUser(_user)
                        .OrderByDescending(m => _userDataManager.GetUserData(_user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate)
                        .Take(5).ToList();

            return viewedEpisodes
                .Select(item => new LastSeenModel
                {
                    Name = item.Name,
                    Played = _userDataManager.GetUserData(_user, item).LastPlayedDate ?? DateTime.MinValue,
                    UserName = null
                }.ToString()).ToList();
        }

        public List<string> CalculateLastSeenMovies()
        {
            var list = new List<LastSeenModel>();
            var viewedMovies = GetAllViewedMoviesByUser(_user)
                        .OrderByDescending(m => _userDataManager.GetUserData(_user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate)
                        .Take(5).ToList();

            return viewedMovies
                .Select(item => new LastSeenModel
                {
                    Name = item.Name,
                    Played = _userDataManager.GetUserData(_user, item).LastPlayedDate ?? DateTime.MinValue,
                    UserName = null
                }.ToString()).ToList();
        }

        #endregion

        #region TopGenres

        public List<string> CalculateTopMovieGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllMovies(_user).Where(m => m.IsVisible(_user)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllMovies(_user).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return result.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();
        }

        public List<string> CalculateTopShowGenres()
        {
            var result = new Dictionary<string, int>();
            var genres = GetAllSeries(_user).Where(m => m.IsVisible(_user)).SelectMany(m => m.Genres).Distinct();

            foreach (var genre in genres)
            {
                var num = GetAllSeries(_user).Count(m => m.Genres.Contains(genre));
                result.Add(genre, num);
            }

            return result.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();
        }

        #endregion

        #region PlayedViewTime

        public string CalculateMovieTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var movies = _user == null
                ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllMovies(_user).Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var movie in movies)
            {
                runTime.Add(movie.RunTimeTicks);
            }

            return runTime.ToString();
        }

        public string CalculateShowTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var shows = _user == null
                ? GetAllEpisodes().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllEpisodes(_user).Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var show in shows)
            {
                runTime.Add(show.RunTimeTicks);
            }

            return runTime.ToString();
        }

        public string CalculateOverallTime(bool onlyPlayed = true)
        {
            var runTime = new RunTime();
            var items = _user == null
                ? GetAllBaseItems().Where(m => _userManager.Users.Any(m.IsPlayed) || !onlyPlayed)
                : GetAllBaseItems().Where(m => (m.IsPlayed(_user) || !onlyPlayed) && m.IsVisible(_user));
            foreach (var item in items)
            {
                runTime.Add(item.RunTimeTicks);
            }

            return runTime.ToString();
        }

        #endregion

        #region TotalMedia

        public int CalculateTotalMovies()
        {
            return GetAllMovies().Count();
        }

        public int CalculateTotalShows()
        {
            return GetAllSeries().Count();
        }

        public int CalculateTotalEpisodes()
        {
            return GetAllEpisodes().Count();
        }

        #endregion

        #region Quality

        public List<VideoQualityModel> CalculateMovieQualities()
        {
            var movies = GetAllMovies(_user);
            var widths = movies.Select(movie => movie.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video)?.Width).ToList();

            var ceilings  = new[] { 720, 1260, 1900, 2500, 3800 };
            var groupings = widths.GroupBy(item => ceilings.FirstOrDefault(ceiling => ceiling >= item)).ToList();

            return new List<VideoQualityModel>
            {
                new VideoQualityModel() { Quality = VideoQuality.Q720, Count = groupings.FirstOrDefault(x => x.Key == 700)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q1260, Count = groupings.FirstOrDefault(x => x.Key == 1260)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q1900, Count = groupings.FirstOrDefault(x => x.Key == 1900)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q2500, Count = groupings.FirstOrDefault(x => x.Key == 2500)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q3800, Count = groupings.FirstOrDefault(x => x.Key == 3800)?.Count() ?? 0 }
            };
        }

        public List<VideoQualityModel> CalculateEpisodeQualities()
        {
            var episodes = GetAllEpisodes(_user);

            var boe = episodes.First().GetDefaultVideoStream();
            var widths = episodes.Select(episode => episode.GetMediaStreams().FirstOrDefault(s => s.Type == MediaStreamType.Video)?.Width).ToList();

            var ceilings = new[] { 700, 1260, 1900, 2500, 3800 };
            var groupings = widths.GroupBy(item => ceilings.FirstOrDefault(ceiling => ceiling >= item)).ToList();

            return new List<VideoQualityModel>
            {
                new VideoQualityModel() { Quality = VideoQuality.Q720, Count = groupings.FirstOrDefault(x => x.Key == 700)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q1260, Count = groupings.FirstOrDefault(x => x.Key == 1260)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q1900, Count = groupings.FirstOrDefault(x => x.Key == 1900)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q2500, Count = groupings.FirstOrDefault(x => x.Key == 2500)?.Count() ?? 0 },
                new VideoQualityModel() { Quality = VideoQuality.Q3800, Count = groupings.FirstOrDefault(x => x.Key == 3800)?.Count() ?? 0 }
            };
        }

        #endregion

        #region Helpers

        private IEnumerable<Movie> GetAllMovies(User user = null)
        {
            return _cachedMovieList ?? (_cachedMovieList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Movie>());
        }

        private IEnumerable<Series> GetAllSeries(User user = null)
        {
            return _cachedSerieList ?? (_cachedSerieList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Series>());
        }

        private IEnumerable<Episode> GetAllEpisodes(User user = null)
        {
            return _cachedEpisodeList ?? (_cachedEpisodeList = _libraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Episode>());
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return GetAllEpisodes(user).Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return GetAllMovies(user).Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<BaseItem> GetAllBaseItems()
        {
            return GetAllMovies().Union(GetAllEpisodes().Cast<BaseItem>());
        }

        #endregion
    }
}
