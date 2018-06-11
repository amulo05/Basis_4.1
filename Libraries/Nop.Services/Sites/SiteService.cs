using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Sites;
using Nop.Services.Events;

namespace Nop.Services.Sites
{
    /// <summary>
    /// Site service
    /// </summary>
    public partial class SiteService : ISiteService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string STORES_ALL_KEY = "Nop.sites.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : site ID
        /// </remarks>
        private const string STORES_BY_ID_KEY = "Nop.sites.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STORES_PATTERN_KEY = "Nop.sites.";

        #endregion

        #region Fields

        private readonly IRepository<Site> _siteRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="siteRepository">Site repository</param>
        /// <param name="eventPublisher">Event publisher</param>
        public SiteService(IStaticCacheManager cacheManager,
            IRepository<Site> siteRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._siteRepository = siteRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a site
        /// </summary>
        /// <param name="site">Site</param>
        public virtual void DeleteSite(Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            if (site is IEntityForCaching)
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");

            var allSites = GetAllSites();
            if (allSites.Count == 1)
                throw new Exception("You cannot delete the only configured site");

            _siteRepository.Delete(site);

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(site);
        }

        /// <summary>
        /// Gets all sites
        /// </summary>
        /// <param name="loadCacheableCopy">A value indicating whether to load a copy that could be cached (workaround until Entity Framework supports 2-level caching)</param>
        /// <returns>Sites</returns>
        public virtual IList<Site> GetAllSites(bool loadCacheableCopy = true)
        {
            Func<IList<Site>> loadSitesFunc = () =>
            {
                var query = from s in _siteRepository.Table
                    orderby s.DisplayOrder, s.Id
                    select s;
                return query.ToList();
            };

            if (loadCacheableCopy)
            {
                //cacheable copy
                return _cacheManager.Get(STORES_ALL_KEY, () =>
                {
                    var result = new List<Site>();
                    foreach (var site in loadSitesFunc())
                        result.Add(new SiteForCaching(site));
                    return result;
                });
            }

            return loadSitesFunc();
        }

        /// <summary>
        /// Gets a site 
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="loadCacheableCopy">A value indicating whether to load a copy that could be cached (workaround until Entity Framework supports 2-level caching)</param>
        /// <returns>Site</returns>
        public virtual Site GetSiteById(int siteId, bool loadCacheableCopy = true)
        {
            if (siteId == 0)
                return null;

            Func<Site> loadSiteFunc = () =>
            {
                return _siteRepository.GetById(siteId);
            };

            if (loadCacheableCopy)
            {
                //cacheable copy
                var key = string.Format(STORES_BY_ID_KEY, siteId);
                return _cacheManager.Get(key, () =>
                {
                    var site = loadSiteFunc();
                    if (site == null)
                        return null;
                    return new SiteForCaching(site);
                });
            }

            return loadSiteFunc();
        } 

        /// <summary>
        /// Inserts a site
        /// </summary>
        /// <param name="site">Site</param>
        public virtual void InsertSite(Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));
            
            if (site is IEntityForCaching)
                throw  new ArgumentException("Cacheable entities are not supported by Entity Framework");

            _siteRepository.Insert(site);

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(site);
        }

        /// <summary>
        /// Updates the site
        /// </summary>
        /// <param name="site">Site</param>
        public virtual void UpdateSite(Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            if (site is IEntityForCaching)
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");

            _siteRepository.Update(site);

            _cacheManager.RemoveByPattern(STORES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(site);
        }

        #endregion
    }
}