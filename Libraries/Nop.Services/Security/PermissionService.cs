using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Security;
using Nop.Services.Users;

namespace Nop.Services.Security
{
    /// <summary>
    /// Permission service
    /// </summary>
    public partial class PermissionService : IPermissionService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : user role ID
        /// {1} : permission system name
        /// </remarks>
        private const string PERMISSIONS_ALLOWED_KEY = "Nop.permission.allowed-{0}-{1}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : user role ID
        /// </remarks>
        private const string PERMISSIONS_ALLBYCUSTOMERTROLEID_KEY = "Nop.permission.allbyuserroleid-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PERMISSIONS_PATTERN_KEY = "Nop.permission.";

        #endregion

        #region Fields

        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly IRepository<PermissionRecordUserRoleMapping> _permissionRecordUserRoleMappingRepository;
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="permissionRecordRepository">Permission repository</param>
        /// <param name="permissionRecordUserRoleMappingRepository">Permission -user role mapping repository</param>
        /// <param name="userService">User service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="staticCacheManager">Static cache manager</param>
        public PermissionService(IRepository<PermissionRecord> permissionRecordRepository,
            IRepository<PermissionRecordUserRoleMapping> permissionRecordUserRoleMappingRepository,
            IUserService userService,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IStaticCacheManager staticCacheManager)
        {
            this._permissionRecordRepository = permissionRecordRepository;
            this._permissionRecordUserRoleMappingRepository = permissionRecordUserRoleMappingRepository;
            this._userService = userService;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get permission records by user role identifier
        /// </summary>
        /// <param name="userRoleId">User role identifier</param>
        /// <returns>Permissions</returns>
        protected virtual IList<PermissionRecord> GetPermissionRecordsByUserRoleId(Guid userRoleId)
        {
            var key = string.Format(PERMISSIONS_ALLBYCUSTOMERTROLEID_KEY, userRoleId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pr in _permissionRecordRepository.Table
                    join prcrm in _permissionRecordUserRoleMappingRepository.Table on pr.Id equals prcrm.PermissionRecordId
                    where prcrm.UserRoleId == userRoleId
                    orderby pr.Id
                    select pr;

                return query.ToList();
            });
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="userRole">User role</param>
        /// <returns>true - authorized; otherwise, false</returns>
        protected virtual bool Authorize(string permissionRecordSystemName, UserRole userRole)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;
            
            var key = string.Format(PERMISSIONS_ALLOWED_KEY, userRole.Id, permissionRecordSystemName);
            return _staticCacheManager.Get(key, () =>
            {
                var permissions = GetPermissionRecordsByUserRoleId(userRole.Id);
                foreach (var permission1 in permissions)
                    if (permission1.SystemName.Equals(permissionRecordSystemName, StringComparison.InvariantCultureIgnoreCase))
                        return true;

                return false;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual void DeletePermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            _permissionRecordRepository.Delete(permission);

            _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
            _staticCacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }

        /// <summary>
        /// Gets a permission
        /// </summary>
        /// <param name="permissionId">Permission identifier</param>
        /// <returns>Permission</returns>
        public virtual PermissionRecord GetPermissionRecordById(int permissionId)
        {
            if (permissionId == 0)
                return null;

            return _permissionRecordRepository.GetById(permissionId);
        }

        /// <summary>
        /// Gets a permission
        /// </summary>
        /// <param name="systemName">Permission system name</param>
        /// <returns>Permission</returns>
        public virtual PermissionRecord GetPermissionRecordBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from pr in _permissionRecordRepository.Table
                        where  pr.SystemName == systemName
                        orderby pr.Id
                        select pr;

            var permissionRecord = query.FirstOrDefault();
            return permissionRecord;
        }

        /// <summary>
        /// Gets all permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IList<PermissionRecord> GetAllPermissionRecords()
        {
            var query = from pr in _permissionRecordRepository.Table
                        orderby pr.Name
                        select pr;
            var permissions = query.ToList();
            return permissions;
        }

        /// <summary>
        /// Inserts a permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual void InsertPermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            _permissionRecordRepository.Insert(permission);

            _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
            _staticCacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the permission
        /// </summary>
        /// <param name="permission">Permission</param>
        public virtual void UpdatePermissionRecord(PermissionRecord permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            _permissionRecordRepository.Update(permission);

            _cacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
            _staticCacheManager.RemoveByPattern(PERMISSIONS_PATTERN_KEY);
        }
        
        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(PermissionRecord permission)
        {
            return Authorize(permission, _workContext.CurrentUser);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <param name="user">User</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(PermissionRecord permission, User user)
        {
            if (permission == null)
                return false;

            if (user == null)
                return false;

            //old implementation of Authorize method
            //var userRoles = user.UserRoles.Where(cr => cr.Active);
            //foreach (var role in userRoles)
            //    foreach (var permission1 in role.PermissionRecords)
            //        if (permission1.SystemName.Equals(permission.SystemName, StringComparison.InvariantCultureIgnoreCase))
            //            return true;

            //return false;

            return Authorize(permission.SystemName, user);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionRecordSystemName)
        {
            return Authorize(permissionRecordSystemName, _workContext.CurrentUser);
        }

        /// <summary>
        /// Authorize permission
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="user">User</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionRecordSystemName, User user)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
                return false;

            var userRoles = user.UserRoles.Where(cr => cr.Active);
            foreach (var role in userRoles)
                if (Authorize(permissionRecordSystemName, role))
                    //yes, we have such permission
                    return true;
            
            //no permission found
            return false;
        }

        #endregion
    }
}