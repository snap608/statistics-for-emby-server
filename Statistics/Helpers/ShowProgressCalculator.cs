using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.ApiClient;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using Statistics.Api;
using Statistics.Configuration;

namespace Statistics.Helpers
{
    public class ShowProgressCalculator : BaseCalculator
    {
        private readonly IZipClient _zipClient;
        private readonly IHttpClient _httpClient;
        private readonly IFileSystem _fileSystem;
        private readonly IMemoryStreamFactory _memoryStreamProvider;
        private readonly IServerApplicationPaths _serverApplicationPaths;
        public ShowProgressCalculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager, IZipClient zipClient, IHttpClient httpClient, IFileSystem fileSystem, IMemoryStreamFactory memoryStreamProvider, IServerApplicationPaths serverApplicationPaths,  User user = null)
            : base(userManager, libraryManager, userDataManager)
        {
            _zipClient = zipClient;
            _httpClient = httpClient;
            _fileSystem = fileSystem;
            _memoryStreamProvider = memoryStreamProvider;
            _serverApplicationPaths = serverApplicationPaths;
            User = user;
        }

        public string GetServerTime(CancellationToken cancellationToken)
        {
            var provider = new TheTvDbProvider(_zipClient, _httpClient, _fileSystem, _memoryStreamProvider, _serverApplicationPaths);
            return provider.GetServerTime(cancellationToken).Result;
        }

        public List<UpdateShowModel> CalculateTotalEpisodes(IEnumerable<string> showIds, CancellationToken cancellationToken)
        {
            var result = new List<UpdateShowModel>();
            var provider = new TheTvDbProvider(_zipClient, _httpClient, _fileSystem, _memoryStreamProvider, _serverApplicationPaths);

            foreach (var showId in showIds)
            {
                var total = provider.CalculateEpisodeCount(showId, "en", cancellationToken).Result;
                result.Add(new UpdateShowModel(showId, total));
            }

            return result;
        }

        public IEnumerable<string> GetShowsToUpdate(IEnumerable<string> showIds, string time, CancellationToken cancellationToken)
        {
            var provider = new TheTvDbProvider(_zipClient, _httpClient, _fileSystem, _memoryStreamProvider, _serverApplicationPaths);
            return provider.GetSeriesIdsToUpdate(showIds, time, cancellationToken).Result;
        }

        public List<ShowProgress> CalculateShowProgress(UpdateModel tvdbData)
        {
            if (User == null)
                return null;

            var showList = GetAllSeries().OrderBy(x => x.SortName);
            var showProgress = new List<ShowProgress>();
            
            foreach (var show in showList)
            {
                var totalEpisodes = tvdbData.IdList.SingleOrDefault(x => x.ShowId == show.GetProviderId(MetadataProviders.Tvdb))?.Count ?? 0;

                var collectedEpisodes = GetOwnedEpisodesCount(show);
                var seenEpisodes = GetPlayedEpisodeCount(show);

                var totalSpecials = GetOwnedSpecials(show);
                var seenSpecials = GetPlayedSpecials(show);

                decimal watched = 0;
                decimal collected = 0;
                if (totalEpisodes > 0)
                {
                    collected = collectedEpisodes / (decimal)totalEpisodes * 100;
                }

                if (collectedEpisodes > 0)
                {
                    watched = seenEpisodes / (decimal)collectedEpisodes * 100;
                }

                showProgress.Add(new ShowProgress
                {
                    Name = show.Name,
                    SortName = show.SortName,
                    Score = show.CommunityRating,
                    Status = show.Status,
                    StartYear = show.PremiereDate?.ToString("yyyy"),
                    Watched = Math.Round(watched, 1),
                    Episodes = collectedEpisodes,
                    SeenEpisodes = seenEpisodes,
                    Specials = totalSpecials,
                    SeenSpecials = seenSpecials,
                    Collected = Math.Round(collected, 1)
                });
            }

            return showProgress;
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

            var seasons = LibraryManager.GetItemList(query).OfType<Season>();
            return seasons.Sum(x => x.Children.Count(e => e.PremiereDate <= DateTime.Now || e.PremiereDate == null))
                + show.Children.OfType<Episode>().Count(e => e.PremiereDate <= DateTime.Now || e.PremiereDate == null);
        }

        private int GetPlayedEpisodeCount(Series show)
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

            var seasons = LibraryManager.GetItemList(query).OfType<Season>();
            return seasons.Sum(x => x.Children.Count(e => (e.PremiereDate <= DateTime.Now || e.PremiereDate == null) && e.IsPlayed(User)))
                + show.Children.OfType<Episode>().Count(e => (e.PremiereDate <= DateTime.Now || e.PremiereDate == null) && e.IsPlayed(User));
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

            var seasons = LibraryManager.GetItemList(query).OfType<Season>();
            return seasons.Sum(x => x.Children.Count(e => e.PremiereDate <= DateTime.Now || e.PremiereDate == null));
        }

        private int GetPlayedSpecials(Series show)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(Season).Name },
                Recursive = true,
                ParentId = show.Id,
                IsSpecialSeason = true,
                MaxPremiereDate = DateTime.Now,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            var seasons = LibraryManager.GetItemList(query).OfType<Season>();
            return seasons.Sum(x => x.Children.Count(e => (e.PremiereDate <= DateTime.Now || e.PremiereDate == null) && e.IsPlayed(User)));
        }
    }
}
