namespace Nop.Core.Domain.Sites
{
    /// <summary>
    /// Represents a site
    /// </summary>
    public partial class Site : BaseEntity
    {
        /// <summary>
        /// Gets or sets the site name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the site URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled
        /// </summary>
        public bool SslEnabled { get; set; }

        /// <summary>
        /// Gets or sets the comma separated list of possible HTTP_HOST values
        /// </summary>
        public string Hosts { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
