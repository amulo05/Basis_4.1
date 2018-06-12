using System.Collections.Generic;
using Nop.Core.Domain.Users;
using Nop.WebApi.Models.User;

namespace Nop.WebApi.Factories
{
    /// <summary>
    /// Represents the interface of the customer model factory
    /// </summary>
    public partial interface IUserModelFactory
    {
        UserModel PrepareUserModel(User entity);
    }
}
