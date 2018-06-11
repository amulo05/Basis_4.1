using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Sites;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Sites;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Site context for web application
    /// </summary>
    public partial class WebSiteContext : ISiteContext
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        private Site _cachedSite;
        private Guid? _cachedActiveSiteScopeConfiguration;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="siteService">Site service</param>
        public WebSiteContext(IHttpContextAccessor httpContextAccessor,
            ISiteService siteService)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._siteService = siteService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current site
        /// </summary>
        public virtual Site CurrentSite
        {
            get
            {
                if (_cachedSite != null)
                    return _cachedSite;

                //try to determine the current site by HOST header
                string host = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Host];

                var allSites = _siteService.GetAllSites();
                var site = allSites.FirstOrDefault(s => s.ContainsHostValue(host));

                if (site == null)
                {
                    //load the first found site
                    site = allSites.FirstOrDefault();
                }

                _cachedSite = site ?? throw new Exception("No site could be loaded");

                return _cachedSite;
            }
        }

        /// <summary>
        /// Gets active site scope configuration
        /// </summary>
        public virtual Guid ActiveSiteScopeConfiguration
        {
            get
            {
                if (_cachedActiveSiteScopeConfiguration.HasValue)
                    return _cachedActiveSiteScopeConfiguration.Value;

                //ensure that we have 2 (or more) sites
                if (_siteService.GetAllSites().Count > 1)
                {
                    //do not inject IWorkContext via constructor because it'll cause circular references
                    var currentUser = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

                    //try to get site identifier from attributes
                    var siteId = currentUser.GetAttribute<int>(SystemUserAttributeNames.AdminAreaSiteScopeConfiguration);

                    _cachedActiveSiteScopeConfiguration = _siteService.GetSiteById(siteId)?.Id ?? default(Guid);
                }
                else
                    _cachedActiveSiteScopeConfiguration = default(Guid);

                return _cachedActiveSiteScopeConfiguration ?? default(Guid);
            }
        }

        #endregion
    }
}