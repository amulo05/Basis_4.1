using System;
using Nop.Core.Domain.Users;
using Nop.Services.Tasks;

namespace Nop.Services.Users
{
    /// <summary>
    /// Represents a task for deleting guest users
    /// </summary>
    public partial class DeleteGuestsTask : IScheduleTask
    {
        private readonly IUserService _userService;
        private readonly UserSettings _userSettings;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService">User service</param>
        /// <param name="userSettings">User settings</param>
        public DeleteGuestsTask(IUserService userService,
            UserSettings userSettings)
        {
            this._userService = userService;
            this._userSettings = userSettings;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanMinutes = _userSettings.DeleteGuestTaskOlderThanMinutes;
            // Default value in case 0 is returned.  0 would effectively disable this service and harm performance.
            olderThanMinutes = olderThanMinutes == 0 ? 1440 : olderThanMinutes;
    
            _userService.DeleteGuestUsers(null, DateTime.UtcNow.AddMinutes(-olderThanMinutes), true);
        }
    }
}
