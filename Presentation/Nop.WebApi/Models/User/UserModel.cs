using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.WebApi.Models.User
{
    public class UserModel: BaseNopEntityModel
    {
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
    }
}
