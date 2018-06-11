using Nop.Core.Domain.Sites;
using System;

namespace Nop.Core
{
    /// <summary>
    /// Site context
    /// </summary>
    public interface ISiteContext
    {
        /// <summary>
        /// Gets the current site
        /// </summary>
        Site CurrentSite { get; }

        /// <summary>
        /// Gets active site scope configuration
        /// </summary>
        Guid ActiveSiteScopeConfiguration { get; }
    }
}
