using Nop.Core.Configuration;

namespace Nop.Core.Domain
{
    /// <summary>
    /// Site information settings
    /// </summary>
    public class SiteInformationSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether "powered by nopCommerce" text should be displayed.
        /// Please find more info at https://www.nopcommerce.com/copyrightremoval.aspx
        /// </summary>
        public bool HidePoweredByNopCommerce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether site is closed
        /// </summary>
        public bool SiteClosed { get; set; }

        /// <summary>
        /// Gets or sets a picture identifier of the logo. If 0, then the default one will be used
        /// </summary>
        public int LogoPictureId { get; set; }

        /// <summary>
        /// Gets or sets a default site theme
        /// </summary>
        public string DefaultSiteTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether users are allowed to select a theme
        /// </summary>
        public bool AllowUserToSelectTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed in public site (used for debugging)
        /// </summary>
        public bool DisplayMiniProfilerInPublicSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed only for users with access to the admin area
        /// </summary>
        public bool DisplayMiniProfilerForAdminOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should display warnings about the new EU cookie law
        /// </summary>
        public bool DisplayEuCookieLawWarning { get; set; }
    }
}
