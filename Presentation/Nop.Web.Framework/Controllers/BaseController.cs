using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Sites;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.UI;

namespace Nop.Web.Framework.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    //[PublishModelEvents]
    //[SignOutFromExternalAuthentication]
    //[ValidatePassword]
    //[SaveIpAddress]
    public abstract class BaseController : ControllerBase
    {
        [ProducesResponseType(200, Type = typeof(ApiResult))]
        public IActionResult Result(object obj, bool success = true, int statecode = 0, string message = "")
        {
            return Ok(new ApiResult()
            {
                Result = obj,
                StateCode = statecode,
                Success = success,
                Message = message
            });
        }
    }
}