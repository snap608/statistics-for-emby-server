using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using Statistics.Configuration;
using Statistics.Enum;
using Statistics.Extentions;
using Statistics.ViewModel;

namespace Statistics.RestServices
{
    public class StatisticsRestfulService : IService
    {
        private readonly CultureInfo _cul = Thread.CurrentThread.CurrentCulture;
        private readonly ILibraryManager _libraryManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;

        private IEnumerable<Movie> _cachedMovieList;
        private IEnumerable<Series> _cachedSerieList;
        private IEnumerable<Episode> _cachedEpisodeList;

        private static PluginConfiguration PluginConfiguration => Plugin.Instance.Configuration;

        public StatisticsRestfulService(ILibraryManager libraryManager, IJsonSerializer jsonSerializer, IUserManager userManager, IUserDataManager userDataManager)
        {
            _libraryManager = libraryManager;
            _jsonSerializer = jsonSerializer;
            _userManager = userManager;
            _userDataManager = userDataManager;
        }

        public object Get(UserStatistics request)
        {
            try
            {
                User userById = null;

                //A user is selected in the frontend
                if (request.Id != "0")
                {
                    userById = _userManager.GetUserById(request.Id);
                    if (userById == null)
                        return null;
                }
                
                var statViewModel = new StatViewModel
                {
                    MovieTotal = GetTotalMovies(userById),
                    EpisodeTotal = GetTotalEpisodes(userById),
                    ShowTotal = GetTotalShows(userById),
                    Stats = new List<ValueGroup>
                    {
                        GetTopGenres(RequestTypeEnum.Movies, "Top Movie genres", userById),
                        GetTopGenres(RequestTypeEnum.Shows, "Top Show genres", userById),
                        GetTopYears(Constants.Topyears, userById),
                        GetPlayedViewTime(RequestTypeEnum.Movies, "Movies watched", userById),
                        GetPlayedViewTime(RequestTypeEnum.Shows, "Shows watched", userById),
                        GetPlayedViewTime(RequestTypeEnum.All, "Total watched", userById),
                        GetViewTime(RequestTypeEnum.Movies, "Total movies time", userById),
                        GetViewTime(RequestTypeEnum.Shows, "Total shows time", userById),
                        GetViewTime(RequestTypeEnum.All, "Total time", userById)
                    },
                    BigStats = new List<ValueGroup>
                    {
                        GetLastViewed(RequestTypeEnum.Movies, "last seen movies", userById),
                        GetLastViewed(RequestTypeEnum.Shows, "last seen shows", userById)
                    }
                };

                return _jsonSerializer.SerializeToString(statViewModel);
            }
            catch (Exception ex)
            {
                return _jsonSerializer.SerializeToString(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private ValueGroup GetTotalMovies(User user = null)
        {
            var list = GetAllMovies(user);
            list = user != null ? list.Where(m => m.IsVisible(user)) : list;

            return new ValueGroup
            {
                MainValue = $"{list.Count()} movies",
                SubValue = user != null ? $"{list.Count(m => m.IsPlayed(user))} watched" : ""
            };
        }

        private ValueGroup GetTotalEpisodes(User user = null)
        {
            var list = GetAllEpisodes(user);
            list = user != null ? list.Where(e => e.IsVisible(user)) : list;

            return new ValueGroup
            {
                MainValue = $"{list.Count()} episodes",
                SubValue = user != null ? $"{list.Count(e => e.IsPlayed(user))} watched" : ""
            };
        }

        private ValueGroup GetTotalShows(User user = null)
        {
            var list = GetAllSeries(user);
            list = user != null ? list.Where(m => m.IsVisible(user)) : list;

            return new ValueGroup
            {
                MainValue = $"{list.Count()} shows",
                SubValue = user != null ? $"{list.Count(s => { return s.GetEpisodes(user).Any() && s.GetEpisodes(user).All(e => e.IsPlayed(user)); })} completed" : ""
            };
        }

        private ValueGroup GetTopGenres(RequestTypeEnum type, string title, User user = null)
        {
            var source = new Dictionary<string, int>();

            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var list1 = GetAllMovies(user);
                    if (user != null)
                        list1 = list1.Where(m => m.IsVisible(user)).ToList();

                    foreach (var genre in list1.SelectMany(m => m.Genres).Distinct())
                    {
                        var num = list1.Count(m => m.Genres.Contains(genre));
                        source.Add(genre, num);
                    }
                    break;
                case RequestTypeEnum.Shows:
                    var list2 = GetAllSeries(user);
                    if (user != null)
                        list2 = list2.Where(e => e.IsVisible(user)).ToList();

                    foreach (var genre in list2.SelectMany(m => m.Genres).Distinct())
                    {
                        var num = list2.Count(m => m.Genres.Contains(genre));
                        source.Add(genre, num);
                    }
                    break;
            }

            return new ValueGroup
            {
                MainValue = title,
                SubValue = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };
        }

        private ValueGroup GetLastViewed(RequestTypeEnum type, string title, User user = null)
        {
            var movieViewModelList = new List<MovieViewModel>();
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var list1 = (user == null ? GetAllViewedMoviesByAllUsers() : GetAllViewedMoviesByUser(user))
                        .OrderByDescending(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate)
                        .Take(5).ToList();

                    movieViewModelList.AddRange(list1.Select(item =>
                    {
                        var localUser = user ?? _userManager.Users.First(u => item.IsPlayed(u) && _userDataManager.GetUserData(u, item).LastPlayedDate.HasValue);
                        return new MovieViewModel
                        {
                            Name = item.Name,
                            Played = _userDataManager.GetUserData(localUser, item).LastPlayedDate ?? DateTime.MinValue,
                            UserName = localUser.Name
                        };
                    }));
                    break;
                case RequestTypeEnum.Shows:
                    var list2 = (user == null ? GetAllViewedEpisodesByAllUsers() : GetAllViewedEpisodesByUser(user))
                        .OrderByDescending(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate)
                        .Take(5).ToList();

                    movieViewModelList.AddRange(list2.Select(item =>
                    {
                        var user1 = user ?? _userManager.Users.First(u => item.IsPlayed(u) && _userDataManager.GetUserData(u, item).LastPlayedDate.HasValue);
                        return new MovieViewModel
                        {
                            Name = $"{item.SeriesName} - S{item.AiredSeasonNumber}xE{item.IndexNumber} - {item.Name}",
                            Played = _userDataManager.GetUserData(user1, item).LastPlayedDate ?? DateTime.MinValue,
                            UserName = user1.Name
                        };
                    }));
                    break;
            }

            return new ValueGroup
            {
                MainValue = title,
                SubValue = string.Join("<br/>", movieViewModelList)
            };
        }

        private ValueGroup GetPlayedViewTime(RequestTypeEnum type, string title, User user = null)
        {
            var runTime = new RunTime(new TimeSpan(0), VideoStateEnum.Watched);
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var movies = user == null ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed)) : GetAllMovies(user).Where(m => m.IsPlayed(user));
                    foreach (var movie in movies)
                    {
                        runTime.Add(new TimeSpan(movie.RunTimeTicks ?? 0));
                    }
                    break;
                case RequestTypeEnum.Shows:
                    var shows = user == null ? GetAllEpisodes().Where(m => _userManager.Users.Any(m.IsPlayed)) : GetAllEpisodes(user).Where(m => m.IsPlayed(user));
                    foreach (var show in shows)
                    {
                        runTime.Add(new TimeSpan(show.RunTimeTicks ?? 0));
                    }
                    break;
                case RequestTypeEnum.All:
                    var items = user == null ? GetAllBaseItems().Where(m => _userManager.Users.Any(m.IsPlayed)) : GetAllBaseItems().Where(m => m.IsPlayed(user));
                    foreach (var item in items)
                    {
                        runTime.Add(new TimeSpan(item.RunTimeTicks ?? 0));
                    }
                    break;
            }

            return new ValueGroup
            {
                MainValue = title,
                SubValue = runTime.ToString()
            };
        }

        private ValueGroup GetViewTime(RequestTypeEnum type, string title, User user = null)
        {
            var runTime = new RunTime(new TimeSpan(0L), VideoStateEnum.All);
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var movies = user == null ? GetAllMovies() : GetAllMovies(user).Where(m => m.IsVisible(user));
                    foreach (var movie in movies)
                    {
                        runTime.Add(new TimeSpan(movie.RunTimeTicks ?? 0L));
                    }
                    break;
                case RequestTypeEnum.Shows:
                    var shows = user == null ? GetAllEpisodes() : GetAllEpisodes(user).Where(m => m.IsVisible(user));
                    foreach (var show in shows)
                    {
                        runTime.Add(new TimeSpan(show.RunTimeTicks ?? 0L));
                    }
                    break;
                case RequestTypeEnum.All:
                    var items = user == null ? GetAllBaseItems() : GetAllBaseItems().Where(m => m.IsVisible(user));
                    foreach (var item in items)
                    {
                        runTime.Add(new TimeSpan(item.RunTimeTicks ?? 0L));
                    }
                    break;
            }

            return new ValueGroup
            {
                MainValue = title,
                SubValue = runTime.ToString()
            };
        }

        private ValueGroup GetTopYears(string title, User user = null)
        {
            var movieList = user == null ? GetAllMovies().Where(m => _userManager.Users.Any(m.IsPlayed)) : GetAllMovies(user).Where(m => m.IsPlayed(user)).ToList();
            var list = movieList.Select(m => m.ProductionYear ?? 0).Distinct().ToList();
            var source = new Dictionary<int, int>();
            foreach (var num1 in list)
            {
                var year = num1;
                var num2 = movieList.Count(m => (m.ProductionYear ?? 0) == year);
                source.Add(year, num2);
            }
            var valueGroup = new ValueGroup
            {
                MainValue = title,
                SubValue = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };

            return valueGroup;
        }

        public string Get(GetBackground request)
        {
            var random = new Random();
            var list = GetAllMovies().ToList();

            var count = 0;
            do
            {
                var index2 = random.Next(0, list.Count - 1);
                if (list[index2].GetImages((ImageType)2).Any())
                {
                    return _jsonSerializer.SerializeToString(list[index2].Id.ToString());
                }
                count++; 
            } while (count < 10);

            return "";
        }

        public object Get(ViewChart request)
        {
            try
            {
                User user = null;
                if (request.Id != "0")
                    user = _userManager.GetUserById(request.Id);

                if (DateTimeFormatInfo.CurrentInfo != null)
                {
                    var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
                    var graphValueList = new List<GraphValue>();

                    var sourceBaseItemList = user == null
                        ? GetAllBaseItems().Where(m => _userManager.Users.Any(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue)).ToList()
                        : GetAllBaseItems().Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue).ToList();

                    switch (request.TimeRange)
                    {
                        case TimeRangeEnum.Monthly:
                            graphValueList = CalculateMonthlyViewChart(sourceBaseItemList, cal, user, request.TimeRange);
                            break;
                        case TimeRangeEnum.Weekly:
                            graphValueList = CalculateWeeklyViewChart(sourceBaseItemList, cal, user, request.TimeRange);
                            break;
                        case TimeRangeEnum.Daily:
                            graphValueList = CalculateDailyViewChart(sourceBaseItemList, cal, user, request.TimeRange);
                            break;
                    }
                    return _jsonSerializer.SerializeToString(graphValueList.ToJSON());
                }

                return _jsonSerializer.SerializeToString("");
            }
            catch (Exception ex)
            {
                return _jsonSerializer.SerializeToString(new {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        public object Get(WeekChart request)
        {
            try
            {
                User user = null;
                if (request.Id != "0")
                    user = _userManager.GetUserById(request.Id);

                var sourceBaseItemList = user == null
                        ? GetAllBaseItems().Where(m => _userManager.Users.Any(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue)).ToList()
                        : GetAllBaseItems().Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue).ToList();

                var groupedList = sourceBaseItemList
                    .GroupBy(m => (GetUserItemData(m, user).LastPlayedDate ?? new DateTime()).DayOfWeek)
                    .OrderBy(d => d.Key)
                    .ToDictionary(k => _cul.DateTimeFormat.GetAbbreviatedDayName(k.Key), g => g.ToList().Count);

                return _jsonSerializer.SerializeToString(groupedList.Select(item => new GraphValue(item.Key, item.Value)).ToList().ToJSON());
            }
            catch (Exception ex)
            {
                return _jsonSerializer.SerializeToString(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        private List<GraphValue> CalculateMonthlyViewChart(IEnumerable<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetMonth(GetUserItemData(m, user).LastPlayedDate ?? new DateTime())).ToList();
            if (lastPlayedDGrouping.Any())
            {
                var timeRangeList = CalculateTimeRange(lastPlayedDGrouping.ToDictionary(k => k.Key, g => g.ToList().Count), timeRange).ToList();
                var overflow = timeRangeList.Count > 12 ? timeRangeList.Count - 12 : 0;
                returnList.AddRange(timeRangeList.Skip(overflow).Select(item => new GraphValue(_cul.DateTimeFormat.GetAbbreviatedMonthName(item.Key), item.Value)));
            }

            return returnList;
        }

        private List<GraphValue> CalculateWeeklyViewChart(IEnumerable<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetWeekOfYear(GetUserItemData(m, user).LastPlayedDate ?? new DateTime(), CalendarWeekRule.FirstDay, DayOfWeek.Monday)).ToList();
            if (lastPlayedDGrouping.Any())
            {
                var timeRangeList = CalculateTimeRange(lastPlayedDGrouping.ToDictionary(k => k.Key, g => g.ToList().Count), timeRange).ToList();
                var overflow = timeRangeList.Count > 20 ? timeRangeList.Count - 20 : 0;
                returnList.AddRange(timeRangeList.Skip(overflow).Select(item => new GraphValue(item.Key, item.Value)));
            }

            return returnList;
        }

        private List<GraphValue> CalculateDailyViewChart(IEnumerable<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetDayOfYear(GetUserItemData(m, user).LastPlayedDate ?? new DateTime())).OrderByDescending(x => x.Key).ToList();
            if (lastPlayedDGrouping.Any())
            {
                var timeRangeList = CalculateTimeRange(lastPlayedDGrouping.ToDictionary(k => k.Key, g => g.ToList().Count), timeRange).ToList();
                var overflowCount = timeRangeList.Count > 7 ? timeRangeList.Count - 7 : 0;
                returnList.AddRange(timeRangeList.Skip(overflowCount).Select(item =>
                {
                    var dateTime = new DateTime(DateTime.Now.Year, 1, 1).AddDays(item.Key - 1);
                    return new GraphValue(_cul.DateTimeFormat.GetAbbreviatedDayName(dateTime.DayOfWeek), item.Value);
                }));
            }
            return returnList;
        }

        private UserItemData GetUserItemData(BaseItem item, User user)
        {
            return _userDataManager.GetUserData(user ?? _userManager.Users.First(u => item.IsPlayed(u) && _userDataManager.GetUserData(u, item).LastPlayedDate.HasValue), item);
        }

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

        private IEnumerable<BaseItem> GetAllBaseItems()
        {
            return GetAllMovies().Union(GetAllEpisodes().Cast<BaseItem>());
        }

        private IEnumerable<Movie> GetAllViewedMoviesByAllUsers()
        {
            return GetAllMovies().Where(m => _userManager.Users.Any(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue));
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return GetAllMovies(user).Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByAllUsers()
        {
            return GetAllEpisodes().Where(s => _userManager.Users.Any(u => s.IsPlayed(u) && _userDataManager.GetUserData(u, s).LastPlayedDate.HasValue));
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return GetAllEpisodes(user).Where(m => m.IsPlayed(user) && _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        private IEnumerable<KeyValuePair<int, int>> CalculateTimeRange(Dictionary<int, int> list, TimeRangeEnum type)
        {
            if (list == null)
                return new List<KeyValuePair<int, int>>();
            var key1 = 1;
            var dictionary = new Dictionary<int, int>();
            foreach (var keyValuePair in list.OrderBy(k => k.Key))
            {
                for (; keyValuePair.Key != key1; ++key1)
                    dictionary.Add(key1, 0);
                ++key1;
            }
            foreach (var keyValuePair in dictionary)
                list.Add(keyValuePair.Key, keyValuePair.Value);
            var source = new List<KeyValuePair<int, int>>();
            if (DateTimeFormatInfo.CurrentInfo != null)
            {
                var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
                var now = 0;
                switch (type)
                {
                    case TimeRangeEnum.Monthly:
                        now = DateTime.Now.Month;
                        break;
                    case TimeRangeEnum.Weekly:
                        now = calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                        break;
                    case TimeRangeEnum.Daily:
                        now = calendar.GetDayOfYear(DateTime.Now);
                        break;
                }
                while (list.OrderBy(x => x.Key).Last().Key < now)
                {
                    var key2 = list.OrderBy(x => x.Key).Last().Key + 1;
                    list.Add(key2, 0);
                }
                source.AddRange(list.Where(k => k.Key <= now).OrderByDescending(k => k.Key).Select(item => new KeyValuePair<int, int>(item.Key, item.Value)));
                source.AddRange(list.Where(k => k.Key > now).OrderByDescending(k => k.Key).Select(item => new KeyValuePair<int, int>(item.Key, item.Value)));
            }
            return source.Take(20).Reverse();
        }
    }
}
