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
        private readonly IActivityManager _activityManager;

        private IEnumerable<Movie> _cachedMovieList;
        private IEnumerable<Series> _cachedSerieList;
        private IEnumerable<Episode> _cachedEpisodeList;

        private static PluginConfiguration PluginConfiguration => Plugin.Instance.Configuration;

        public StatisticsRestfulService(ILibraryManager libraryManager, IJsonSerializer jsonSerializer, IUserManager userManager, IUserDataManager userDataManager, IActivityManager activityManager)
        {
            _libraryManager = libraryManager;
            _jsonSerializer = jsonSerializer;
            _userManager = userManager;
            _userDataManager = userDataManager;
            _activityManager = activityManager;
        }

        public object Get(UserStatistics request)
        {
            try
            {
                var userById = _userManager.GetUserById(request.Id);
                if (userById == null)
                    return null;
                var statViewModel = new StatViewModel
                {
                    MovieTotal = GetTotalMovies(userById),
                    EpisodeTotal = GetTotalEpisodes(userById),
                    ShowTotal = GetTotalShows(userById)
                };

                statViewModel.TopMovieGenres = new ValueGroup
                {
                    MainValue = "Top Movie genres",
                    SubValue = string.Join(", ", GetTopGenres(RequestTypeEnum.Movies, userById))
                };

                statViewModel.TopShowGenres = new ValueGroup
                {
                    MainValue = "Top Show genres",
                    SubValue = string.Join(", ", GetTopGenres(RequestTypeEnum.Shows, userById))
                };

                var valueGroup3 = new ValueGroup
                {
                    MainValue = "last seen movies",
                    SubValue = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Movies, userById))
                };

                statViewModel.MovieLastViewed = valueGroup3;
                var valueGroup4 = new ValueGroup
                {
                    MainValue = "last seen shows",
                    SubValue = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Shows, userById))
                };

                statViewModel.ShowLastViewed = valueGroup4;
                var valueGroup5 = new ValueGroup
                {
                    MainValue = "Movies watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.Movies, userById).ToString()
                };

                statViewModel.MoviePlayedViewTime = valueGroup5;
                var valueGroup6 = new ValueGroup
                {
                    MainValue = "Shows watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.Shows, userById).ToString()
                };

                statViewModel.ShowPlayedViewTime = valueGroup6;
                var valueGroup7 = new ValueGroup
                {
                    MainValue = "Total watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.All, userById).ToString()
                };

                statViewModel.TotalPlayedViewTime = valueGroup7;
                var valueGroup8 = new ValueGroup
                {
                    MainValue = "Total movies time",
                    SubValue = GetViewTime(RequestTypeEnum.Movies, userById).ToString()
                };

                statViewModel.MovieViewTime = valueGroup8;
                var valueGroup9 = new ValueGroup
                {
                    MainValue = "Total shows time",
                    SubValue = GetViewTime(RequestTypeEnum.Shows, userById).ToString()
                };
                statViewModel.ShowViewTime = valueGroup9;
                var valueGroup10 = new ValueGroup
                {
                    MainValue = "Total time",
                    SubValue = GetViewTime(RequestTypeEnum.All, userById).ToString()
                };

                statViewModel.TotalViewTime = valueGroup10;
                var topYears = GetTopYears(userById);
                statViewModel.TopYears = topYears;
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

        public object Get(MainStatistics request)
        {
            try
            {
                var statViewModel = new StatViewModel
                {
                    MovieTotal = GetTotalMovies(),
                    EpisodeTotal = GetTotalEpisodes(),
                    ShowTotal = GetTotalShows()
                };

                statViewModel.TopMovieGenres = new ValueGroup
                {
                    MainValue = "Top Movie genres",
                    SubValue = string.Join(", ", GetTopGenres(RequestTypeEnum.Movies))
                };

                statViewModel.TopShowGenres = new ValueGroup
                {
                    MainValue = "Top Show genres",
                    SubValue = string.Join(", ", GetTopGenres(RequestTypeEnum.Shows))
                };

                statViewModel.MovieLastViewed = new ValueGroup
                {
                    MainValue = "last seen movies",
                    SubValue = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Movies))
                };

                statViewModel.ShowLastViewed = new ValueGroup
                {
                    MainValue = "last seen shows",
                    SubValue = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Shows))
                };

                statViewModel.MoviePlayedViewTime = new ValueGroup
                {
                    MainValue = "Movies watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.Movies).ToString()
                };

                statViewModel.ShowPlayedViewTime = new ValueGroup
                {
                    MainValue = "Show watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.Shows).ToString()
                };

                statViewModel.TotalPlayedViewTime = new ValueGroup
                {
                    MainValue = "Total watched",
                    SubValue = GetPlayedViewTime(RequestTypeEnum.All).ToString()
                };

                statViewModel.MovieViewTime = new ValueGroup
                {
                    MainValue = "Total movies time",
                    SubValue = GetViewTime(RequestTypeEnum.Movies).ToString()
                };

                statViewModel.ShowViewTime = new ValueGroup
                {
                    MainValue = "Total show time",
                    SubValue = GetViewTime(RequestTypeEnum.Shows).ToString()
                };

                statViewModel.TotalViewTime = new ValueGroup
                {
                    MainValue = "Total time",
                    SubValue = GetViewTime(RequestTypeEnum.All).ToString()
                };

                var topYears = GetTopYears();
                statViewModel.TopYears = topYears;
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

        private List<string> GetTopGenres(RequestTypeEnum type, User user = null)
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
            return source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();
        }

        private List<MovieViewModel> GetLastViewed(RequestTypeEnum type, User user = null)
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
            return movieViewModelList;
        }

        private RunTime GetPlayedViewTime(RequestTypeEnum type, User user = null)
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
            return runTime;
        }

        private RunTime GetViewTime(RequestTypeEnum type, User user = null)
        {
            var runTime = new RunTime(new TimeSpan(0L), VideoStateEnum.All);
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var movies = user == null ? _libraryManager.RootFolder.Children.OfType<Movie>() : _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => m.IsVisible(user));
                    foreach (var movie in movies)
                    {
                        runTime.Add(new TimeSpan(movie.RunTimeTicks ?? 0L));
                    }
                    break;
                case RequestTypeEnum.Shows:
                    var shows = user == null ? _libraryManager.RootFolder.Children.OfType<Episode>() : _libraryManager.RootFolder.Children.OfType<Episode>().Where(m => m.IsVisible(user));
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
            return runTime;
        }

        private ValueGroup GetTopYears(User user = null)
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
                MainValue = "top movie years",
                SubValue = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList())
            };

            return valueGroup;
        }

        public object Get(GetBackground request)
        {
            var stringList1 = new List<string>();
            var random = new Random();
            for (var index1 = 0; index1 < int.Parse(request.Count); ++index1)
            {
                var list = GetAllMovies().ToList();
                int index2;
                Guid id;
                int num;
                do
                {
                    index2 = random.Next(0, list.Count - 1);
                    if (list[index2].GetImages((ImageType)2).Any())
                    {
                        var stringList2 = stringList1;
                        id = list[index2].Id;
                        num = stringList2.Contains(id.ToString()) ? 1 : 0;
                    }
                    else
                        num = 0;
                }
                while (num != 0);
                var stringList3 = stringList1;
                id = list[index2].Id;
                var str1 = id.ToString();
                stringList3.Add(str1);
            }
            return stringList1.ToJSON();
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
                return _jsonSerializer.SerializeToString((user == null ? GetAllBaseItems().Where(m => _userManager.Users.Any(u =>
                {
                    if (m.IsPlayed(u))
                        return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                    return false;
                })) : GetAllBaseItems().Where(m =>
                {
                    if (m.IsPlayed(user))
                        return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                    return false;
                })).GroupBy(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u =>
                {
                    if (m.IsPlayed(u))
                        return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                    return false;
                }), m).LastPlayedDate.Value.DayOfWeek).OrderBy(d => d.Key).ToDictionary(k => _cul.DateTimeFormat.GetAbbreviatedDayName(k.Key), g => g.ToList().Count()).Select(item => new GraphValue(item.Key, item.Value)).ToList().ToJSON());
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


        private List<GraphValue> CalculateMonthlyViewChart(List<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetMonth(_userDataManager.GetUserData(user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate ?? new DateTime())).ToList();
            if (lastPlayedDGrouping.Any())
            {
                var timeRangeList = CalculateTimeRange(lastPlayedDGrouping.ToDictionary(k => k.Key, g => g.ToList().Count), timeRange).ToList();
                var overflow = timeRangeList.Count > 12 ? timeRangeList.Count - 12 : 0;
                returnList.AddRange(timeRangeList.Skip(overflow).Select(item => new GraphValue(_cul.DateTimeFormat.GetAbbreviatedMonthName(item.Key), item.Value)));
            }

            return returnList;
        }

        private List<GraphValue> CalculateWeeklyViewChart(List<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetWeekOfYear(_userDataManager.GetUserData(user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate ?? new DateTime(), CalendarWeekRule.FirstDay, DayOfWeek.Monday)).ToList();
            if (lastPlayedDGrouping.Any())
            {
                var timeRangeList = CalculateTimeRange(lastPlayedDGrouping.ToDictionary(k => k.Key, g => g.ToList().Count), timeRange).ToList();
                var overflow = timeRangeList.Count > 20 ? timeRangeList.Count - 20 : 0;
                returnList.AddRange(timeRangeList.Skip(overflow).Select(item => new GraphValue(item.Key, item.Value)));
            }

            return returnList;
        }

        private List<GraphValue> CalculateDailyViewChart(List<BaseItem> sourceBaseItemList, Calendar cal, User user, TimeRangeEnum timeRange)
        {
            var returnList = new List<GraphValue>();
            var lastPlayedDGrouping = sourceBaseItemList.GroupBy(m => cal.GetDayOfYear(_userDataManager.GetUserData(user ?? _userManager.Users.First(u => m.IsPlayed(u) && _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue), m).LastPlayedDate ?? new DateTime())).OrderByDescending(x => x.Key).ToList();
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
            return GetAllMovies().Where(m => _userManager.Users.Any(u =>
            {
                if (m.IsPlayed(u))
                    return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                return false;
            }));
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return GetAllMovies(user).Where(m =>
            {
                if (m.IsPlayed(user))
                    return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                return false;
            });
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByAllUsers()
        {
            return GetAllEpisodes().Where(s => _userManager.Users.Any(u =>
            {
                if (s.IsPlayed(u))
                    return _userDataManager.GetUserData(u, s).LastPlayedDate.HasValue;
                return false;
            }));
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return GetAllEpisodes(user).Where(m =>
            {
                if (m.IsPlayed(user))
                    return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                return false;
            });
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
