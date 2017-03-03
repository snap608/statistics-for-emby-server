using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
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

            var showList = GetAllSeries().OrderBy(x => x.SortName);
            var showProgress = new List<ShowProgress>();

            foreach (var show in showList)
            {
                var seasons = show.Children.ToList();

                var collectedEpisodes = seasons.OfType<Season>().Where(s => s.Name != "Specials").Sum(s => s.Children.OfType<Episode>().Count(e => e.GetMediaStreams().Any(y => y.Type == MediaStreamType.Video)));
                collectedEpisodes += seasons.OfType<Episode>().Count(e => e.GetMediaStreams().Any(y => y.Type == MediaStreamType.Video)); //Some Episodes are not in Seasons

                var totalEpisodes = seasons.OfType<Season>().Where(s => s.Name != "Specials").Sum(s => s.Children.OfType<Episode>().Count());
                totalEpisodes += seasons.OfType<Episode>().Count();

                var seenEpisodes = seasons.OfType<Season>().Where(s => s.Name != "Specials").Sum(s => s.Children.OfType<Episode>().Count(e => e.IsPlayed(User)));
                seenEpisodes += seasons.OfType<Episode>().Count(e => e.IsPlayed(User));

                var totalSpecials = seasons.OfType<Season>().Where(s => s.Name == "Specials").Sum(s => s.Children.OfType<Episode>().Count());
                var seenSpecials = seasons.OfType<Season>().Where(s => s.Name == "Specials").Sum(s => s.Children.OfType<Episode>().Count(e => e.IsPlayed(User)));

                decimal watched = 0;
                decimal collected = 0;
                if (totalEpisodes > 0)
                {
                    watched = seenEpisodes / (decimal) totalEpisodes * 100;
                    collected = collectedEpisodes / (decimal) totalEpisodes * 100;
                }

                showProgress.Add(new ShowProgress
                {
                    Name = show.Name,
                    SortName = show.SortName,
                    Score = show.CommunityRating,
                    Status = show.Status,
                    StartYear = show.PremiereDate?.ToString("yyyy"),
                    Watched = Math.Round(watched, 1),
                    Episodes = totalEpisodes,
                    SeenEpisodes = seenEpisodes,
                    Specials = totalSpecials,
                    SeenSpecials = seenSpecials,
                    Collected = Math.Round(collected, 1)
                });
            }

            return showProgress;
        }
    }
}
