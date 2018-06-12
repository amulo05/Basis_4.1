
using System;

namespace Nop.Web.Framework.Models
{
    /// <summary>
    /// Represents base nopCommerce entity model
    /// </summary>
    public partial class BaseNopEntityModel : BaseNopModel
    {
        /// <summary>
        /// Gets or sets model identifier
        /// </summary>
        public virtual Guid Id { get; set; }
    }
}