using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Sites;

namespace Nop.Data.Mapping.Sites
{
    /// <summary>
    /// Represents a site mapping mapping configuration
    /// </summary>
    public partial class SiteMappingMap : NopEntityTypeConfiguration<SiteMapping>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SiteMapping> builder)
        {
            builder.ToTable(nameof(SiteMapping));
            builder.HasKey(siteMapping => siteMapping.Id);

            builder.Property(siteMapping => siteMapping.EntityName).HasMaxLength(400).IsRequired();

            builder.HasOne(siteMapping => siteMapping.Site)
                .WithMany()
                .HasForeignKey(siteMapping => siteMapping.SiteId)
                .IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}