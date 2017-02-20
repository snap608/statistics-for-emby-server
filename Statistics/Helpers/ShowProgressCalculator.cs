using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Statistics.Configuration;

namespace Statistics.Helpers
{
    public class ShowProgressCalculator : BaseCalculator
    {
        public ShowProgressCalculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager, User user)
            : base(userManager, libraryManager, userDataManager)
        {
            User = user;
        }

        public List<ShowProgress> CalculateShowProgress()
        {
            if (User == null)
                return null;

            var showList = GetAllSeries(User).OrderBy(x => x.SortName);
            var showProgress = new List<ShowProgress>();

            foreach (var show in showList)
            {
                var TotalEpisodes = show.GetEpisodes(User).Count();
                var seenEpisodes = show.GetEpisodes(User).Count(e => e.IsPlayed(User));
                float progress = 0;
                if (TotalEpisodes > 0)
                    progress = seenEpisodes / (float)TotalEpisodes * 100;

                showProgress.Add(new ShowProgress
                {
                    Name = show.Name,
                    Score = show.CommunityRating,
                    Status = show.Status,
                    StartYear = show.PremiereDate?.ToString("MM/yyyy"),
                    Progress = progress.ToString("F1") + "%"
                });
            }

            return showProgress;
        }
    }
}
