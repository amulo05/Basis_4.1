using Nop.Core.Domain.Users;

namespace Nop.Core
{
    /// <summary>
    /// Represents work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current user
        /// </summary>
        User CurrentUser { get; set; }

        /// <summary>
        /// Gets the original user (in case the current one is impersonated)
        /// </summary>
        User OriginalUserIfImpersonated { get; }

        /// <summary>
        /// Gets or sets value indicating whether we're in admin area
        /// </summary>
        bool IsAdmin { get; set; }
    }
}
