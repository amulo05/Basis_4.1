using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Sites;

namespace Nop.Data.Mapping.Sites
{
    /// <summary>
    /// Represents a site mapping configuration
    /// </summary>
    public partial class SiteMap : NopEntityTypeConfiguration<Site>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Site> builder)
        {
            builder.ToTable(nameof(Site));
            builder.HasKey(site => site.Id);

            builder.Property(site => site.Name).HasMaxLength(400).IsRequired();
            builder.Property(site => site.Url).HasMaxLength(400).IsRequired();
            builder.Property(site => site.Hosts).HasMaxLength(1000);

            base.Configure(builder);
        }

        #endregion
    }
}