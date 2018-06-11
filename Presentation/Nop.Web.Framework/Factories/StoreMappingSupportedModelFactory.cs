using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Sites;
using Nop.Services.Sites;
using Nop.Web.Framework.Models;

namespace Nop.Web.Framework.Factories
{
    /// <summary>
    /// Represents the base site mapping supported model factory implementation
    /// </summary>
    public partial class SiteMappingSupportedModelFactory : ISiteMappingSupportedModelFactory
    {
        #region Fields
        
        private readonly ISiteMappingService _siteMappingService;
        private readonly ISiteService _siteService;

        #endregion

        #region Ctor

        public SiteMappingSupportedModelFactory(ISiteMappingService siteMappingService,
            ISiteService siteService)
        {
            this._siteMappingService = siteMappingService;
            this._siteService = siteService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare selected and all available sites for the passed model
        /// </summary>
        /// <typeparam name="TModel">Site mapping supported model type</typeparam>
        /// <param name="model">Model</param>
        public virtual void PrepareModelSites<TModel>(TModel model) where TModel : ISiteMappingSupportedModel
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available sites
            var availableSites = _siteService.GetAllSites();
            model.AvailableSites = availableSites.Select(site => new SelectListItem
            {
                Text = site.Name,
                Value = site.Id.ToString(),
                Selected = model.SelectedSiteIds.Contains(site.Id)
            }).ToList();
        }

        /// <summary>
        /// Prepare selected and all available sites for the passed model by site mappings
        /// </summary>
        /// <typeparam name="TModel">Site mapping supported model type</typeparam>
        /// <typeparam name="TEntity">Site mapping supported entity type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="entity">Entity</param>
        /// <param name="ignoreSiteMappings">Whether to ignore existing site mappings</param>
        public virtual void PrepareModelSites<TModel, TEntity>(TModel model, TEntity entity, bool ignoreSiteMappings)
            where TModel : ISiteMappingSupportedModel where TEntity : BaseEntity, ISiteMappingSupported
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare sites with granted access
            if (!ignoreSiteMappings && entity != null)
                model.SelectedSiteIds = _siteMappingService.GetSitesIdsWithAccess(entity).ToList();

            PrepareModelSites(model);
        }
        
        #endregion
    }
}