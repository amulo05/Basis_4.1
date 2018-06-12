using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Users;
using Nop.WebApi.Models.User;

namespace Nop.WebApi.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial class UserModelFactory : IUserModelFactory
    {
        public UserModel PrepareUserModel(User entity)
        {
            var model = new UserModel()
            {
                Id = entity.Id,
                Email = entity.Email,
                Phone = entity.Phone,
                Username = entity.Username
            };
            return model;
        }
    }
}
