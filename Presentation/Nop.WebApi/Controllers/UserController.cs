using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain;
using Nop.Core.Domain.Users;
using Nop.Services.Messages;
using Nop.Services.Users;
using Nop.Web.Models.User;
using Nop.WebApi.Factories;
using Nop.Services.Authentication;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Nop.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        #region Fields

        private readonly IUserModelFactory _userModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly UserSettings _userSettings;
        private readonly SiteInformationSettings _siteInformationSettings;

        #endregion

        #region Ctor

        public UserController(IUserModelFactory userModelFactory,
            IAuthenticationService authenticationService,
            IUserService userService,
            IUserRegistrationService userRegistrationService,
            UserSettings userSettings,
            SiteInformationSettings siteInformationSettings)
        {
            this._userModelFactory = userModelFactory;
            this._authenticationService = authenticationService;
            this._userRegistrationService = userRegistrationService;
            this._userService = userService;
            this._userSettings = userSettings;
            this._siteInformationSettings = siteInformationSettings;
        }

        #endregion

        #region Methods


        // GET api/values
        [HttpGet("get")]
        public IActionResult Get()
        {
            return Result(_userService.GetAllUserRoles().ToList());
        }


        #region Register
        [HttpPost("register")]
        public virtual IActionResult Register(RegisterModel model)
        {
            //check whether registration is allowed
            //var user = _workContext.CurrentUser;
            var user = new User()
            {
                Active = true,
                Deleted = false,
                LastActivityDate = DateTime.Now,
                CreatedOn = DateTime.Now,
            };

            if (ModelState.IsValid)
            {
                if (_userSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }

                var isApproved = _userSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new UserRegistrationRequest(user,
                    model.Username,
                    model.Email,
                    model.Phone,
                    model.Password,
                    _userSettings.DefaultPasswordFormat,
                    isApproved);
                var registrationResult = _userRegistrationService.RegisterUser(registrationRequest);
                if (registrationResult.Success)
                {
                    //login user now
                    //if (isApproved)
                    //    _authenticationService.SignIn(user, true);
                    var userModel = this._userModelFactory.PrepareUserModel(user);
                    return Result(userModel);
                }
                return Result(null, false,9, registrationResult.Errors.FirstOrDefault());
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        #endregion

        //#region My account / Change password

        //[HttpsRequirement(SslRequirement.Yes)]
        //public virtual IActionResult ChangePassword()
        //{
        //    if (!_workContext.CurrentUser.IsRegistered())
        //        return Challenge();

        //    var model = _userModelFactory.PrepareChangePasswordModel();

        //    //display the cause of the change password 
        //    if (_workContext.CurrentUser.PasswordIsExpired())
        //        ModelState.AddModelError(string.Empty, _localizationService.GetResource("Account.ChangePassword.PasswordIsExpired"));

        //    return View(model);
        //}

        //[HttpPost]
        //[PublicAntiForgery]
        //public virtual IActionResult ChangePassword(ChangePasswordModel model)
        //{
        //    if (!_workContext.CurrentUser.IsRegistered())
        //        return Challenge();

        //    var user = _workContext.CurrentUser;

        //    if (ModelState.IsValid)
        //    {
        //        var changePasswordRequest = new ChangePasswordRequest(user.Email,
        //            true, _userSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
        //        var changePasswordResult = _userRegistrationService.ChangePassword(changePasswordRequest);
        //        if (changePasswordResult.Success)
        //        {
        //            model.Result = _localizationService.GetResource("Account.ChangePassword.Success");
        //            return View(model);
        //        }

        //        //errors
        //        foreach (var error in changePasswordResult.Errors)
        //            ModelState.AddModelError("", error);
        //    }

        //    //If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //#endregion

        #endregion
    }
}
