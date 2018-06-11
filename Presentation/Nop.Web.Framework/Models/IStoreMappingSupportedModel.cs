using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Web.Framework.Models
{
    /// <summary>
    /// Represents the site mapping supported model
    /// </summary>
    public partial interface ISiteMappingSupportedModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets identifiers of the selected sites
        /// </summary>
        IList<Guid> SelectedSiteIds { get; set; }

        /// <summary>
        /// Gets or sets items for the all available sites
        /// </summary>
        IList<SelectListItem> AvailableSites { get; set; }

        #endregion
    }
}