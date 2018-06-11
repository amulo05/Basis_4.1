namespace Nop.Core.Domain.Sites
{
    /// <summary>
    /// Represents an entity which supports site mapping
    /// </summary>
    public partial interface ISiteMappingSupported
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain sites
        /// </summary>
        bool LimitedToSites { get; set; }
    }
}
