using System.Collections.Generic;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Sites;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial interface IMessageTokenProvider
    {
        /// <summary>
        /// Add site tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="site">Site</param>
        /// <param name="emailAccount">Email account</param>
        void AddSiteTokens(IList<Token> tokens, Site site, EmailAccount emailAccount);

        /// <summary>
        /// Add user tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="user">User</param>
        void AddUserTokens(IList<Token> tokens, User user);

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>Collection of allowed (supported) message tokens for campaigns</returns>
        IEnumerable<string> GetListOfCampaignAllowedTokens();

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null);
    }
}
