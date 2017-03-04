using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Statistics.Configuration;

namespace Statistics.Helpers
{
    public class ShowProgressCalculator : BaseCalculator
    {
        public ShowProgressCalculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager,  User user)
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
                var totalEpisodes = GetTotalEpisodesCount(show);
                var collectedEpisodes = GetOwnedEpisodesCount(show);
                var seenEpisodes = GetPlayedEpisodeCount(show);

                var totalSpecials = GetOwnedSpecials(show);
                var seenSpecials = GetPlayedSpecials(show);

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

        private int GetTotalEpisodesCount(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsSpecialSeason = false,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote, LocationType.Virtual },
                SourceTypes = new[] { SourceType.Library }
            };

            var list = LibraryManager.GetItemList(query).OfType<Season>();
            return list.Sum(x => x.Children.Count(e => e.PremiereDate <= DateTime.Now || e.PremiereDate ==  null))
                + show.Children.OfType<Episode>().Count(e => e.PremiereDate <= DateTime.Now || e.PremiereDate == null);
        }

        private int GetOwnedEpisodesCount(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsSpecialSeason = false,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            var list = LibraryManager.GetItemList(query).OfType<Season>();
            return CalculateResult(show, list);
        }

        private int GetPlayedEpisodeCount(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsPlayed = true,
                IsSpecialSeason = false,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote, LocationType.Virtual },
                SourceTypes = new[] { SourceType.Library }
            };

            var list = LibraryManager.GetItemList(query).OfType<Season>();
            return CalculateResult(show, list);
        }

        private int GetOwnedSpecials(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsSpecialSeason = true,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            var list = LibraryManager.GetItemList(query).OfType<Season>();
            return CalculateResult(show, list);
        }

        private int GetPlayedSpecials(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsPlayed = true,
                IsSpecialSeason = true,
                MaxPremiereDate = DateTime.Now,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            var list = LibraryManager.GetItemList(query).OfType<Season>();
            return CalculateResult(show, list);
        }

        private int CalculateResult(Series show, IEnumerable<Season> seasons)
        {
            return seasons.Sum(x => x.Children.Count(e => (e.PremiereDate <= DateTime.Now || e.PremiereDate == null) && e.LocationType != LocationType.Virtual))
                + show.Children.OfType<Episode>().Count(e => (e.PremiereDate <= DateTime.Now || e.PremiereDate == null) && e.LocationType != LocationType.Virtual);
        }
    }
}
