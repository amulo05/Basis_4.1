using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.WebApi.Validators.User;

namespace Nop.Web.Models.User
{
    [Validator(typeof(RegisterValidator))]
    public partial class RegisterModel : BaseNopModel
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }
    }
}