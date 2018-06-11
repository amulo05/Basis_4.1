using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Services.Users;
using Nop.Web.Framework.Controllers;

namespace Nop.WebApi.Controllers
{
    public class TestController : BaseController
    {

        #region Fields

        private readonly IUserService _userService;
        //private readonly IWorkContext _workContext;
        //private readonly ISiteContext _siteContext;

        #endregion

        #region Ctor

        public TestController(IUserService userService)
        {
            this._userService = userService;
        }

        #endregion

        // GET api/values
        [HttpGet]
        public ActionResult Get()
        {
            return Json(_userService.GetAllUserRoles().ToList()) ;
        }
    }
}
