using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Sites;

namespace Nop.Services.Sites
{
    /// <summary>
    /// Site mapping service interface
    /// </summary>
    public partial interface ISiteMappingService
    {
        /// <summary>
        /// Deletes a site mapping record
        /// </summary>
        /// <param name="siteMapping">Site mapping record</param>
        void DeleteSiteMapping(SiteMapping siteMapping);

        /// <summary>
        /// Gets a site mapping record
        /// </summary>
        /// <param name="siteMappingId">Site mapping record identifier</param>
        /// <returns>Site mapping record</returns>
        SiteMapping GetSiteMappingById(Guid siteMappingId);

        /// <summary>
        /// Gets site mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Site mapping records</returns>
        IList<SiteMapping> GetSiteMappings<T>(T entity) where T : BaseEntity, ISiteMappingSupported;

        /// <summary>
        /// Inserts a site mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="siteId">Site id</param>
        /// <param name="entity">Entity</param>
        void InsertSiteMapping<T>(T entity, Guid siteId) where T : BaseEntity, ISiteMappingSupported;

        /// <summary>
        /// Find site identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Site identifiers</returns>
        Guid[] GetSitesIdsWithAccess<T>(T entity) where T : BaseEntity, ISiteMappingSupported;

        /// <summary>
        /// Authorize whether entity could be accessed in the current site (mapped to this site)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity) where T : BaseEntity, ISiteMappingSupported;

        /// <summary>
        /// Authorize whether entity could be accessed in a site (mapped to this site)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize<T>(T entity, Guid siteId) where T : BaseEntity, ISiteMappingSupported;
    }
}