using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Users;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Events;

namespace Nop.Services.Users
{
    /// <summary>
    /// User service
    /// </summary>
    public partial class UserService : IUserService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        private const string CUSTOMERROLES_ALL_KEY = "Nop.userrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        private const string CUSTOMERROLES_BY_SYSTEMNAME_KEY = "Nop.userrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string CUSTOMERROLES_PATTERN_KEY = "Nop.userrole.";

        #endregion

        #region Fields

        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserUserRoleMapping> _userUserRoleMappingRepository;
        private readonly IRepository<UserPassword> _userPasswordRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly UserSettings _userSettings;
        private readonly CommonSettings _commonSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="userRepository">User repository</param>
        /// <param name="userUserRoleMappingRepository">User role mapping repository</param>
        /// <param name="userPasswordRepository">User password repository</param>
        /// <param name="userRoleRepository">User role repository</param>
        /// <param name="gaRepository">Generic attribute repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="forumPostRepository">Forum post repository</param>
        /// <param name="forumTopicRepository">Forum topic repository</param>
        /// <param name="blogCommentRepository">Blog comment repository</param>
        /// <param name="newsCommentRepository">News comment repository</param>
        /// <param name="pollVotingRecordRepository">Poll voting record repository</param>
        /// <param name="productReviewRepository">Product review repository</param>
        /// <param name="productReviewHelpfulnessRepository">Product review helpfulness repository</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="userSettings">User settings</param>
        /// <param name="commonSettings">Common settings</param>
        public UserService(ICacheManager cacheManager,
            IRepository<User> userRepository,
            IRepository<UserUserRoleMapping> userUserRoleMappingRepository,
            IRepository<UserPassword> userPasswordRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<GenericAttribute> gaRepository,
            IGenericAttributeService genericAttributeService,
            IDataProvider dataProvider,
            IDbContext dbContext,
            IEventPublisher eventPublisher, 
            UserSettings userSettings,
            CommonSettings commonSettings)
        {
            this._cacheManager = cacheManager;
            this._userRepository = userRepository;
            this._userUserRoleMappingRepository = userUserRoleMappingRepository;
            this._userPasswordRepository = userPasswordRepository;
            this._userRoleRepository = userRoleRepository;
            this._gaRepository = gaRepository;
            this._genericAttributeService = genericAttributeService;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._eventPublisher = eventPublisher;
            this._userSettings = userSettings;
            this._commonSettings = commonSettings;
        }

        #endregion
        
        #region Methods

        #region Users

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="userRoleIds">A list of user role identifiers to filter by (at least one match); pass null or empty list in order to load all users; </param>
        /// <param name="email">Email; null to load all users</param>
        /// <param name="username">Username; null to load all users</param>
        /// <param name="firstName">First name; null to load all users</param>
        /// <param name="lastName">Last name; null to load all users</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all users</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all users</param>
        /// <param name="company">Company; null to load all users</param>
        /// <param name="phone">Phone; null to load all users</param>
        /// <param name="zipPostalCode">Phone; null to load all users</param>
        /// <param name="ipAddress">IP address; null to load all users</param>
        /// <param name="loadOnlyWithShoppingCart">Value indicating whether to load users only with shopping cart</param>
        /// <param name="sct">Value indicating what shopping cart type to filter; userd when 'loadOnlyWithShoppingCart' param is 'true'</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>Users</returns>
        public virtual IPagedList<User> GetAllUsers(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            Guid[] userRoleIds = null, string email = null, string username = null,
            string ipAddress = null, 
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _userRepository.Table;
            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);

            query = query.Where(c => !c.Deleted);

            if (userRoleIds != null && userRoleIds.Length > 0)
            {
                query = query.Join(_userUserRoleMappingRepository.Table, x => x.Id, y => y.UserId,
                        (x, y) => new {User = x, Mapping = y})
                    .Where(z => userRoleIds.Contains(z.Mapping.UserRoleId))
                    .Select(z => z.User)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));

            //search by IpAddress
            if (!string.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                    query = query.Where(w => w.LastIpAddress == ipAddress);
            }
            
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var users = new PagedList<User>(query, pageIndex, pageSize, getOnlyTotalCount);
            return users;
        }

        /// <summary>
        /// Gets online users
        /// </summary>
        /// <param name="lastActivityFromUtc">User last activity date (from)</param>
        /// <param name="userRoleIds">A list of user role identifiers to filter by (at least one match); pass null or empty list in order to load all users; </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Users</returns>
        public virtual IPagedList<User> GetOnlineUsers(DateTime lastActivityFromUtc,
            Guid[] userRoleIds, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _userRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);
            if (userRoleIds != null && userRoleIds.Length > 0)
                query = query.Where(c => c.UserUserRoleMappings.Select(mapping => mapping.UserRoleId).Intersect(userRoleIds).Any());
            
            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var users = new PagedList<User>(query, pageIndex, pageSize);
            return users;
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void DeleteUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.IsSystemAccount)
                throw new NopException($"System user account ({user.SystemName}) could not be deleted");

            user.Deleted = true;

            if (_userSettings.SuffixDeletedUsers)
            {
                if (!string.IsNullOrEmpty(user.Email))
                    user.Email += "-DELETED";
                if (!string.IsNullOrEmpty(user.Username))
                    user.Username += "-DELETED";
            }

            UpdateUser(user);

            //event notification
            _eventPublisher.EntityDeleted(user);
        }

        /// <summary>
        /// Gets a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>A user</returns>
        public virtual User GetUserById(Guid userId)
        {
            if (userId == default(Guid))
                return null;
            
            return _userRepository.GetById(userId);
        }

        /// <summary>
        /// Get users by identifiers
        /// </summary>
        /// <param name="userIds">User identifiers</param>
        /// <returns>Users</returns>
        public virtual IList<User> GetUsersByIds(Guid[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return new List<User>();

            var query = from c in _userRepository.Table
                        where userIds.Contains(c.Id) && !c.Deleted
                        select c;
            var users = query.ToList();
            //sort by passed identifiers
            var sortedUsers = new List<User>();
            foreach (var id in userIds)
            {
                var user = users.Find(x => x.Id == id);
                if (user != null)
                    sortedUsers.Add(user);
            }
            return sortedUsers;
        }
        
        /// <summary>
        /// Gets a user by GUID
        /// </summary>
        /// <param name="userGuid">User GUID</param>
        /// <returns>A user</returns>
        public virtual User GetUserByGuid(Guid userGuid)
        {
            if (userGuid == Guid.Empty)
                return null;

            var query = from c in _userRepository.Table
                        where c.UserGuid == userGuid
                        orderby c.Id
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>User</returns>
        public virtual User GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.Email == email
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>User</returns>
        public virtual User GetUserBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User</returns>
        public virtual User GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in _userRepository.Table
                        orderby c.Id
                        where c.Username == username
                        select c;
            var user = query.FirstOrDefault();
            return user;
        }
        
        /// <summary>
        /// Insert a guest user
        /// </summary>
        /// <returns>User</returns>
        public virtual User InsertGuestUser()
        {
            var user = new User
            {
                UserGuid = Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            //add to 'Guests' role
            var guestRole = GetUserRoleBySystemName(SystemUserRoleNames.Guests);
            if (guestRole == null)
                throw new NopException("'Guests' role could not be loaded");
            //user.UserRoles.Add(guestRole);
            user.UserUserRoleMappings.Add(new UserUserRoleMapping { UserRole = guestRole });

            _userRepository.Insert(user);

            return user;
        }
        
        /// <summary>
        /// Insert a user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void InsertUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _userRepository.Insert(user);

            //event notification
            _eventPublisher.EntityInserted(user);
        }
        
        /// <summary>
        /// Updates the user
        /// </summary>
        /// <param name="user">User</param>
        public virtual void UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _userRepository.Update(user);

            //event notification
            _eventPublisher.EntityUpdated(user);
        }

        /// <summary>
        /// Delete guest user records
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="onlyWithoutShoppingCart">A value indicating whether to delete users only without shopping cart</param>
        /// <returns>Number of deleted users</returns>
        public virtual int DeleteGuestUsers(DateTime? createdFromUtc, DateTime? createdToUtc, bool onlyWithoutShoppingCart)
        {
            //prepare parameters
            var pOnlyWithoutShoppingCart = _dataProvider.GetBooleanParameter("OnlyWithoutShoppingCart", onlyWithoutShoppingCart);
            var pCreatedFromUtc = _dataProvider.GetDateTimeParameter("CreatedFromUtc", createdFromUtc);
            var pCreatedToUtc = _dataProvider.GetDateTimeParameter("CreatedToUtc", createdToUtc);
            var pTotalRecordsDeleted = _dataProvider.GetOutputInt32Parameter("TotalRecordsDeleted");

            //invoke sited procedure
            _dbContext.ExecuteSqlCommand(
                "EXEC [DeleteGuests] @OnlyWithoutShoppingCart, @CreatedFromUtc, @CreatedToUtc, @TotalRecordsDeleted OUTPUT",
                false, null,
                pOnlyWithoutShoppingCart,
                pCreatedFromUtc,
                pCreatedToUtc,
                pTotalRecordsDeleted);

            var totalRecordsDeleted = pTotalRecordsDeleted.Value != DBNull.Value ? Convert.ToInt32(pTotalRecordsDeleted.Value) : 0;
            return totalRecordsDeleted;
        }
        
        #endregion

        #region User roles

        /// <summary>
        /// Delete a user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void DeleteUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            if (userRole.IsSystemRole)
                throw new NopException("System role could not be deleted");

            _userRoleRepository.Delete(userRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(userRole);
        }

        /// <summary>
        /// Gets a user role
        /// </summary>
        /// <param name="userRoleId">User role identifier</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleById(Guid userRoleId)
        {
            if (userRoleId == default(Guid))
                return null;

            return _userRoleRepository.GetById(userRoleId);
        }

        /// <summary>
        /// Gets a user role
        /// </summary>
        /// <param name="systemName">User role system name</param>
        /// <returns>User role</returns>
        public virtual UserRole GetUserRoleBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var key = string.Format(CUSTOMERROLES_BY_SYSTEMNAME_KEY, systemName);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _userRoleRepository.Table
                            orderby cr.Id
                            where cr.SystemName == systemName
                            select cr;
                var userRole = query.FirstOrDefault();
                return userRole;
            });
        }

        /// <summary>
        /// Gets all user roles
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>User roles</returns>
        public virtual IList<UserRole> GetAllUserRoles(bool showHidden = false)
        {
            var key = string.Format(CUSTOMERROLES_ALL_KEY, showHidden);
            return _cacheManager.Get(key, () =>
            {
                var query = from cr in _userRoleRepository.Table
                            orderby cr.Name
                            where showHidden || cr.Active
                            select cr;
                var userRoles = query.ToList();
                return userRoles;
            });
        }
        
        /// <summary>
        /// Inserts a user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void InsertUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            _userRoleRepository.Insert(userRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(userRole);
        }

        /// <summary>
        /// Updates the user role
        /// </summary>
        /// <param name="userRole">User role</param>
        public virtual void UpdateUserRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            _userRoleRepository.Update(userRole);

            _cacheManager.RemoveByPattern(CUSTOMERROLES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(userRole);
        }

        #endregion

        #region User passwords

        /// <summary>
        /// Gets user passwords
        /// </summary>
        /// <param name="userId">User identifier; pass null to load all records</param>
        /// <param name="passwordFormat">Password format; pass null to load all records</param>
        /// <param name="passwordsToReturn">Number of returning passwords; pass null to load all records</param>
        /// <returns>List of user passwords</returns>
        public virtual IList<UserPassword> GetUserPasswords(Guid? userId = null, 
            PasswordFormat? passwordFormat = null, int? passwordsToReturn = null)
        {
            var query = _userPasswordRepository.Table;

            //filter by user
            if (userId.HasValue)
                query = query.Where(password => password.UserId == userId.Value);

            //filter by password format
            if (passwordFormat.HasValue)
                query = query.Where(password => password.PasswordFormatId == (int)(passwordFormat.Value));

            //get the latest passwords
            if (passwordsToReturn.HasValue)
                query = query.OrderByDescending(password => password.CreatedOnUtc).Take(passwordsToReturn.Value);

            return query.ToList();
        }

        /// <summary>
        /// Get current user password
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User password</returns>
        public virtual UserPassword GetCurrentPassword(Guid userId)
        {
            if (userId == default(Guid))
                return null;

            //return the latest password
            return GetUserPasswords(userId, passwordsToReturn: 1).FirstOrDefault();
        }

        /// <summary>
        /// Insert a user password
        /// </summary>
        /// <param name="userPassword">User password</param>
        public virtual void InsertUserPassword(UserPassword userPassword)
        {
            if (userPassword == null)
                throw new ArgumentNullException(nameof(userPassword));

            _userPasswordRepository.Insert(userPassword);

            //event notification
            _eventPublisher.EntityInserted(userPassword);
        }

        /// <summary>
        /// Update a user password
        /// </summary>
        /// <param name="userPassword">User password</param>
        public virtual void UpdateUserPassword(UserPassword userPassword)
        {
            if (userPassword == null)
                throw new ArgumentNullException(nameof(userPassword));

            _userPasswordRepository.Update(userPassword);

            //event notification
            _eventPublisher.EntityUpdated(userPassword);
        }

        #endregion

        #endregion
    }
}