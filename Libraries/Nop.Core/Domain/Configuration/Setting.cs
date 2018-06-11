
using System;

namespace Nop.Core.Domain.Configuration
{
    /// <summary>
    /// Represents a setting
    /// </summary>
    public partial class Setting : BaseEntity
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public Setting()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="siteId">Site identifier</param>
        public Setting(string name, string value, Guid siteId = default(Guid)) 
        {
            this.Name = name;
            this.Value = value;
            this.SiteId = siteId;
        }
        
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the site for which this setting is valid. 0 is set when the setting is for all sites
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>Result</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
