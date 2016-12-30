using MediaBrowser.Controller.Activity;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Serialization;
using Statistics.Configuration;
using Statistics.Enum;
using Statistics.Extentions;
using Statistics.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Services;

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

        private static PluginConfiguration _pluginConfiguration
        {
            get
            {
                return Plugin.Instance.get_Configuration();
            }
        }

        public StatisticsRestfulService(ILibraryManager libraryManager, IJsonSerializer JsonSerializer, IUserManager userManager, IUserDataManager userDataManager, IActivityManager activityManager)
        {
            this._libraryManager = libraryManager;
            this._JsonSerializer = JsonSerializer;
            this._userManager = userManager;
            this._userDataManager = userDataManager;
            this._activityManager = activityManager;
        }

        public object Get(UserStatistics request)
        {
            try
            {
                User userById = this._userManager.GetUserById(request.Id);
                if (userById == null)
                    return (object)null;
                StatViewModel statViewModel = new StatViewModel();
                statViewModel.MovieTotal = this.GetTotalMovies(userById);
                statViewModel.EpisodeTotal = this.GetTotalEpisodes(userById);
                statViewModel.ShowTotal = this.GetTotalShows(userById);
                ValueGroup valueGroup1 = new ValueGroup();
                valueGroup1.MainValue = "Top Movie genres";
                string str1 = string.Join(", ", (IEnumerable<string>)this.GetTopGenres(RequestTypeEnum.Movies, userById));
                valueGroup1.SubValue = str1;
                statViewModel.TopMovieGenres = valueGroup1;
                ValueGroup valueGroup2 = new ValueGroup();
                valueGroup2.MainValue = "Top Show genres";
                string str2 = string.Join(", ", (IEnumerable<string>)this.GetTopGenres(RequestTypeEnum.Shows, userById));
                valueGroup2.SubValue = str2;
                statViewModel.TopShowGenres = valueGroup2;
                ValueGroup valueGroup3 = new ValueGroup();
                valueGroup3.MainValue = "last seen movies";
                string str3 = string.Join<MovieViewModel>("<br/>", (IEnumerable<MovieViewModel>)this.GetLastViewed(RequestTypeEnum.Movies, userById));
                valueGroup3.SubValue = str3;
                statViewModel.MovieLastViewed = valueGroup3;
                ValueGroup valueGroup4 = new ValueGroup();
                valueGroup4.MainValue = "last seen shows";
                string str4 = string.Join<MovieViewModel>("<br/>", (IEnumerable<MovieViewModel>)this.GetLastViewed(RequestTypeEnum.Shows, userById));
                valueGroup4.SubValue = str4;
                statViewModel.ShowLastViewed = valueGroup4;
                ValueGroup valueGroup5 = new ValueGroup();
                valueGroup5.MainValue = "Movies watched";
                string str5 = this.GetPlayedViewTime(RequestTypeEnum.Movies, userById).ToString();
                valueGroup5.SubValue = str5;
                statViewModel.MoviePlayedViewTime = valueGroup5;
                ValueGroup valueGroup6 = new ValueGroup();
                valueGroup6.MainValue = "Shows watched";
                string str6 = this.GetPlayedViewTime(RequestTypeEnum.Shows, userById).ToString();
                valueGroup6.SubValue = str6;
                statViewModel.ShowPlayedViewTime = valueGroup6;
                ValueGroup valueGroup7 = new ValueGroup();
                valueGroup7.MainValue = "Total watched";
                string str7 = this.GetPlayedViewTime(RequestTypeEnum.All, userById).ToString();
                valueGroup7.SubValue = str7;
                statViewModel.TotalPlayedViewTime = valueGroup7;
                ValueGroup valueGroup8 = new ValueGroup();
                valueGroup8.MainValue = "Total movies time";
                string str8 = this.GetViewTime(RequestTypeEnum.Movies, userById).ToString();
                valueGroup8.SubValue = str8;
                statViewModel.MovieViewTime = valueGroup8;
                ValueGroup valueGroup9 = new ValueGroup();
                valueGroup9.MainValue = "Total shows time";
                string str9 = this.GetViewTime(RequestTypeEnum.Shows, userById).ToString();
                valueGroup9.SubValue = str9;
                statViewModel.ShowViewTime = valueGroup9;
                ValueGroup valueGroup10 = new ValueGroup();
                valueGroup10.MainValue = "Total time";
                string str10 = this.GetViewTime(RequestTypeEnum.All, userById).ToString();
                valueGroup10.SubValue = str10;
                statViewModel.TotalViewTime = valueGroup10;
                ValueGroup topYears = this.GetTopYears(userById);
                statViewModel.TopYears = topYears;
                return (object)this._JsonSerializer.SerializeToString((object)statViewModel);
            }
            catch (Exception ex)
            {
                return (object)this._JsonSerializer.SerializeToString((object)new
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
                StatViewModel statViewModel = new StatViewModel();
                statViewModel.MovieTotal = this.GetTotalMovies((User)null);
                statViewModel.EpisodeTotal = this.GetTotalEpisodes((User)null);
                statViewModel.ShowTotal = this.GetTotalShows((User)null);
                ValueGroup valueGroup1 = new ValueGroup();
                valueGroup1.MainValue = "Top Movie genres";
                string str1 = string.Join(", ", (IEnumerable<string>)this.GetTopGenres(RequestTypeEnum.Movies, (User)null));
                valueGroup1.SubValue = str1;
                statViewModel.TopMovieGenres = valueGroup1;
                ValueGroup valueGroup2 = new ValueGroup();
                valueGroup2.MainValue = "Top Show genres";
                string str2 = string.Join(", ", (IEnumerable<string>)this.GetTopGenres(RequestTypeEnum.Shows, (User)null));
                valueGroup2.SubValue = str2;
                statViewModel.TopShowGenres = valueGroup2;
                ValueGroup valueGroup3 = new ValueGroup();
                valueGroup3.MainValue = "last seen movies";
                string str3 = string.Join<MovieViewModel>("<br/>", (IEnumerable<MovieViewModel>)this.GetLastViewed(RequestTypeEnum.Movies, (User)null));
                valueGroup3.SubValue = str3;
                statViewModel.MovieLastViewed = valueGroup3;
                ValueGroup valueGroup4 = new ValueGroup();
                valueGroup4.MainValue = "last seen shows";
                string str4 = string.Join<MovieViewModel>("<br/>", (IEnumerable<MovieViewModel>)this.GetLastViewed(RequestTypeEnum.Shows, (User)null));
                valueGroup4.SubValue = str4;
                statViewModel.ShowLastViewed = valueGroup4;
                ValueGroup valueGroup5 = new ValueGroup();
                valueGroup5.MainValue = "Movies watched";
                string str5 = this.GetPlayedViewTime(RequestTypeEnum.Movies, (User)null).ToString();
                valueGroup5.SubValue = str5;
                statViewModel.MoviePlayedViewTime = valueGroup5;
                ValueGroup valueGroup6 = new ValueGroup();
                valueGroup6.MainValue = "Show watched";
                string str6 = this.GetPlayedViewTime(RequestTypeEnum.Shows, (User)null).ToString();
                valueGroup6.SubValue = str6;
                statViewModel.ShowPlayedViewTime = valueGroup6;
                ValueGroup valueGroup7 = new ValueGroup();
                valueGroup7.MainValue = "Total watched";
                string str7 = this.GetPlayedViewTime(RequestTypeEnum.All, (User)null).ToString();
                valueGroup7.SubValue = str7;
                statViewModel.TotalPlayedViewTime = valueGroup7;
                ValueGroup valueGroup8 = new ValueGroup();
                valueGroup8.MainValue = "Total movies time";
                string str8 = this.GetViewTime(RequestTypeEnum.Movies, (User)null).ToString();
                valueGroup8.SubValue = str8;
                statViewModel.MovieViewTime = valueGroup8;
                ValueGroup valueGroup9 = new ValueGroup();
                valueGroup9.MainValue = "Total show time";
                string str9 = this.GetViewTime(RequestTypeEnum.Shows, (User)null).ToString();
                valueGroup9.SubValue = str9;
                statViewModel.ShowViewTime = valueGroup9;
                ValueGroup valueGroup10 = new ValueGroup();
                valueGroup10.MainValue = "Total time";
                string str10 = this.GetViewTime(RequestTypeEnum.All, (User)null).ToString();
                valueGroup10.SubValue = str10;
                statViewModel.TotalViewTime = valueGroup10;
                ValueGroup topYears = this.GetTopYears((User)null);
                statViewModel.TopYears = topYears;
                return (object)this._JsonSerializer.SerializeToString((object)statViewModel);
            }
            catch (Exception ex)
            {
                return (object)this._JsonSerializer.SerializeToString((object)new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private ValueGroup GetTotalMovies(User user = null)
        {
            List<Movie> list = _libraryManager.RootFolder.Children.OfType<Movie>().ToList<Movie>();
            string str = "";
            if (user != null)
            {
                str = string.Format("{0} watched", (object)((IEnumerable<Movie>)list).Count<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsPlayed(user))));
                list = ((IEnumerable<Movie>)list).Where<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsVisible(user))).ToList<Movie>();
            }
            return new ValueGroup()
            {
                MainValue = string.Format("{0} movies", (object)list.Count),
                SubValue = str
            };
        }

        private ValueGroup GetTotalEpisodes(User user = null)
        {
            List<Episode> list = _libraryManager.RootFolder.Children.OfType<Episode>().ToList<Episode>();
            string str = "";
            if (user != null)
            {
                str = string.Format("{0} watched", (object)((IEnumerable<Episode>)list).Count<Episode>((Func<Episode, bool>)(e => ((BaseItem)e).IsPlayed(user))));
                list = ((IEnumerable<Episode>)list).Where<Episode>((Func<Episode, bool>)(e => ((BaseItem)e).IsVisible(user))).ToList<Episode>();
            }
            return new ValueGroup()
            {
                MainValue = string.Format("{0} episodes", (object)list.Count),
                SubValue = str
            };
        }

        private ValueGroup GetTotalShows(User user = null)
        {
            List<Series> list = _libraryManager.RootFolder.Children.OfType<Series>().ToList<Series>();
            string str = "";
            if (user != null)
            {
                str = string.Format("{0} completed", (object)((IEnumerable<Series>)list).Count<Series>((Func<Series, bool>)(s =>
                {
                    if (s.GetEpisodes(user).Any<Episode>())
                        return s.GetEpisodes(user).All<Episode>((Func<Episode, bool>)(e => ((BaseItem)e).IsPlayed(user)));
                    return false;
                })));
                list = ((IEnumerable<Series>)list).Where<Series>((Func<Series, bool>)(m => ((BaseItem)m).IsVisible(user))).ToList<Series>();
            }
            return new ValueGroup()
            {
                MainValue = string.Format("{0} shows", (object)list.Count),
                SubValue = str
            };
        }

        private List<string> GetTopGenres(RequestTypeEnum type, User user = null)
        {
            Dictionary<string, int> source = new Dictionary<string, int>();
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    List<Movie> list1 = _libraryManager.RootFolder.Children.OfType<Movie>().ToList<Movie>();
                    if (user != null)
                        list1 = ((IEnumerable<Movie>)list1).Where<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsVisible(user))).ToList<Movie>();
                    using (List<string>.Enumerator enumerator = ((IEnumerable<Movie>)list1).SelectMany<Movie, string>((Func<Movie, IEnumerable<string>>)(m => (IEnumerable<string>)((BaseItem)m).get_Genres())).Distinct<string>().ToList<string>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string genre = enumerator.Current;
                            int num = ((IEnumerable<Movie>)list1).Count<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).get_Genres().Contains(genre)));
                            source.Add(genre, num);
                        }
                        break;
                    }
                case RequestTypeEnum.Shows:
                    List<Series> list2 = _libraryManager.RootFolder.Children.OfType<Series>().ToList<Series>();
                    if (user != null)
                        list2 = ((IEnumerable<Series>)list2).Where<Series>((Func<Series, bool>)(e => ((BaseItem)e).IsVisible(user))).ToList<Series>();
                    using (List<string>.Enumerator enumerator = ((IEnumerable<Series>)list2).SelectMany<Series, string>((Func<Series, IEnumerable<string>>)(m => (IEnumerable<string>)((BaseItem)m).get_Genres())).Distinct<string>().ToList<string>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            string genre = enumerator.Current;
                            int num = ((IEnumerable<Series>)list2).Count<Series>((Func<Series, bool>)(m => ((BaseItem)m).get_Genres().Contains(genre)));
                            source.Add(genre, num);
                        }
                        break;
                    }
            }
            return source.OrderByDescending<KeyValuePair<string, int>, int>((Func<KeyValuePair<string, int>, int>)(g => g.Value)).Take<KeyValuePair<string, int>>(5).Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>)(g => g.Key)).ToList<string>();
        }

        private List<MovieViewModel> GetLastViewed(RequestTypeEnum type, User user = null)
        {
            List<MovieViewModel> movieViewModelList = new List<MovieViewModel>();
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    List<Movie> list1 = ((IEnumerable<Movie>)(user == null ? this.GetAllViewedMoviesByAllUsers() : this.GetAllViewedMoviesByUser(user)).OrderByDescending<Movie, DateTime?>((Func<Movie, DateTime?>)(m => this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                    {
                        if (((BaseItem)m).IsPlayed(u))
                            return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                        return false;
                    }))), (IHasUserData)m).get_LastPlayedDate()))).Take<Movie>(5).ToList<Movie>();
                    movieViewModelList.AddRange(((IEnumerable<Movie>)list1).Select<Movie, MovieViewModel>((Func<Movie, MovieViewModel>)(item =>
                    {
                        User user1 = user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                        {
                            if (((BaseItem)item).IsPlayed(u))
                                return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)item).get_LastPlayedDate().HasValue;
                            return false;
                        }));
                        return new MovieViewModel()
                        {
                            Name = ((BaseItem)item).get_Name(),
                            Played = this._userDataManager.GetUserData((IHasUserData)user1, (IHasUserData)item).get_LastPlayedDate() ?? DateTime.MinValue,
                            UserName = ((BaseItem)user1).get_Name()
                        };
                    })));
                    break;
                case RequestTypeEnum.Shows:
                    List<Episode> list2 = ((IEnumerable<Episode>)(user == null ? this.GetAllViewedEpisodesByAllUsers() : this.GetAllViewedEpisodesByUser(user)).OrderByDescending<Episode, DateTime?>((Func<Episode, DateTime?>)(m => this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                    {
                        if (((BaseItem)m).IsPlayed(u))
                            return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                        return false;
                    }))), (IHasUserData)m).get_LastPlayedDate()))).Take<Episode>(5).ToList<Episode>();
                    movieViewModelList.AddRange(((IEnumerable<Episode>)list2).Select<Episode, MovieViewModel>((Func<Episode, MovieViewModel>)(item =>
                    {
                        User user1 = user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                        {
                            if (((BaseItem)item).IsPlayed(u))
                                return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)item).get_LastPlayedDate().HasValue;
                            return false;
                        }));
                        return new MovieViewModel()
                        {
                            Name = string.Format("{0} - S{1}xE{2} - {3}", item.SeriesName, (object)item.get_AiredSeasonNumber(), (object)((BaseItem)item).get_IndexNumber(), (object)((BaseItem)item).get_Name()),
                            Played = this._userDataManager.GetUserData((IHasUserData)user1, (IHasUserData)item).get_LastPlayedDate() ?? DateTime.MinValue,
                            UserName = user1.Name
                        };
                    })));
                    break;
            }
            return movieViewModelList;
        }

        private RunTime GetPlayedViewTime(RequestTypeEnum type, User user = null)
        {
            RunTime runTime = new RunTime(new TimeSpan(0L), VideoStateEnum.Watched);
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    List<Movie> movieList = new List<Movie>();
                    using (List<Movie>.Enumerator enumerator = (user == null ? ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => this._userManager.get_Users().Any<User>(new Func<User, bool>(((BaseItem)m).IsPlayed)))).ToList<Movie>() : ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsPlayed(user))).ToList<Movie>()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Movie current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.RunTimeTicks ?? 0));
                        }
                        break;
                    }
                case RequestTypeEnum.Shows:
                    using (List<Episode>.Enumerator enumerator = (user == null ? ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().Where<Episode>((Func<Episode, bool>)(m => this._userManager.get_Users().Any<User>(new Func<User, bool>(((BaseItem)m).IsPlayed)))).ToList<Episode>() : ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().Where<Episode>((Func<Episode, bool>)(m => ((BaseItem)m).IsPlayed(user))).ToList<Episode>()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Episode current = enumerator.Current;
                            runTime.Add(new TimeSpan(((BaseItem)current).get_RunTimeTicks() ?? 0L));
                        }
                        break;
                    }
                case RequestTypeEnum.All:
                    using (List<BaseItem>.Enumerator enumerator = (user == null ? this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m => this._userManager.get_Users().Any<User>(new Func<User, bool>(m.IsPlayed)))).ToList<BaseItem>() : this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m => m.IsPlayed(user))).ToList<BaseItem>()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            BaseItem current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.get_RunTimeTicks() ?? 0L));
                        }
                        break;
                    }
            }
            return runTime;
        }

        private RunTime GetViewTime(RequestTypeEnum type, User user = null)
        {
            RunTime runTime = new RunTime(new TimeSpan(0L), VideoStateEnum.All);
            switch (type)
            {
                case RequestTypeEnum.Movies:
                    using (List<Movie>.Enumerator enumerator = (user == null ? ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().ToList<Movie>() : ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsVisible(user))).ToList<Movie>()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Movie current = enumerator.Current;
                            runTime.Add(new TimeSpan(((BaseItem)current).get_RunTimeTicks() ?? 0L));
                        }
                        break;
                    }
                case RequestTypeEnum.Shows:
                    using (List<Episode>.Enumerator enumerator = (user == null ? ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().ToList<Episode>() : ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().Where<Episode>((Func<Episode, bool>)(m => ((BaseItem)m).IsVisible(user))).ToList<Episode>()).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Episode current = enumerator.Current;
                            runTime.Add(new TimeSpan(((BaseItem)current).get_RunTimeTicks() ?? 0L));
                        }
                        break;
                    }
                case RequestTypeEnum.All:
                    using (IEnumerator<BaseItem> enumerator = (user == null ? this.GetAllBaseItems() : (IEnumerable<BaseItem>)this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m => m.IsVisible(user))).ToList<BaseItem>()).GetEnumerator())
                    {
                        while (((IEnumerator)enumerator).MoveNext())
                        {
                            BaseItem current = enumerator.Current;
                            runTime.Add(new TimeSpan(current.get_RunTimeTicks() ?? 0L));
                        }
                        break;
                    }
            }
            return runTime;
        }

        private ValueGroup GetTopYears(User user = null)
        {
            List<Movie> movieList = user == null ? ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => this._userManager.get_Users().Any<User>(new Func<User, bool>(((BaseItem)m).IsPlayed)))).ToList<Movie>() : ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => ((BaseItem)m).IsPlayed(user))).ToList<Movie>();
            List<int> list = ((IEnumerable<Movie>)movieList).Select<Movie, int>((Func<Movie, int>)(m => ((BaseItem)m).get_ProductionYear() ?? 0)).Distinct<int>().ToList<int>();
            Dictionary<int, int> source = new Dictionary<int, int>();
            foreach (int num1 in list)
            {
                int year = num1;
                int num2 = ((IEnumerable<Movie>)movieList).Count<Movie>((Func<Movie, bool>)(m => (((BaseItem)m).get_ProductionYear() ?? 0) == year));
                source.Add(year, num2);
            }
            ValueGroup valueGroup = new ValueGroup();
            valueGroup.MainValue = "top movie years";
            string str = string.Join<int>(", ", (IEnumerable<int>)source.OrderByDescending<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(g => g.Value)).Take<KeyValuePair<int, int>>(5).Select<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(g => g.Key)).ToList<int>());
            valueGroup.SubValue = str;
            return valueGroup;
        }

        public object Get(GetBackground request)
        {
            List<string> stringList1 = new List<string>();
            Random random = new Random();
            for (int index1 = 0; index1 < int.Parse(request.Count); ++index1)
            {
                List<Movie> list = ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().ToList<Movie>();
                int index2;
                Guid id;
                int num;
                do
                {
                    index2 = random.Next(0, list.Count - 1);
                    if (((BaseItem)list[index2]).GetImages((ImageType)2).Any<ItemImageInfo>())
                    {
                        List<string> stringList2 = stringList1;
                        id = ((BaseItem)list[index2]).get_Id();
                        string str = id.ToString();
                        num = stringList2.Contains(str) ? 1 : 0;
                    }
                    else
                        num = 0;
                }
                while (num != 0);
                List<string> stringList3 = stringList1;
                id = ((BaseItem)list[index2]).get_Id();
                string str1 = id.ToString();
                stringList3.Add(str1);
            }
            return (object)stringList1.ToJSON();
        }

        public object Get(ViewChart request)
        {
            try
            {
                User user = (User)null;
                if (request.Id != "0")
                    user = this._userManager.GetUserById(request.Id);
                Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
                List<GraphValue> graphValueList = new List<GraphValue>();
                List<KeyValuePair<int, int>> keyValuePairList = new List<KeyValuePair<int, int>>();
                IEnumerable<BaseItem> source1 = user == null ? this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m => this._userManager.get_Users().Any<User>((Func<User, bool>)(u =>
                {
                    if (m.IsPlayed(u))
                        return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                    return false;
                })))) : this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m =>
                {
                    if (m.IsPlayed(user))
                        return this._userDataManager.GetUserData((IHasUserData)user, (IHasUserData)m).get_LastPlayedDate().HasValue;
                    return false;
                }));
                switch (request.TimeRange)
                {
                    case TimeRangeEnum.Monthly:
                        IEnumerable<IGrouping<int, BaseItem>> source2 = source1.GroupBy<BaseItem, int>((Func<BaseItem, int>)(m => cal.GetMonth(this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                        {
                            if (m.IsPlayed(u))
                                return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                            return false;
                        }))), (IHasUserData)m).get_LastPlayedDate().Value)));
                        Func<IGrouping<int, BaseItem>, int> func1 = (Func<IGrouping<int, BaseItem>, int>)(k => k.Key);
                        Func<IGrouping<int, BaseItem>, int> keySelector1;
                        List<KeyValuePair<int, int>> list1 = this.CalculateTimeRange(source2.ToDictionary<IGrouping<int, BaseItem>, int, int>(keySelector1, (Func<IGrouping<int, BaseItem>, int>)(g => ((IEnumerable<BaseItem>)g).ToList<BaseItem>().Count)), request.TimeRange).ToList<KeyValuePair<int, int>>();
                        int count1 = list1.Count > 12 ? list1.Count - 12 : 0;
                        graphValueList.AddRange(list1.Skip<KeyValuePair<int, int>>(count1).Select<KeyValuePair<int, int>, GraphValue>((Func<KeyValuePair<int, int>, GraphValue>)(item => new GraphValue((object)this._cul.DateTimeFormat.GetAbbreviatedMonthName(item.Key), (object)item.Value))));
                        break;
                    case TimeRangeEnum.Weekly:
                        IEnumerable<IGrouping<int, BaseItem>> source3 = source1.GroupBy<BaseItem, int>((Func<BaseItem, int>)(m => cal.GetWeekOfYear(this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                        {
                            if (m.IsPlayed(u))
                                return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                            return false;
                        }))), (IHasUserData)m).get_LastPlayedDate().Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday)));
                        Func<IGrouping<int, BaseItem>, int> func2 = (Func<IGrouping<int, BaseItem>, int>)(k => k.Key);
                        Func<IGrouping<int, BaseItem>, int> keySelector2;
                        List<KeyValuePair<int, int>> list2 = this.CalculateTimeRange(source3.ToDictionary<IGrouping<int, BaseItem>, int, int>(keySelector2, (Func<IGrouping<int, BaseItem>, int>)(g => ((IEnumerable<BaseItem>)g).ToList<BaseItem>().Count)), request.TimeRange).ToList<KeyValuePair<int, int>>();
                        int count2 = list2.Count > 20 ? list2.Count - 20 : 0;
                        graphValueList.AddRange(list2.Skip<KeyValuePair<int, int>>(count2).Select<KeyValuePair<int, int>, GraphValue>((Func<KeyValuePair<int, int>, GraphValue>)(item => new GraphValue((object)item.Key, (object)item.Value))));
                        break;
                    case TimeRangeEnum.Daily:
                        IOrderedEnumerable<IGrouping<int, BaseItem>> orderedEnumerable = source1.GroupBy<BaseItem, int>((Func<BaseItem, int>)(m => cal.GetDayOfYear(this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                        {
                            if (m.IsPlayed(u))
                                return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                            return false;
                        }))), (IHasUserData)m).get_LastPlayedDate().Value))).OrderByDescending<IGrouping<int, BaseItem>, int>((Func<IGrouping<int, BaseItem>, int>)(x => x.Key));
                        Func<IGrouping<int, BaseItem>, int> func3 = (Func<IGrouping<int, BaseItem>, int>)(k => k.Key);
                        Func<IGrouping<int, BaseItem>, int> keySelector3;
                        List<KeyValuePair<int, int>> list3 = this.CalculateTimeRange(((IEnumerable<IGrouping<int, BaseItem>>)orderedEnumerable).ToDictionary<IGrouping<int, BaseItem>, int, int>(keySelector3, (Func<IGrouping<int, BaseItem>, int>)(g => ((IEnumerable<BaseItem>)g).ToList<BaseItem>().Count)), request.TimeRange).ToList<KeyValuePair<int, int>>();
                        int count3 = list3.Count > 7 ? list3.Count - 7 : 0;
                        graphValueList.AddRange(list3.Skip<KeyValuePair<int, int>>(count3).Select<KeyValuePair<int, int>, GraphValue>((Func<KeyValuePair<int, int>, GraphValue>)(item =>
                        {
                            DateTimeFormatInfo dateTimeFormat = this._cul.DateTimeFormat;
                            DateTime dateTime = DateTime.Now;
                            dateTime = new DateTime(dateTime.Year, 1, 1);
                            dateTime = dateTime.AddDays((double)(item.Key - 1));
                            int dayOfWeek = (int)dateTime.DayOfWeek;
                            return new GraphValue((object)dateTimeFormat.GetAbbreviatedDayName((DayOfWeek)dayOfWeek), (object)item.Value);
                        })));
                        break;
                }
                return (object)this._JsonSerializer.SerializeToString((object)graphValueList.ToJSON());
            }
            catch (Exception ex)
            {
                return (object)this._JsonSerializer.SerializeToString((object)new
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
                User user = (User)null;
                if (request.Id != "0")
                    user = this._userManager.GetUserById(request.Id);
                return (object)this._JsonSerializer.SerializeToString((object)((IEnumerable<IGrouping<DayOfWeek, BaseItem>>)(user == null ? this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m => this._userManager.get_Users().Any<User>((Func<User, bool>)(u =>
                {
                    if (m.IsPlayed(u))
                        return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                    return false;
                })))) : this.GetAllBaseItems().Where<BaseItem>((Func<BaseItem, bool>)(m =>
                {
                    if (m.IsPlayed(user))
                        return this._userDataManager.GetUserData((IHasUserData)user, (IHasUserData)m).get_LastPlayedDate().HasValue;
                    return false;
                }))).GroupBy<BaseItem, DayOfWeek>((Func<BaseItem, DayOfWeek>)(m => this._userDataManager.GetUserData((IHasUserData)(user ?? this._userManager.get_Users().First<User>((Func<User, bool>)(u =>
                {
                    if (m.IsPlayed(u))
                        return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                    return false;
                }))), (IHasUserData)m).get_LastPlayedDate().Value.DayOfWeek)).OrderBy<IGrouping<DayOfWeek, BaseItem>, DayOfWeek>((Func<IGrouping<DayOfWeek, BaseItem>, DayOfWeek>)(d => d.Key))).ToDictionary<IGrouping<DayOfWeek, BaseItem>, string, int>((Func<IGrouping<DayOfWeek, BaseItem>, string>)(k => this._cul.DateTimeFormat.GetAbbreviatedDayName(k.Key)), (Func<IGrouping<DayOfWeek, BaseItem>, int>)(g => ((IEnumerable<BaseItem>)((IEnumerable<BaseItem>)g).ToList<BaseItem>()).Count<BaseItem>())).Select<KeyValuePair<string, int>, GraphValue>((Func<KeyValuePair<string, int>, GraphValue>)(item => new GraphValue((object)item.Key, (object)item.Value))).ToList<GraphValue>().ToJSON());
            }
            catch (Exception ex)
            {
                return (object)this._JsonSerializer.SerializeToString((object)new
                {
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        private IEnumerable<BaseItem> GetAllBaseItems()
        {
            return ((IEnumerable<BaseItem>)((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>()).Union<BaseItem>(((IEnumerable)((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>()).Cast<BaseItem>());
        }

        private IEnumerable<Movie> GetAllViewedMoviesByAllUsers()
        {
            return ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m => this._userManager.get_Users().Any<User>((Func<User, bool>)(u =>
            {
                if (((BaseItem)m).IsPlayed(u))
                    return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)m).get_LastPlayedDate().HasValue;
                return false;
            }))));
        }

        private IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Movie>().Where<Movie>((Func<Movie, bool>)(m =>
            {
                if (((BaseItem)m).IsPlayed(user))
                    return this._userDataManager.GetUserData((IHasUserData)user, (IHasUserData)m).get_LastPlayedDate().HasValue;
                return false;
            }));
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByAllUsers()
        {
            return ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().Where<Episode>((Func<Episode, bool>)(s => this._userManager.get_Users().Any<User>((Func<User, bool>)(u =>
            {
                if (((BaseItem)s).IsPlayed(u))
                    return this._userDataManager.GetUserData((IHasUserData)u, (IHasUserData)s).get_LastPlayedDate().HasValue;
                return false;
            }))));
        }

        private IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return ((IEnumerable)((Folder)this._libraryManager.get_RootFolder()).get_RecursiveChildren()).OfType<Episode>().Where<Episode>((Func<Episode, bool>)(m =>
            {
                if (((BaseItem)m).IsPlayed(user))
                    return this._userDataManager.GetUserData((IHasUserData)user, (IHasUserData)m).get_LastPlayedDate().HasValue;
                return false;
            }));
        }

        private IEnumerable<KeyValuePair<int, int>> CalculateTimeRange(Dictionary<int, int> list, TimeRangeEnum type)
        {
            if (list == null)
                return (IEnumerable<KeyValuePair<int, int>>)new List<KeyValuePair<int, int>>();
            int key1 = 1;
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> keyValuePair in (IEnumerable<KeyValuePair<int, int>>)list.OrderBy<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(k => k.Key)))
            {
                for (; keyValuePair.Key != key1; ++key1)
                    dictionary.Add(key1, 0);
                ++key1;
            }
            foreach (KeyValuePair<int, int> keyValuePair in dictionary)
                list.Add(keyValuePair.Key, keyValuePair.Value);
            List<KeyValuePair<int, int>> source = new List<KeyValuePair<int, int>>();
            Calendar calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
            int now = 0;
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
            while (list.OrderBy<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(x => x.Key)).Last<KeyValuePair<int, int>>().Key < now)
            {
                int key2 = list.OrderBy<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(x => x.Key)).Last<KeyValuePair<int, int>>().Key + 1;
                list.Add(key2, 0);
            }
            source.AddRange(list.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(k => k.Key <= now)).OrderByDescending<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(k => k.Key)).Select<KeyValuePair<int, int>, KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, KeyValuePair<int, int>>)(item => new KeyValuePair<int, int>(item.Key, item.Value))));
            source.AddRange(list.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(k => k.Key > now)).OrderByDescending<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(k => k.Key)).Select<KeyValuePair<int, int>, KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, KeyValuePair<int, int>>)(item => new KeyValuePair<int, int>(item.Key, item.Value))));
            return source.Take<KeyValuePair<int, int>>(20).Reverse<KeyValuePair<int, int>>();
        }
    }
}
