using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Sites;

namespace Nop.Services.Users
{
    /// <summary>
    /// User registration service
    /// </summary>
    public partial class UserRegistrationService : IUserRegistrationService
    {
        #region Fields

        private const int SALT_KEY_SIZE = 5;

        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptionService;
        private readonly ISiteService _siteService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEventPublisher _eventPublisher;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly UserSettings _userSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService">User service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="newsLetterSubscriptionService">Newsletter subscription service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="siteService">Site service</param>
        /// <param name="rewardPointService">Reward points service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="userSettings">User settings</param>
        public UserRegistrationService(IUserService userService, 
            IEncryptionService encryptionService, 
            ISiteService siteService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IWorkflowMessageService workflowMessageService,
            IEventPublisher eventPublisher,
            RewardPointsSettings rewardPointsSettings,
            UserSettings userSettings)
        {
            this._userService = userService;
            this._encryptionService = encryptionService;
            this._siteService = siteService;
            this._genericAttributeService = genericAttributeService;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._eventPublisher = eventPublisher;
            this._rewardPointsSettings = rewardPointsSettings;
            this._userSettings = userSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether the entered password matches with a saved one
        /// </summary>
        /// <param name="userPassword">User password</param>
        /// <param name="enteredPassword">The entered password</param>
        /// <returns>True if passwords match; otherwise false</returns>
        protected bool PasswordsMatch(UserPassword userPassword, string enteredPassword)
        {
            if (userPassword == null || string.IsNullOrEmpty(enteredPassword))
                return false;

            var savedPassword = string.Empty;
            switch (userPassword.PasswordFormat)
            {
                case PasswordFormat.Clear:
                    savedPassword = enteredPassword;
                    break;
                case PasswordFormat.Encrypted:
                    savedPassword = _encryptionService.EncryptText(enteredPassword);
                    break;
                case PasswordFormat.Hashed:
                    savedPassword = _encryptionService.CreatePasswordHash(enteredPassword, userPassword.PasswordSalt, _userSettings.HashedPasswordFormat);
                    break;
            }

            if (userPassword.Password == null)
                return false;

            return userPassword.Password.Equals(savedPassword);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validate user
        /// </summary>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">Password</param>
        /// <returns>Result</returns>
        public virtual UserLoginResults ValidateUser(string usernameOrEmail, string password)
        {
            var user = _userSettings.UsernamesEnabled ? 
                _userService.GetUserByUsername(usernameOrEmail) :
                _userService.GetUserByEmail(usernameOrEmail);

            if (user == null)
                return UserLoginResults.UserNotExist;
            if (user.Deleted)
                return UserLoginResults.Deleted;
            if (!user.Active)
                return UserLoginResults.NotActive;
            //only registered can login
            if (!user.IsRegistered())
                return UserLoginResults.NotRegistered;
            //check whether a user is locked out
            if (user.CannotLoginUntilDateUtc.HasValue && user.CannotLoginUntilDateUtc.Value > DateTime.UtcNow)
                return UserLoginResults.LockedOut;

            if (!PasswordsMatch(_userService.GetCurrentPassword(user.Id), password))
            {
                //wrong password
                user.FailedLoginAttempts++;
                if (_userSettings.FailedPasswordAllowedAttempts > 0 &&
                    user.FailedLoginAttempts >= _userSettings.FailedPasswordAllowedAttempts)
                {
                    //lock out
                    user.CannotLoginUntilDateUtc = DateTime.UtcNow.AddMinutes(_userSettings.FailedPasswordLockoutMinutes);
                    //reset the counter
                    user.FailedLoginAttempts = 0;
                }
                _userService.UpdateUser(user);

                return UserLoginResults.WrongPassword;
            }

            //update login details
            user.FailedLoginAttempts = 0;
            user.CannotLoginUntilDateUtc = null;
            user.RequireReLogin = false;
            user.LastLoginDateUtc = DateTime.UtcNow;
            _userService.UpdateUser(user);

            return UserLoginResults.Successful;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual UserRegistrationResult RegisterUser(UserRegistrationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.User == null)
                throw new ArgumentException("Can't load current user");

            var result = new UserRegistrationResult();
            if (request.User.IsSearchEngineAccount())
            {
                result.AddError("Search engine can't be registered");
                return result;
            }
            if (request.User.IsBackgroundTaskAccount())
            {
                result.AddError("Background task account can't be registered");
                return result;
            }
            if (request.User.IsRegistered())
            {
                result.AddError("Current user is already registered");
                return result;
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                result.AddError("Email is required.");
                return result;
            }
            if (!CommonHelper.IsValidEmail(request.Email))
            {
                result.AddError("Wrong email");
                return result;
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                result.AddError("Password is not provided");
                return result;
            }
            if (_userSettings.UsernamesEnabled)
            {
                if (string.IsNullOrEmpty(request.Username))
                {
                    result.AddError("Username is required.");
                    return result;
                }
            }

            //validate unique user
            if (_userService.GetUserByEmail(request.Email) != null)
            {
                result.AddError("The specified email already exists");
                return result;
            }
            if (_userSettings.UsernamesEnabled)
            {
                if (_userService.GetUserByUsername(request.Username) != null)
                {
                    result.AddError("The specified username already exists");
                    return result;
                }
            }

            //at this point request is valid
            request.User.Username = request.Username;
            request.User.Email = request.Email;

            var userPassword = new UserPassword
            {
                User = request.User,
                PasswordFormat = request.PasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.PasswordFormat)
            {
                case PasswordFormat.Clear:
                        userPassword.Password = request.Password;
                    break;
                case PasswordFormat.Encrypted:
                    userPassword.Password = _encryptionService.EncryptText(request.Password);
                    break;
                case PasswordFormat.Hashed:
                    {
                        var saltKey = _encryptionService.CreateSaltKey(SALT_KEY_SIZE);
                        userPassword.PasswordSalt = saltKey;
                        userPassword.Password = _encryptionService.CreatePasswordHash(request.Password, saltKey, _userSettings.HashedPasswordFormat);
                    }
                    break;
            }
            _userService.InsertUserPassword(userPassword);

            request.User.Active = request.IsApproved;
            
            //add to 'Registered' role
            var registeredRole = _userService.GetUserRoleBySystemName(SystemUserRoleNames.Registered);
            if (registeredRole == null)
                throw new NopException("'Registered' role could not be loaded");
            //request.User.UserRoles.Add(registeredRole);
            request.User.UserUserRoleMappings.Add(new UserUserRoleMapping { UserRole = registeredRole });
            //remove from 'Guests' role
            var guestRole = request.User.UserRoles.FirstOrDefault(cr => cr.SystemName == SystemUserRoleNames.Guests);
            if (guestRole != null)
            {
                //request.User.UserRoles.Remove(guestRole);
                request.User.UserUserRoleMappings
                    .Remove(request.User.UserUserRoleMappings.FirstOrDefault(mapping => mapping.UserRoleId == guestRole.Id));
            }

            _userService.UpdateUser(request.User);

            return result;
        }
        
        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Result</returns>
        public virtual ChangePasswordResult ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new ChangePasswordResult();
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                result.AddError("Email is not entered");
                return result;
            }
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                result.AddError("Password is not entered");
                return result;
            }

            var user = _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                result.AddError("The specified email could not be found");
                return result;
            }

            if (request.ValidateRequest)
            {
                //request isn't valid
                if (!PasswordsMatch(_userService.GetCurrentPassword(user.Id), request.OldPassword))
                {
                    result.AddError("Old password doesn't match");
                    return result;
                }
            }

            //check for duplicates
            if (_userSettings.UnduplicatedPasswordsNumber > 0)
            {
                //get some of previous passwords
                var previousPasswords = _userService.GetUserPasswords(user.Id, passwordsToReturn: _userSettings.UnduplicatedPasswordsNumber);

                var newPasswordMatchesWithPrevious = previousPasswords.Any(password => PasswordsMatch(password, request.NewPassword));
                if (newPasswordMatchesWithPrevious)
                {
                    result.AddError("You entered the password that is the same as one of the last passwords you used. Please create a new password.");
                    return result;
                }
            }

            //at this point request is valid
            var userPassword = new UserPassword
            {
                User = user,
                PasswordFormat = request.NewPasswordFormat,
                CreatedOnUtc = DateTime.UtcNow
            };
            switch (request.NewPasswordFormat)
            {
                case PasswordFormat.Clear:
                        userPassword.Password = request.NewPassword;
                    break;
                case PasswordFormat.Encrypted:
                        userPassword.Password = _encryptionService.EncryptText(request.NewPassword);
                    break;
                case PasswordFormat.Hashed:
                    {
                        var saltKey = _encryptionService.CreateSaltKey(SALT_KEY_SIZE);
                        userPassword.PasswordSalt = saltKey;
                        userPassword.Password = _encryptionService.CreatePasswordHash(request.NewPassword, saltKey, _userSettings.HashedPasswordFormat);
                    }
                    break;
            }
            _userService.InsertUserPassword(userPassword);

            //publish event
            _eventPublisher.Publish(new UserPasswordChangedEvent(userPassword));

            return result;
        }

        /// <summary>
        /// Sets a user email
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="newEmail">New email</param>
        /// <param name="requireValidation">Require validation of new email address</param>
        public virtual void SetEmail(User user, string newEmail, bool requireValidation)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (newEmail == null)
                throw new NopException("Email cannot be null");

            newEmail = newEmail.Trim();
            var oldEmail = user.Email;

            if (!CommonHelper.IsValidEmail(newEmail))
                throw new NopException("New email is not valid");

            if (newEmail.Length > 100)
                throw new NopException("E-mail address is too long");

            var user2 = _userService.GetUserByEmail(newEmail);
            if (user2 != null && user.Id != user2.Id)
                throw new NopException("The e-mail address is already in use");

            if (requireValidation)
            {
                //re-validate email
                user.EmailToRevalidate = newEmail;
                _userService.UpdateUser(user);

                //email re-validation message
                _genericAttributeService.SaveAttribute(user, SystemUserAttributeNames.EmailRevalidationToken, Guid.NewGuid().ToString());
                //_workflowMessageService.SendUserEmailRevalidationMessage(user, _workContext.WorkingLanguage.Id);
            }
            else
            {
                user.Email = newEmail;
                _userService.UpdateUser(user);
            }
        }

        /// <summary>
        /// Sets a user username
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="newUsername">New Username</param>
        public virtual void SetUsername(User user, string newUsername)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_userSettings.UsernamesEnabled)
                throw new NopException("Usernames are disabled");

            newUsername = newUsername.Trim();

            if (newUsername.Length > 100)
                throw new NopException("Username is too long");

            var user2 = _userService.GetUserByUsername(newUsername);
            if (user2 != null && user.Id != user2.Id)
                throw new NopException("The username is already in use");

            user.Username = newUsername;
            _userService.UpdateUser(user);
        }

        #endregion
    }
}