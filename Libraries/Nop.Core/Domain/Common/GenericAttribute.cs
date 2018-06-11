using System;

namespace Nop.Core.Domain.Common
{
    /// <summary>
    /// Represents a generic attribute
    /// </summary>
    public partial class GenericAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the key group
        /// </summary>
        public string KeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the site identifier
        /// </summary>
        public Guid SiteId { get; set; }
    }
}
