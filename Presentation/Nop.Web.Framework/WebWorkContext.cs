using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Users;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Sites;
using Nop.Services.Tasks;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Represents work context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        #region Const

        private const string CUSTOMER_COOKIE_NAME = ".Nop.User";

        #endregion

        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISiteContext _siteContext;
        private readonly ISiteMappingService _siteMappingService;
        private readonly IUserAgentHelper _userAgentHelper;

        private User _cachedUser;
        private User _originalUserIfImpersonated;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="authenticationService">Authentication service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="userService">User service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="siteContext">Site context</param>
        /// <param name="siteMappingService">Site mapping service</param>
        /// <param name="userAgentHelper">User gent helper</param>
        /// <param name="vendorService">Vendor service</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="taxSettings">Tax settings</param>
        public WebWorkContext(IHttpContextAccessor httpContextAccessor, 
            IAuthenticationService authenticationService,
            IUserService userService,
            IGenericAttributeService genericAttributeService,
            ISiteContext siteContext,
            ISiteMappingService siteMappingService,
            IUserAgentHelper userAgentHelper)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._authenticationService = authenticationService;
            this._userService = userService;
            this._genericAttributeService = genericAttributeService;
            this._siteContext = siteContext;
            this._siteMappingService = siteMappingService;
            this._userAgentHelper = userAgentHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get nop user cookie
        /// </summary>
        /// <returns>String value of cookie</returns>
        protected virtual string GetUserCookie()
        {
            return _httpContextAccessor.HttpContext?.Request?.Cookies[CUSTOMER_COOKIE_NAME];
        }

        /// <summary>
        /// Set nop user cookie
        /// </summary>
        /// <param name="userGuid">Guid of the user</param>
        protected virtual void SetUserCookie(Guid userGuid)
        {
            if (_httpContextAccessor.HttpContext?.Response == null)
                return;

            //delete current cookie value
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(CUSTOMER_COOKIE_NAME);

            //get date of cookie expiration
            var cookieExpires = 24 * 365; //TODO make configurable
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            //if passed guid is empty set cookie as expired
            if (userGuid == Guid.Empty)
                cookieExpiresDate = DateTime.Now.AddMonths(-1);

            //set new cookie value
            var options = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(CUSTOMER_COOKIE_NAME, userGuid.ToString(), options);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current user
        /// </summary>
        public virtual User CurrentUser
        {
            get
            {
                //whether there is a cached value
                if (_cachedUser != null)
                    return _cachedUser;

                User user = null;

                //check whether request is made by a background (schedule) task
                if (_httpContextAccessor.HttpContext == null ||
                    _httpContextAccessor.HttpContext.Request.Path.Equals(new PathString($"/{TaskManager.ScheduleTaskPath}"), StringComparison.InvariantCultureIgnoreCase))
                {
                    //in this case return built-in user record for background task
                    user = _userService.GetUserBySystemName(SystemUserNames.BackgroundTask);
                }

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //check whether request is made by a search engine, in this case return built-in user record for search engines
                    if (_userAgentHelper.IsSearchEngine())
                        user = _userService.GetUserBySystemName(SystemUserNames.SearchEngine);
                }

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //try to get registered user
                    user = _authenticationService.GetAuthenticatedUser();
                }

                if (user != null && !user.Deleted && user.Active && !user.RequireReLogin)
                {
                    //get impersonate user if required
                    var impersonatedUserId = user.GetAttribute<Guid?>(SystemUserAttributeNames.ImpersonatedUserId);
                    if (impersonatedUserId.HasValue && impersonatedUserId.Value != default(Guid))
                    {
                        var impersonatedUser = _userService.GetUserById(impersonatedUserId.Value);
                        if (impersonatedUser != null && !impersonatedUser.Deleted && impersonatedUser.Active && !impersonatedUser.RequireReLogin)
                        {
                            //set impersonated user
                            _originalUserIfImpersonated = user;
                            user = impersonatedUser;
                        }
                    }
                }

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //get guest user
                    var userCookie = GetUserCookie();
                    if (!string.IsNullOrEmpty(userCookie))
                    {
                        if (Guid.TryParse(userCookie, out Guid userGuid))
                        {
                            //get user from cookie (should not be registered)
                            var userByCookie = _userService.GetUserByGuid(userGuid);
                            if (userByCookie != null && !userByCookie.IsRegistered())
                                user = userByCookie;
                        }
                    }
                }

                if (user == null || user.Deleted || !user.Active || user.RequireReLogin)
                {
                    //create guest if not exists
                    user = _userService.InsertGuestUser();
                }

                if (!user.Deleted && user.Active && !user.RequireReLogin)
                {
                    //set user cookie
                    SetUserCookie(user.UserGuid);

                    //cache the found user
                    _cachedUser = user;
                }

                return _cachedUser;
            }
            set
            {
                SetUserCookie(value.UserGuid);
                _cachedUser = value;
            }
        }

        /// <summary>
        /// Gets the original user (in case the current one is impersonated)
        /// </summary>
        public virtual User OriginalUserIfImpersonated
        {
            get { return _originalUserIfImpersonated; }
        }

        /// <summary>
        /// Gets or sets value indicating whether we're in admin area
        /// </summary>
        public virtual bool IsAdmin { get; set; }

        #endregion
    }
}
