using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Messages;
using Nop.Services.Users;
using Nop.Services.Events;
using Nop.Services.Sites;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ITokenizer _tokenizer;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ISiteService _siteService;
        private readonly ISiteContext _siteContext;
        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="messageTemplateService">Message template service</param>
        /// <param name="queuedEmailService">Queued email service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="tokenizer">Tokenizer</param>
        /// <param name="emailAccountService">Email account service</param>
        /// <param name="messageTokenProvider">Message token provider</param>
        /// <param name="siteService">Site service</param>
        /// <param name="siteContext">Site context</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="emailAccountSettings">Email account settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="affiliateService">Affiliate service</param>
        public WorkflowMessageService(IMessageTemplateService messageTemplateService,
            IQueuedEmailService queuedEmailService,
            ITokenizer tokenizer,
            IEmailAccountService emailAccountService,
            IMessageTokenProvider messageTokenProvider,
            ISiteService siteService,
            ISiteContext siteContext,
            CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IEventPublisher eventPublisher)
        {
            this._messageTemplateService = messageTemplateService;
            this._queuedEmailService = queuedEmailService;
            this._tokenizer = tokenizer;
            this._emailAccountService = emailAccountService;
            this._messageTokenProvider = messageTokenProvider;
            this._siteService = siteService;
            this._siteContext = siteContext;
            this._commonSettings = commonSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get active message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="siteId">Site identifier</param>
        /// <returns>List of message templates</returns>
        protected virtual IList<MessageTemplate> GetActiveMessageTemplates(string messageTemplateName, Guid siteId)
        {
            //get message templates by the name
            var messageTemplates = _messageTemplateService.GetMessageTemplatesByName(messageTemplateName, siteId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>EmailAccount</returns>
        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate)
        {
            var emailAccountId = messageTemplate.EmailAccountId;
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = _emailAccountService.GetEmailAccountById(emailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;
        }

        #endregion

        #region Methods

        #region User workflow

        /// <summary>
        /// Sends 'New user' notification message to a site owner
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<Guid> SendUserRegisteredNotificationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var site = _siteContext.CurrentSite;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserRegisteredNotification, site.Id);
            if (!messageTemplates.Any())
                return new List<Guid>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddSiteTokens(tokens, site, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends a welcome message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<Guid> SendUserWelcomeMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var site = _siteContext.CurrentSite;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserWelcomeMessage, site.Id);
            if (!messageTemplates.Any())
                return new List<Guid>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddSiteTokens(tokens, site, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = user.GetFullName();

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends an email validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<Guid> SendUserEmailValidationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var site = _siteContext.CurrentSite;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserEmailValidationMessage, site.Id);
            if (!messageTemplates.Any())
                return new List<Guid>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddSiteTokens(tokens, site, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = user.GetFullName();

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends an email re-validation message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<Guid> SendUserEmailRevalidationMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var site = _siteContext.CurrentSite;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserEmailRevalidationMessage, site.Id);
            if (!messageTemplates.Any())
                return new List<Guid>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddSiteTokens(tokens, site, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                //email to re-validate
                var toEmail = user.EmailToRevalidate;
                var toName = user.GetFullName();

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

        /// <summary>
        /// Sends password recovery message to a user
        /// </summary>
        /// <param name="user">User instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>Queued email identifier</returns>
        public virtual IList<Guid> SendUserPasswordRecoveryMessage(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var site = _siteContext.CurrentSite;

            var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.UserPasswordRecoveryMessage, site.Id);
            if (!messageTemplates.Any())
                return new List<Guid>();

            //tokens
            var commonTokens = new List<Token>();
            _messageTokenProvider.AddUserTokens(commonTokens, user);

            return messageTemplates.Select(messageTemplate =>
            {
                //email account
                var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate);

                var tokens = new List<Token>(commonTokens);
                _messageTokenProvider.AddSiteTokens(tokens, site, emailAccount);

                //event notification
                _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                var toEmail = user.Email;
                var toName = user.GetFullName();

                return SendNotification(messageTemplate, emailAccount, tokens, toEmail, toName);
            }).ToList();
        }

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
        public virtual Guid SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = messageTemplate.BccEmailAddresses;
            if (string.IsNullOrEmpty(subject))
                subject = messageTemplate.Subject;
            var body = messageTemplate.Body;

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOn = DateTime.Now,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDate = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.Now + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        #endregion

        #endregion
    }
}