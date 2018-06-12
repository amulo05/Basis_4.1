using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Common;

namespace Nop.Core.Domain.Users
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public partial class User : BaseEntity
    {
        private ICollection<ExternalAuthenticationRecord> _externalAuthenticationRecords;
        private ICollection<UserUserRoleMapping> _userUserRoleMappings;

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the email that should be re-validated. Used in scenarios when a user is already registered and wants to change an email address.
        /// </summary>
        public string EmailToRevalidate { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is required to re-login
        /// </summary>
        public bool RequireReLogin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating number of failed login attempts (wrong password)
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the date and time until which a user cannot login (locked out)
        /// </summary>
        public DateTime? CannotLoginUntilDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is system
        /// </summary>
        public bool IsSystemAccount { get; set; }

        /// <summary>
        /// Gets or sets the user system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the last IP address
        /// </summary>
        public string LastIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of last activity
        /// </summary>
        public DateTime LastActivityDate { get; set; }

        #region Navigation properties

        /// <summary>
        /// Gets or sets user generated content
        /// </summary>
        public virtual ICollection<ExternalAuthenticationRecord> ExternalAuthenticationRecords
        {
            get => _externalAuthenticationRecords ?? (_externalAuthenticationRecords = new List<ExternalAuthenticationRecord>());
            protected set => _externalAuthenticationRecords = value;
        }

        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public IList<UserRole> UserRoles => UserUserRoleMappings.Select(mapping => mapping.UserRole).ToList();

        /// <summary>
        /// Gets or sets user-user role mappings
        /// </summary>
        public virtual ICollection<UserUserRoleMapping> UserUserRoleMappings
        {
            get => _userUserRoleMappings ?? (_userUserRoleMappings = new List<UserUserRoleMapping>());
            protected set => _userUserRoleMappings = value;
        }

        #endregion
    }
}