using System.Collections.Generic;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Messages;
using System;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial interface IWorkflowMessageService
    {
        #region User workflow

        /// <summary>
        /// Sends 'New user' notification message to a site owner
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<Guid> SendUserRegisteredNotificationMessage(User user);

        /// <summary>
        /// Sends a welcome message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<Guid> SendUserWelcomeMessage(User user);

        /// <summary>
        /// Sends an email validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<Guid> SendUserEmailValidationMessage(User user);

        /// <summary>
        /// Sends an email re-validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<Guid> SendUserEmailRevalidationMessage(User user);

        /// <summary>
        /// Sends password recovery message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        IList<Guid> SendUserPasswordRecoveryMessage(User user);

        #endregion

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        Guid SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null);

    }
}