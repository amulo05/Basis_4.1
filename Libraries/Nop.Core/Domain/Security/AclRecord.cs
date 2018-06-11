using Nop.Core.Domain.Users;
using System;

namespace Nop.Core.Domain.Security
{
    /// <summary>
    /// Represents an ACL record
    /// </summary>
    public partial class AclRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the user role identifier
        /// </summary>
        public Guid UserRoleId { get; set; }

        /// <summary>
        /// Gets or sets the user role
        /// </summary>
        public virtual UserRole UserRole { get; set; }
    }
}
