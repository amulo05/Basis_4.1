using Nop.Core.Domain.Users;
using System;

namespace Nop.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission record-user role mapping class
    /// </summary>
    public partial class PermissionRecordUserRoleMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the permission record identifier
        /// </summary>
        public Guid PermissionRecordId { get; set; }

        /// <summary>
        /// Gets or sets the user role identifier
        /// </summary>
        public Guid UserRoleId { get; set; }

        /// <summary>
        /// Gets or sets the permission record
        /// </summary>
        public virtual PermissionRecord PermissionRecord { get; set; }

        /// <summary>
        /// Gets or sets the user role
        /// </summary>
        public virtual UserRole UserRole { get; set; }
    }
}