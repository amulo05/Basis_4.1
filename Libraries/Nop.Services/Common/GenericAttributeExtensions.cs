using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Infrastructure;

namespace Nop.Services.Common
{
    /// <summary>
    /// Generic attribute extensions
    /// </summary>
    public static class GenericAttributeExtensions
    {
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="siteId">Load a value specific for a certain site; pass 0 to load a value shared for all sites</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, Guid siteId = default(Guid))
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            return GetAttribute<TPropType>(entity, key, genericAttributeService, siteId);
        }

        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="genericAttributeService">GenericAttributeService</param>
        /// <param name="siteId">Load a value specific for a certain site; pass 0 to load a value shared for all sites</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity,
            string key, IGenericAttributeService genericAttributeService, Guid siteId = default(Guid))
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var keyGroup = entity.GetType().BaseType.Name;

            var props = genericAttributeService.GetAttributesForEntity(entity.Id, keyGroup);
            //little hack here (only for unit testing). we should write expect-return rules in unit tests for such cases
            if (props == null)
                return default(TPropType);
            props = props.Where(x => x.SiteId == siteId).ToList();
            if (!props.Any())
                return default(TPropType);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return default(TPropType);

            return CommonHelper.To<TPropType>(prop.Value);
        }
    }
}
