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
using Statistics.ViewModel;

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
            PluginConfiguration.GeneralStat = new List<ValueGroup>();
            Plugin.Instance.SaveConfiguration();
            
            var calculator = new Calculator(_userManager, _libraryManager, _userDataManager);

            // purely for progress reporting
            var percentPerUser = 100 / (users.Count + 1);
            var numComplete = 0;

            PluginConfiguration.LastUpdated = DateTime.Now.ToString("g", Thread.CurrentThread.CurrentCulture);

            //General Stats calculations
            //General Stats calculations"
            PluginConfiguration.GeneralStat.Add(calculator.CalculateMovieQualities());
            PluginConfiguration.GeneralStat.Add(calculator.CalculateTotalMovies());
            PluginConfiguration.GeneralStat.Add(calculator.CalculateTotalShows());

            numComplete++;
            double currentProgress = (percentPerUser * numComplete);
            progress.Report(currentProgress);

            var ActiveUsers = new Dictionary<string, RunTime>();

            foreach (var user in users)
            {
                calculator.SetUser(user);

                await Task.Run(() =>
                {
                    var overallTime = calculator.CalculateOverallTime();
                    ActiveUsers.Add(user.Name, new RunTime(overallTime.Raw));
                    var stat = new UserStat

                    {
                        UserName = user.Name,
                        ValueGroups = new List<ValueGroup>
                        {
                            calculator.CalculateTotalMovies(),
                            calculator.CalculateTotalShows(),
                            calculator.CalculateTotalEpisodes(),
                            calculator.CalculateTopYears(),
                            calculator.CalculateTopMovieGenres(),
                            calculator.CalculateTopShowGenres(),
                            calculator.CalculateMovieTime(),
                            calculator.CalculateShowTime(),
                            overallTime,
                            calculator.CalculateMovieTime(false),
                            calculator.CalculateShowTime(false),
                            calculator.CalculateOverallTime(false),
                            calculator.CalculateLastSeenShows(),
                            calculator.CalculateLastSeenMovies()
                        },
                        ShowProgresses = new ShowProgressCalculator(_userManager, _libraryManager, _userDataManager, user).CalculateShowProgress()
                    };
                    
                    PluginConfiguration.UserStats.Add(stat);
                }, cancellationToken);

                numComplete++;
                currentProgress = percentPerUser * numComplete;
                progress.Report(currentProgress);
            }

            calculator.SetUser(null);

            PluginConfiguration.GeneralStat.Add(calculator.CalculateMostActiveUsers(ActiveUsers));
            PluginConfiguration.GeneralStat.Add(calculator.CalculateTotalEpisodes());

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
