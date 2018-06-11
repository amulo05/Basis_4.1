using System.Collections.Generic;
using Nop.Core.Domain.Sites;

namespace Nop.Services.Sites
{
    /// <summary>
    /// Site service interface
    /// </summary>
    public partial interface ISiteService
    {
        /// <summary>
        /// Deletes a site
        /// </summary>
        /// <param name="site">Site</param>
        void DeleteSite(Site site);

        /// <summary>
        /// Gets all sites
        /// </summary>
        /// <param name="loadCacheableCopy">A value indicating whether to load a copy that could be cached (workaround until Entity Framework supports 2-level caching)</param>
        /// <returns>Sites</returns>
        IList<Site> GetAllSites(bool loadCacheableCopy = true);

        /// <summary>
        /// Gets a site 
        /// </summary>
        /// <param name="siteId">Site identifier</param>
        /// <param name="loadCacheableCopy">A value indicating whether to load a copy that could be cached (workaround until Entity Framework supports 2-level caching)</param>
        /// <returns>Site</returns>
        Site GetSiteById(int siteId, bool loadCacheableCopy = true);

        /// <summary>
        /// Inserts a site
        /// </summary>
        /// <param name="site">Site</param>
        void InsertSite(Site site);

        /// <summary>
        /// Updates the site
        /// </summary>
        /// <param name="site">Site</param>
        void UpdateSite(Site site);
    }
}