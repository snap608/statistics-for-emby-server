using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using Statistics.Configuration;
using Statistics.Helpers;

namespace Statistics.ScheduledTasks
{
    public class CalculateStatsTask : IScheduledTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;

        private static PluginConfiguration PluginConfiguration => Plugin.Instance.Configuration;

        public CalculateStatsTask(ILogManager logger,
            IJsonSerializer jsonSerializer,
            IUserManager userManager,
            IUserDataManager userDataManager,
            ILibraryManager libraryManager)
        {
            _logger = logger.GetLogger("Statistics");
            _jsonSerializer = jsonSerializer;
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;

            
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var users = _userManager.Users.ToList();

            // No users found, so stop the task
            if (users.Count == 0)
            {
                _logger.Info("No Users returned");
                return;
            }

            // clear all previously saved stats
            PluginConfiguration.UserStats = new List<UserStat>();
            PluginConfiguration.GeneralStat = new GeneralStat();
            Plugin.Instance.SaveConfiguration();
            
            var calculator = new Calculator(_userManager, _libraryManager, _userDataManager);

            // purely for progress reporting
            var percentPerUser = 100 / users.Count + 1;
            var numComplete = 0;

            //General Stats calculations
            PluginConfiguration.GeneralStat.TotalMovies = calculator.CalculateTotalMovies();
            PluginConfiguration.GeneralStat.TotalShows = calculator.CalculateTotalShows();
            PluginConfiguration.GeneralStat.TotalEpisodes = calculator.CalculateTotalEpisodes();

            PluginConfiguration.GeneralStat.MovieQualities = calculator.CalculateMovieQualities();
            PluginConfiguration.GeneralStat.EpisodeQualities = calculator.CalculateEpisodeQualities();

            numComplete++;
            double currentProgress = percentPerUser * numComplete;
            progress.Report(currentProgress);

            foreach (var user in users)
            {
                calculator.SetUser(user);

                await Task.Run(() =>
                {
                    var stat = new UserStat
                    {
                        UserName = user.Name,
                        TopYears = calculator.CalculateTopYears(),
                        LastShowsSeen = calculator.CalculateLastSeenShows(),
                        LastMoviesSeen = calculator.CalculateLastSeenMovies(),
                        TopMovieGenres = calculator.CalculateTopMovieGenres(),
                        TopShowGenres = calculator.CalculateTopShowGenres(),
                        PlayedMovieViewTime = calculator.CalculateMovieTime(),
                        PlayedShowViewTime = calculator.CalculateShowTime(),
                        PlayedOverallViewTime = calculator.CalculateOverallTime(),
                        MovieViewTime = calculator.CalculateMovieTime(false),
                        ShowViewTime = calculator.CalculateShowTime(false),
                        OverallViewTime = calculator.CalculateOverallTime(false)
                    };
                    
                    PluginConfiguration.UserStats.Add(stat);
                }, cancellationToken);

                numComplete++;
                currentProgress = percentPerUser * numComplete;
                progress.Report(currentProgress);
            }

            Plugin.Instance.SaveConfiguration();
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            var trigger = new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerDaily, TimeOfDayTicks = TimeSpan.FromHours(0).Ticks }; //12am

            return new[] { trigger };
        }

        public string Name => "Calculate statistics for all users";
        public string Key => "StatisticsCalculateStatsTask";
        public string Description => "Task that will calculate statistics needed for the statistics plugin for all users.";
        public string Category => "Statistics";
    }
}
