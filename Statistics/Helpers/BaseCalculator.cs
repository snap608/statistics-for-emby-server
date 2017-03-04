using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Statistics.Helpers
{
    public abstract class BaseCalculator : IDisposable
    {
        private IEnumerable<Movie> _movieCache;
        private IEnumerable<Series> _seriesCache;
        private IEnumerable<Episode> _episodeCache;

        private IEnumerable<Movie> _ownedMovieCache;
        private IEnumerable<Episode> _ownedEpisodeCache;



        public readonly IUserManager UserManager;
        public readonly ILibraryManager LibraryManager;
        public readonly IUserDataManager UserDataManager;
        public User User;


        protected BaseCalculator(IUserManager userManager, ILibraryManager libraryManager,
            IUserDataManager userDataManager)
        {
            UserManager = userManager;
            LibraryManager = libraryManager;
            UserDataManager = userDataManager;
        }

        #region Helpers

        protected IEnumerable<Movie> GetAllMovies()
        {
            return _movieCache ?? (_movieCache = GetItems<Movie>());
        }

        protected IEnumerable<Series> GetAllSeries()
        {
            return _seriesCache ?? (_seriesCache = GetItems<Series>());
        }

        protected IEnumerable<Episode> GetAllEpisodes()
        {
            return _episodeCache ?? (_episodeCache = GetItems<Episode>());
        }

        protected IEnumerable<Episode> GetAllOwnedEpisodes()
        {
            return _ownedEpisodeCache ?? (_ownedEpisodeCache = GetOwnedItems<Episode>());
        }

        protected IEnumerable<Episode> GetAllViewedEpisodesByUser()
        {
            return _ownedEpisodeCache ?? (_ownedEpisodeCache = GetOwnedItems<Episode>(true));
        }

        protected IEnumerable<Movie> GetAllViewedMoviesByUser()
        {
            return _ownedMovieCache ?? (_ownedMovieCache = GetOwnedItems<Movie>(true));
        }

        protected List<BaseItem> GetAllBaseItems()
        {
            return GetAllMovies().Union(GetAllEpisodes().Cast<BaseItem>()).ToList();
        }

        protected int GetOwnedCount(Type type)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { type.Name },
                Limit = 0,
                Recursive = true,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            return LibraryManager.GetCount(query);
        }

        private IEnumerable<T> GetItems<T>()
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(T).Name },
                Recursive = true,
                SourceTypes = new[] { SourceType.Library }
            };
            
            return LibraryManager.GetItemList(query).OfType<T>();
        }

        private IEnumerable<T> GetOwnedItems<T>(bool? isPLayed = null)
        {
            var query = new InternalItemsQuery(User)
            {
                IncludeItemTypes = new[] { typeof(T).Name },
                IsPlayed = isPLayed,
                Recursive = true,
                LocationTypes = new[] { LocationType.FileSystem, LocationType.Offline, LocationType.Remote },
                SourceTypes = new[] { SourceType.Library }
            };

            return LibraryManager.GetItemsResult(query).Items.OfType<T>().ToList();
        }

        #endregion

        public void Dispose()
        {
            User = null;
        }

        public void SetUser(User user)
        {
            User = user;
        }
    }
}