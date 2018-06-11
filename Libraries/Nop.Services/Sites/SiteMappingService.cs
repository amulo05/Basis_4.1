using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Sites;
using Nop.Data.Extensions;
using Nop.Services.Events;

namespace Nop.Services.Sites
{
    /// <summary>
    /// Site mapping service
    /// </summary>
    public partial class SiteMappingService : ISiteMappingService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        private const string STOREMAPPING_BY_ENTITYID_NAME_KEY = "Nop.sitemapping.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string STOREMAPPING_PATTERN_KEY = "Nop.sitemapping.";

        #endregion

        #region Fields

        private readonly IRepository<SiteMapping> _siteMappingRepository;
        private readonly ISiteContext _siteContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Static cache manager</param>
        /// <param name="siteContext">Site context</param>
        /// <param name="siteMappingRepository">Site mapping repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        public SiteMappingService(IStaticCacheManager cacheManager, 
            ISiteContext siteContext,
            IRepository<SiteMapping> siteMappingRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._siteContext = siteContext;
            this._siteMappingRepository = siteMappingRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a site mapping record
        /// </summary>
        /// <param name="siteMapping">Site mapping record</param>
        public virtual void DeleteSiteMapping(SiteMapping siteMapping)
        {
            if (siteMapping == null)
                throw new ArgumentNullException(nameof(siteMapping));

            _siteMappingRepository.Delete(siteMapping);

            //cache
            _cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(siteMapping);
        }

        /// <summary>
        /// Gets a site mapping record
        /// </summary>
        /// <param name="siteMappingId">Site mapping record identifier</param>
        /// <returns>Site mapping record</returns>
        public virtual SiteMapping GetSiteMappingById(Guid siteMappingId)
        {
            if (siteMappingId == default(Guid))
                return null;

            return _siteMappingRepository.GetById(siteMappingId);
        }

        /// <summary>
        /// Gets site mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Site mapping records</returns>
        public virtual IList<SiteMapping> GetSiteMappings<T>(T entity) where T : BaseEntity, ISiteMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityId = entity.Id;
            var entityName = entity.GetType().BaseType.Name;

            var query = from sm in _siteMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var siteMappings = query.ToList();
            return siteMappings;
        }
        
        /// <summary>
        /// Inserts a site mapping record
        /// </summary>
        /// <param name="siteMapping">Site mapping</param>
        protected virtual void InsertSiteMapping(SiteMapping siteMapping)
        {
            if (siteMapping == null)
                throw new ArgumentNullException(nameof(siteMapping));

            _siteMappingRepository.Insert(siteMapping);

            //cache
            _cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(siteMapping);
        }

        /// <summary>
        /// Inserts a site mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="siteId">Site id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertSiteMapping<T>(T entity, Guid siteId) where T : BaseEntity, ISiteMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (siteId == default(Guid))
                throw new ArgumentOutOfRangeException("siteId");

            var entityId = entity.Id;
            var entityName = entity.GetType().BaseType.Name;

            var siteMapping = new SiteMapping
            {
                EntityId = entityId,
                EntityName = entityName,
                SiteId = siteId
            };

            InsertSiteMapping(siteMapping);
        }
        
        /// <summary>
        /// Find site identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Site identifiers</returns>
        public virtual Guid[] GetSitesIdsWithAccess<T>(T entity) where T : BaseEntity, ISiteMappingSupported
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityId = entity.Id;
            var entityName = entity.GetType().BaseType.Name;

            var key = string.Format(STOREMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
            return _cacheManager.Get(key, () =>
            {
                var query = from sm in _siteMappingRepository.Table
                            where sm.EntityId == entityId &&
                            sm.EntityName == entityName
                            select sm.SiteId;
                return query.ToArray();
            });
        }

        /// <summary>
        /// Authorize whether entity could be accessed in the current site (mapped to this site)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity) where T : BaseEntity, ISiteMappingSupported
        {
            return Authorize(entity, _siteContext.CurrentSite.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a site (mapped to this site)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize<T>(T entity, Guid siteId) where T : BaseEntity, ISiteMappingSupported
        {
            if (entity == null)
                return false;

            if (siteId == default(Guid))
                //return true if no site specified/found
                return true;

            if (!entity.LimitedToSites)
                return true;

            foreach (var siteIdWithAccess in GetSitesIdsWithAccess(entity))
                if (siteId == siteIdWithAccess)
                    //yes, we have such permission
                    return true;

            //no permission found
            return false;
        }

        #endregion
    }
}