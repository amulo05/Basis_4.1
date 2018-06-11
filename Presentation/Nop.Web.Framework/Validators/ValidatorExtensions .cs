using FluentValidation;
using Nop.Core.Domain.Users;

namespace Nop.Web.Framework.Validators
{
    /// <summary>
    /// Validator extensions
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// Set credit card validator
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="ruleBuilder">RuleBuilder</param>
        /// <returns>Result</returns>
        public static IRuleBuilderOptions<T, string> IsCreditCard<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new CreditCardPropertyValidator());
        }

        /// <summary>
        /// Set username validator
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="ruleBuilder">RuleBuilder</param>
        /// <param name="userSettings">User settings</param>
        /// <returns>Result</returns>
        public static IRuleBuilderOptions<T, string> IsUsername<T>(this IRuleBuilder<T, string> ruleBuilder, UserSettings userSettings)
        {
            return ruleBuilder.SetValidator(new UsernamePropertyValidator(userSettings));
        }
    }
}
