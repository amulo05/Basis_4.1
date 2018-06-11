using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Core.Domain.Sites
{
    /// <summary>
    /// Site extensions
    /// </summary>
    public static class SiteExtensions
    {
        /// <summary>
        /// Parse comma-separated Hosts
        /// </summary>
        /// <param name="site">Site</param>
        /// <returns>Comma-separated hosts</returns>
        public static string[] ParseHostValues(this Site site)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            var parsedValues = new List<string>();
            if (string.IsNullOrEmpty(site.Hosts)) 
                return parsedValues.ToArray();

            var hosts = site.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var host in hosts)
            {
                var tmp = host.Trim();
                if (!string.IsNullOrEmpty(tmp))
                    parsedValues.Add(tmp);
            }

            return parsedValues.ToArray();
        }

        /// <summary>
        /// Indicates whether a site contains a specified host
        /// </summary>
        /// <param name="site">Site</param>
        /// <param name="host">Host</param>
        /// <returns>true - contains, false - no</returns>
        public static bool ContainsHostValue(this Site site, string host)
        {
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            if (string.IsNullOrEmpty(host))
                return false;

            var contains = site.ParseHostValues()
                .FirstOrDefault(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase)) != null;

            return contains;
        }
    }
}
