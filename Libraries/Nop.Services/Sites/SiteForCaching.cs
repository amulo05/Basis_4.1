using System;
using System.ComponentModel.DataAnnotations.Schema;
using Nop.Core.Caching;
using Nop.Core.Domain.Sites;

namespace Nop.Services.Sites
{
    /// <summary>
    /// Site (for caching)
    /// </summary>
    [Serializable]
    //Entity Framework will assume that any class that inherits from a POCO class that is mapped to a table on the database requires a Discriminator column
    //That's why we have to add [NotMapped] as an attribute of the derived class.
    [NotMapped]
    public class SiteForCaching : Site, IEntityForCaching
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public SiteForCaching()
        {
            
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="s">Site to copy</param>
        public SiteForCaching(Site s)
        {
            Id = s.Id;
            Name = s.Name;
            Url = s.Url;
            SslEnabled = s.SslEnabled;
            Hosts = s.Hosts;
            DisplayOrder = s.DisplayOrder;
        }
    }
}
