using System;

namespace Nop.Core.Domain.Sites
{
    /// <summary>
    /// Represents a site mapping record
    /// </summary>
    public partial class SiteMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the site identifier
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the site
        /// </summary>
        public virtual Site Site { get; set; }
    }
}
