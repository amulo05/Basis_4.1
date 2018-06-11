using Nop.Core;
using Nop.Core.Domain.Sites;
using Nop.Web.Framework.Models;

namespace Nop.Web.Framework.Factories
{
    /// <summary>
    /// Represents the site mapping supported model factory
    /// </summary>
    public partial interface ISiteMappingSupportedModelFactory
    {
        /// <summary>
        /// Prepare selected and all available sites for the passed model
        /// </summary>
        /// <typeparam name="TModel">Site mapping supported model type</typeparam>
        /// <param name="model">Model</param>
        void PrepareModelSites<TModel>(TModel model) where TModel : ISiteMappingSupportedModel;

        /// <summary>
        /// Prepare selected and all available sites for the passed model by site mappings
        /// </summary>
        /// <typeparam name="TModel">Site mapping supported model type</typeparam>
        /// <typeparam name="TEntity">Site mapping supported entity type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="entity">Entity</param>
        /// <param name="ignoreSiteMappings">Whether to ignore existing site mappings</param>
        void PrepareModelSites<TModel, TEntity>(TModel model, TEntity entity, bool ignoreSiteMappings)
            where TModel : ISiteMappingSupportedModel where TEntity : BaseEntity, ISiteMappingSupported;
    }
}