using FluentValidation;
using FluentValidation.Attributes;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.WebApi.Validators.User
{
    [Validator(typeof(RegisterValidator))]
    public class RegisterValidator : BaseNopValidator<RegisterModel>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Phone).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
        }
    }
}
