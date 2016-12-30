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
        private readonly IJsonSerializer _JsonSerializer;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly IActivityManager _activityManager;

        private static PluginConfiguration _pluginConfiguration => Plugin.Instance.Configuration;

        public StatisticsRestfulService(ILibraryManager libraryManager, IJsonSerializer JsonSerializer, IUserManager userManager, IUserDataManager userDataManager, IActivityManager activityManager)
        {
            _libraryManager = libraryManager;
            _JsonSerializer = JsonSerializer;
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
                var valueGroup1 = new ValueGroup {MainValue = "Top Movie genres"};
                var str1 = string.Join(", ", GetTopGenres(RequestTypeEnum.Movies, userById));
                valueGroup1.SubValue = str1;
                statViewModel.TopMovieGenres = valueGroup1;
                var valueGroup2 = new ValueGroup {MainValue = "Top Show genres"};
                var str2 = string.Join(", ", GetTopGenres(RequestTypeEnum.Shows, userById));
                valueGroup2.SubValue = str2;
                statViewModel.TopShowGenres = valueGroup2;
                var valueGroup3 = new ValueGroup {MainValue = "last seen movies"};
                var str3 = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Movies, userById));
                valueGroup3.SubValue = str3;
                statViewModel.MovieLastViewed = valueGroup3;
                var valueGroup4 = new ValueGroup {MainValue = "last seen shows"};
                var str4 = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Shows, userById));
                valueGroup4.SubValue = str4;
                statViewModel.ShowLastViewed = valueGroup4;
                var valueGroup5 = new ValueGroup {MainValue = "Movies watched"};
                var str5 = GetPlayedViewTime(RequestTypeEnum.Movies, userById).ToString();
                valueGroup5.SubValue = str5;
                statViewModel.MoviePlayedViewTime = valueGroup5;
                var valueGroup6 = new ValueGroup {MainValue = "Shows watched"};
                var str6 = GetPlayedViewTime(RequestTypeEnum.Shows, userById).ToString();
                valueGroup6.SubValue = str6;
                statViewModel.ShowPlayedViewTime = valueGroup6;
                var valueGroup7 = new ValueGroup {MainValue = "Total watched"};
                var str7 = GetPlayedViewTime(RequestTypeEnum.All, userById).ToString();
                valueGroup7.SubValue = str7;
                statViewModel.TotalPlayedViewTime = valueGroup7;
                var valueGroup8 = new ValueGroup {MainValue = "Total movies time"};
                var str8 = GetViewTime(RequestTypeEnum.Movies, userById).ToString();
                valueGroup8.SubValue = str8;
                statViewModel.MovieViewTime = valueGroup8;
                var valueGroup9 = new ValueGroup {MainValue = "Total shows time"};
                var str9 = GetViewTime(RequestTypeEnum.Shows, userById).ToString();
                valueGroup9.SubValue = str9;
                statViewModel.ShowViewTime = valueGroup9;
                var valueGroup10 = new ValueGroup {MainValue = "Total time"};
                var str10 = GetViewTime(RequestTypeEnum.All, userById).ToString();
                valueGroup10.SubValue = str10;
                statViewModel.TotalViewTime = valueGroup10;
                var topYears = GetTopYears(userById);
                statViewModel.TopYears = topYears;
                return _JsonSerializer.SerializeToString(statViewModel);
            }
            catch (Exception ex)
            {
                return _JsonSerializer.SerializeToString(new
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
                var valueGroup1 = new ValueGroup {MainValue = "Top Movie genres"};
                var str1 = string.Join(", ", GetTopGenres(RequestTypeEnum.Movies));
                valueGroup1.SubValue = str1;
                statViewModel.TopMovieGenres = valueGroup1;
                var valueGroup2 = new ValueGroup {MainValue = "Top Show genres"};
                var str2 = string.Join(", ", GetTopGenres(RequestTypeEnum.Shows));
                valueGroup2.SubValue = str2;
                statViewModel.TopShowGenres = valueGroup2;
                var valueGroup3 = new ValueGroup {MainValue = "last seen movies"};
                var str3 = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Movies));
                valueGroup3.SubValue = str3;
                statViewModel.MovieLastViewed = valueGroup3;
                var valueGroup4 = new ValueGroup {MainValue = "last seen shows"};
                var str4 = string.Join("<br/>", GetLastViewed(RequestTypeEnum.Shows));
                valueGroup4.SubValue = str4;
                statViewModel.ShowLastViewed = valueGroup4;
                var valueGroup5 = new ValueGroup {MainValue = "Movies watched"};
                var str5 = GetPlayedViewTime(RequestTypeEnum.Movies).ToString();
                valueGroup5.SubValue = str5;
                statViewModel.MoviePlayedViewTime = valueGroup5;
                var valueGroup6 = new ValueGroup {MainValue = "Show watched"};
                var str6 = GetPlayedViewTime(RequestTypeEnum.Shows).ToString();
                valueGroup6.SubValue = str6;
                statViewModel.ShowPlayedViewTime = valueGroup6;
                var valueGroup7 = new ValueGroup {MainValue = "Total watched"};
                var str7 = GetPlayedViewTime(RequestTypeEnum.All).ToString();
                valueGroup7.SubValue = str7;
                statViewModel.TotalPlayedViewTime = valueGroup7;
                var valueGroup8 = new ValueGroup {MainValue = "Total movies time"};
                var str8 = GetViewTime(RequestTypeEnum.Movies).ToString();
                valueGroup8.SubValue = str8;
                statViewModel.MovieViewTime = valueGroup8;
                var valueGroup9 = new ValueGroup {MainValue = "Total show time"};
                var str9 = GetViewTime(RequestTypeEnum.Shows).ToString();
                valueGroup9.SubValue = str9;
                statViewModel.ShowViewTime = valueGroup9;
                var valueGroup10 = new ValueGroup {MainValue = "Total time"};
                var str10 = GetViewTime(RequestTypeEnum.All).ToString();
                valueGroup10.SubValue = str10;
                statViewModel.TotalViewTime = valueGroup10;
                var topYears = GetTopYears();
                statViewModel.TopYears = topYears;
                return _JsonSerializer.SerializeToString(statViewModel);
            }
            catch (Exception ex)
            {
                return _JsonSerializer.SerializeToString(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private ValueGroup GetTotalMovies(User user = null)
        {
            var list = _libraryManager.RootFolder.Children.OfType<Movie>().ToList();
            var str = "";
            if (user != null)
            {
                str = $"{list.Count(m => m.IsPlayed(user))} watched";
                list = list.Where(m => m.IsVisible(user)).ToList();
            }
            return new ValueGroup
            {
                MainValue = $"{list.Count} movies",
                SubValue = str
            };
        }

        private ValueGroup GetTotalEpisodes(User user = null)
        {
            var list = _libraryManager.RootFolder.Children.OfType<Episode>().ToList();
            var str = "";
            if (user != null)
            {
                str = $"{list.Count(e => e.IsPlayed(user))} watched";
                list = list.Where(e => e.IsVisible(user)).ToList();
            }
            return new ValueGroup
            {
                MainValue = $"{list.Count} episodes",
                SubValue = str
            };
        }

        private ValueGroup GetTotalShows(User user = null)
        {
            var list = _libraryManager.RootFolder.Children.OfType<Series>().ToList();
            var str = "";
            if (user != null)
            {
                str =
                    $"{list.Count(s => { if (s.GetEpisodes(user).Any()) return s.GetEpisodes(user).All(e => e.IsPlayed(user)); return false; })} completed";
                list = list.Where(m => m.IsVisible(user)).ToList();
            }
            return new ValueGroup
            {
                MainValue = $"{list.Count} shows",
                SubValue = str
            };
        }

        private List<string> GetTopGenres(RequestTypeEnum type, User user = null)
        {
            var source = new Dictionary<string, int>();
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var list1 = _libraryManager.RootFolder.Children.OfType<Movie>().ToList();
                    if (user != null)
                        list1 = list1.Where(m => m.IsVisible(user)).ToList();
                    using (var enumerator = list1.SelectMany(m => m.Genres).Distinct().ToList().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var genre = enumerator.Current;
                            var num = list1.Count(m => m.Genres.Contains(genre));
                            source.Add(genre, num);
                        }
                        break;
                    }
                case RequestTypeEnum.Shows:
                    var list2 = _libraryManager.RootFolder.Children.OfType<Series>().ToList();
                    if (user != null)
                        list2 = list2.Where(e => e.IsVisible(user)).ToList();
                    using (var enumerator = list2.SelectMany(m => m.Genres).Distinct().ToList().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var genre = enumerator.Current;
                            var num = list2.Count(m => m.Genres.Contains(genre));
                            source.Add(genre, num);
                        }
                        break;
                    }
            }
            return source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList();
        }

        private List<MovieViewModel> GetLastViewed(RequestTypeEnum type, User user = null)
        {
            var movieViewModelList = new List<MovieViewModel>();
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    var list1 = (user == null ? GetAllViewedMoviesByAllUsers() : GetAllViewedMoviesByUser(user)).OrderByDescending(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u =>
                    {
                        if (m.IsPlayed(u))
                            return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                        return false;
                    }), m).LastPlayedDate).Take(5).ToList();
                    movieViewModelList.AddRange(list1.Select(item =>
                    {
                        var user1 = user ?? _userManager.Users.First(u =>
                        {
                            if (item.IsPlayed(u))
                                return _userDataManager.GetUserData(u, item).LastPlayedDate.HasValue;
                            return false;
                        });
                        return new MovieViewModel
                        {
                            Name = item.Name,
                            Played = _userDataManager.GetUserData(user1, item).LastPlayedDate ?? DateTime.MinValue,
                            UserName = user1.Name
                        };
                    }));
                    break;
                case RequestTypeEnum.Shows:
                    var list2 = (user == null ? GetAllViewedEpisodesByAllUsers() : GetAllViewedEpisodesByUser(user))
                        .OrderByDescending(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u => {
                        if (m.IsPlayed(u))
                            return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                        return false;
                    }), m).LastPlayedDate).Take(5).ToList();
                    movieViewModelList.AddRange(list2.Select(item =>
                    {
                        var user1 = user ?? _userManager.Users.First(u =>
                        {
                            if (item.IsPlayed(u))
                                return _userDataManager.GetUserData(u, item).LastPlayedDate.HasValue;
                            return false;
                        });
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
                    var movies = user == null ? _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => _userManager.Users.Any(m.IsPlayed)).ToList() : _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => m.IsPlayed(user));
                    foreach (var movie in movies)
                    {
                        runTime.Add(new TimeSpan(movie.RunTimeTicks ?? 0));
                    }
                    break;
                case RequestTypeEnum.Shows:
                    var shows = user == null ? _libraryManager.RootFolder.Children.OfType<Episode>().Where(m => _userManager.Users.Any(((BaseItem)m).IsPlayed)).ToList() : _libraryManager.RootFolder.Children.OfType<Episode>().Where(m => m.IsPlayed(user)).ToList();
                    foreach (var show in shows)
                    {
                        runTime.Add(new TimeSpan(show.RunTimeTicks ?? 0));
                    }
                    break;
                case RequestTypeEnum.All:
                    var items = user == null ? GetAllBaseItems().Where(m => _userManager.Users.Any(m.IsPlayed)).ToList() : GetAllBaseItems().Where(m => m.IsPlayed(user)).ToList();
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
                    using (var enumerator = (user == null ? _libraryManager.RootFolder.Children.OfType<Movie>().ToList() : _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => m.IsVisible(user)).ToList()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.RunTimeTicks ?? 0L));
                        }
                        break;
                    }
                case RequestTypeEnum.Shows:
                    using (var enumerator = (user == null ? _libraryManager.RootFolder.Children.OfType<Episode>().ToList() : _libraryManager.RootFolder.Children.OfType<Episode>().Where(m => m.IsVisible(user)).ToList()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.RunTimeTicks ?? 0L));
                        }
                        break;
                    }
                case RequestTypeEnum.All:
                    using (var enumerator = (user == null ? GetAllBaseItems() : GetAllBaseItems().Where(m => m.IsVisible(user)).ToList()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.RunTimeTicks ?? 0L));
                        }
                        break;
                    }
            }
            return runTime;
        }

        private ValueGroup GetTopYears(User user = null)
        {
            var movieList = user == null ? _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => _userManager.Users.Any(((BaseItem)m).IsPlayed)).ToList() : _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => m.IsPlayed(user)).ToList();
            var list = movieList.Select(m => m.ProductionYear ?? 0).Distinct().ToList();
            var source = new Dictionary<int, int>();
            foreach (var num1 in list)
            {
                var year = num1;
                var num2 = movieList.Count(m => (m.ProductionYear ?? 0) == year);
                source.Add(year, num2);
            }
            var valueGroup = new ValueGroup {MainValue = "top movie years"};
            var str = string.Join(", ", source.OrderByDescending(g => g.Value).Take(5).Select(g => g.Key).ToList());
            valueGroup.SubValue = str;
            return valueGroup;
        }

        public object Get(GetBackground request)
        {
            var stringList1 = new List<string>();
            var random = new Random();
            for (var index1 = 0; index1 < int.Parse(request.Count); ++index1)
            {
                var list = _libraryManager.RootFolder.Children.OfType<Movie>().ToList();
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
                        var str = id.ToString();
                        num = stringList2.Contains(str) ? 1 : 0;
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

                if (DateTimeFormatInfo.CurrentInfo != null) {
                    var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
                    var graphValueList = new List<GraphValue>();
                    var keyValuePairList = new List<KeyValuePair<int, int>>();
                    var source1 = user == null ? GetAllBaseItems().Where(m => _userManager.Users.Any(u =>
                    {
                        if (m.IsPlayed(u))
                            return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                        return false;
                    })) : GetAllBaseItems().Where(m =>
                    {
                        if (m.IsPlayed(user))
                            return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                        return false;
                    });
                    switch (request.TimeRange)
                    {
                        case TimeRangeEnum.Monthly:
                            var source2 = source1.GroupBy(m => cal.GetMonth(_userDataManager.GetUserData(user ?? _userManager.Users.First(u =>
                            {
                                if (m.IsPlayed(u))
                                    return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                                return false;
                            }), m).LastPlayedDate.Value));
                            Func<IGrouping<int, BaseItem>, int> func1 = k => k.Key;
                            Func<IGrouping<int, BaseItem>, int> keySelector1;
                            var list1 = CalculateTimeRange(source2.ToDictionary(keySelector1, g => g.ToList().Count), request.TimeRange).ToList();
                            var count1 = list1.Count > 12 ? list1.Count - 12 : 0;
                            graphValueList.AddRange(list1.Skip(count1).Select(item => new GraphValue(_cul.DateTimeFormat.GetAbbreviatedMonthName(item.Key), item.Value)));
                            break;
                        case TimeRangeEnum.Weekly:
                            var source3 = source1.GroupBy(m => cal.GetWeekOfYear(_userDataManager.GetUserData(user ?? _userManager.Users.First(u =>
                            {
                                if (m.IsPlayed(u))
                                    return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                                return false;
                            }), m).LastPlayedDate.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday));
                            Func<IGrouping<int, BaseItem>, int> func2 = k => k.Key;
                            Func<IGrouping<int, BaseItem>, int> keySelector2;
                            var list2 = CalculateTimeRange(source3.ToDictionary(keySelector2, g => g.ToList().Count), request.TimeRange).ToList();
                            var count2 = list2.Count > 20 ? list2.Count - 20 : 0;
                            graphValueList.AddRange(list2.Skip(count2).Select(item => new GraphValue(item.Key, item.Value)));
                            break;
                        case TimeRangeEnum.Daily:
                            var orderedEnumerable = source1.GroupBy(m => cal.GetDayOfYear(_userDataManager.GetUserData(user ?? _userManager.Users.First(u =>
                            {
                                if (m.IsPlayed(u))
                                    return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                                return false;
                            }), m).LastPlayedDate.Value)).OrderByDescending(x => x.Key);
                            Func<IGrouping<int, BaseItem>, int> func3 = k => k.Key;
                            Func<IGrouping<int, BaseItem>, int> keySelector3;
                            var list3 = CalculateTimeRange(orderedEnumerable.ToDictionary(keySelector3, g => g.ToList().Count), request.TimeRange).ToList();
                            var count3 = list3.Count > 7 ? list3.Count - 7 : 0;
                            graphValueList.AddRange(list3.Skip(count3).Select(item =>
                            {
                                var dateTimeFormat = _cul.DateTimeFormat;
                                var dateTime = DateTime.Now;
                                dateTime = new DateTime(dateTime.Year, 1, 1);
                                dateTime = dateTime.AddDays(item.Key - 1);
                                var dayOfWeek = (int)dateTime.DayOfWeek;
                                return new GraphValue(dateTimeFormat.GetAbbreviatedDayName((DayOfWeek)dayOfWeek), item.Value);
                            }));
                            break;
                    }
                    return _JsonSerializer.SerializeToString(graphValueList.ToJSON());
                }

                return _JsonSerializer.SerializeToString("");
            }
            catch (Exception ex)
            {
                return _JsonSerializer.SerializeToString(new
                {
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
                return _JsonSerializer.SerializeToString((user == null ? GetAllBaseItems().Where(m => _userManager.Users.Any(u =>
                {
                    if (m.IsPlayed(u))
                        return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                    return false;
                })) : GetAllBaseItems().Where(m =>
                {
                    if (m.IsPlayed(user))
                        return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                    return false;
                })).GroupBy(m => _userDataManager.GetUserData(user ?? _userManager.Users.First(u => {
                    if (m.IsPlayed(u))
                        return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                    return false;
                }), m).LastPlayedDate.Value.DayOfWeek).OrderBy(d => d.Key).ToDictionary(k => _cul.DateTimeFormat.GetAbbreviatedDayName(k.Key), g => g.ToList().Count()).Select(item => new GraphValue(item.Key, item.Value)).ToList().ToJSON());
            }
            catch (Exception ex)
            {
                return _JsonSerializer.SerializeToString(new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private IEnumerable<BaseItem> GetAllBaseItems()
        {
            return _libraryManager.RootFolder.Children.OfType<Movie>().Union(_libraryManager.RootFolder.Children.OfType<Episode>().Cast<BaseItem>());
        }

        private IEnumerable<Movie> GetAllViewedMoviesByAllUsers()
        {
            return _libraryManager.RootFolder.Children.OfType<Movie>().Where(m => _userManager.Users.Any(u =>
            {
                if (m.IsPlayed(u))
                    return _userDataManager.GetUserData(u, m).LastPlayedDate.HasValue;
                return false;
            }));
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return _libraryManager.RootFolder.Children.OfType<Movie>().Where(m =>
            {
                if (m.IsPlayed(user))
                    return _userDataManager.GetUserData(user, m).LastPlayedDate.HasValue;
                return false;
            });
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByAllUsers()
        {
            return _libraryManager.RootFolder.Children.OfType<Episode>().Where(s => _userManager.Users.Any(u =>
            {
                if (s.IsPlayed(u))
                    return _userDataManager.GetUserData(u, s).LastPlayedDate.HasValue;
                return false;
            }));
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return _libraryManager.RootFolder.Children.OfType<Episode>().Where(m =>
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
