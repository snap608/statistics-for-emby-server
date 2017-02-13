using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Statistics.Helpers
{
    public abstract class BaseCalculator
    {
        public IEnumerable<Movie> CachedMovieList;
        public IEnumerable<Episode> CachedEpisodeList;
        public IEnumerable<Series> CachedSerieList;

        public readonly IUserManager UserManager;
        public readonly ILibraryManager LibraryManager;
        public readonly IUserDataManager UserDataManager;
        public User User;


        protected BaseCalculator(IUserManager userManager, ILibraryManager libraryManager, IUserDataManager userDataManager)
        {
            UserManager = userManager;
            LibraryManager = libraryManager;
            UserDataManager = userDataManager;
        }

        public void SetUser(User user)
        {
            User = user;
            CachedEpisodeList = null;
            CachedMovieList = null;
            CachedSerieList = null;
        }

        #region Helpers

        public IEnumerable<Movie> GetAllMovies(User user = null)
        {
            return CachedMovieList ??
                   (CachedMovieList = LibraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Movie>());
        }

        public IEnumerable<Series> GetAllSeries(User user = null)
        {
            return CachedSerieList ??
                   (CachedSerieList = LibraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Series>());
        }

        public IEnumerable<Episode> GetAllEpisodes(User user = null)
        {
            return CachedEpisodeList ??
                   (CachedEpisodeList = LibraryManager.GetItemList(new InternalItemsQuery(user)).OfType<Episode>());
        }

        public IEnumerable<Episode> GetAllOwnedEpisodes(User user = null)
        {
            return GetAllEpisodes(user).Where(e => e.GetMediaStreams().Any());
        }

        public IEnumerable<Episode> GetAllViewedEpisodesByUser(User user)
        {
            return
                GetAllEpisodes(user)
                    .Where(m => m.IsPlayed(user) && UserDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        public IEnumerable<Movie> GetAllViewedMoviesByUser(User user)
        {
            return
                GetAllMovies(user)
                    .Where(m => m.IsPlayed(user) && UserDataManager.GetUserData(user, m).LastPlayedDate.HasValue);
        }

        public IEnumerable<BaseItem> GetAllBaseItems()
        {
            return GetAllMovies().Union(GetAllEpisodes().Cast<BaseItem>());
        }

        #endregion
    }
}
