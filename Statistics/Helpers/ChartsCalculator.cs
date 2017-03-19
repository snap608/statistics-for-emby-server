using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Statistics.Configuration;
using Statistics.Models;
using Statistics.Models.Chart;

namespace Statistics.Helpers
{
    public class ChartsCalculator : BaseCalculator
    {
        private readonly CultureInfo _cul = Thread.CurrentThread.CurrentCulture;

        public ChartsCalculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager) : base(userManager, libraryManager, userDataManager)
        {

        }

        #region WeekChart
        public ChartModel CalculateDayOfWeekForAllUsersChart()
        {
            var chartValues = new ChartModel(new[] 
            {
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Monday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Tuesday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Wednesday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Thursday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Friday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Saturday),
                _cul.DateTimeFormat.GetDayName(DayOfWeek.Sunday)
            });

            foreach (var user in UserManager.Users)
            {
                SetUser(user);
                var userMovies = GetAllViewedMoviesByUser();
                var userEpisodes = GetAllViewedEpisodesByUser();

                userMovies
                    .Where(x => UserDataManager.GetUserData(user, x).Played && UserDataManager.GetUserData(user, x).LastPlayedDate.HasValue)
                    .GroupBy(x => UserDataManager.GetUserData(user, x).LastPlayedDate.Value.DayOfWeek)
                    .ToDictionary(x => x.Key, x => x.Count())
                    .ToList()
                    .ForEach(x => chartValues.Week.Single(y => y.Key == _cul.DateTimeFormat.GetDayName(x.Key)).Movies += x.Value);

                userEpisodes
                    .Where(x => UserDataManager.GetUserData(user, x).Played && UserDataManager.GetUserData(user, x).LastPlayedDate.HasValue)
                    .GroupBy(x => UserDataManager.GetUserData(user, x).LastPlayedDate.Value.DayOfWeek)
                    .ToDictionary(x => x.Key, x => x.Count())
                    .ToList()
                    .ForEach(x => chartValues.Week.Single(y => y.Key == _cul.DateTimeFormat.GetDayName(x.Key)).Episodes += x.Value);

                ClearCache();
            }

            return chartValues;
        }

        #endregion


        #region HourChart



        #endregion
    }
}
