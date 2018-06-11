using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Users;
using Nop.Services.Users;

namespace Nop.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        #region Fields

        private readonly IUserService _userService;
        //private readonly IWorkContext _workContext;
        //private readonly ISiteContext _siteContext;

        #endregion

        #region Ctor

        public ValuesController(IUserService userService)
        {
            this._userService = userService;
        }

        #endregion

        // GET api/values
        [HttpGet]
        public ActionResult<IList<UserRole>> Get()
        {
            return _userService.GetAllUserRoles().ToList();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
