using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Sites;
using Nop.Services.Events;
using Nop.Services.Sites;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Message template service
    /// </summary>
    public partial class MessageTemplateService: IMessageTemplateService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : site ID
        /// </remarks>
        private const string MESSAGETEMPLATES_ALL_KEY = "Nop.messagetemplate.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : site ID
        /// </remarks>
        private const string MESSAGETEMPLATES_BY_NAME_KEY = "Nop.messagetemplate.name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MESSAGETEMPLATES_PATTERN_KEY = "Nop.messagetemplate.";

        #endregion

        #region Fields

        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<SiteMapping> _siteMappingRepository;
        private readonly ISiteMappingService _siteMappingService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="siteMappingRepository">Site mapping repository</param>
        /// <param name="languageService">Language service</param>
        /// <param name="localizedEntityService">Localized entity service</param>
        /// <param name="siteMappingService">Site mapping service</param>
        /// <param name="messageTemplateRepository">Message template repository</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        public MessageTemplateService(ICacheManager cacheManager,
            IRepository<SiteMapping> siteMappingRepository,
            ISiteMappingService siteMappingService,
            IRepository<MessageTemplate> messageTemplateRepository,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._siteMappingRepository = siteMappingRepository;
            this._siteMappingService = siteMappingService;
            this._messageTemplateRepository = messageTemplateRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void DeleteMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            _messageTemplateRepository.Delete(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(messageTemplate);
        }

        /// <summary>
        /// Inserts a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void InsertMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            _messageTemplateRepository.Insert(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(messageTemplate);
        }

        /// <summary>
        /// Updates a message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        public virtual void UpdateMessageTemplate(MessageTemplate messageTemplate)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            _messageTemplateRepository.Update(messageTemplate);

            _cacheManager.RemoveByPattern(MESSAGETEMPLATES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(messageTemplate);
        }

        /// <summary>
        /// Gets a message template
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <returns>Message template</returns>
        public virtual MessageTemplate GetMessageTemplateById(Guid messageTemplateId)
        {
            if (messageTemplateId == default(Guid))
                return null;

            return _messageTemplateRepository.GetById(messageTemplateId);
        }

        /// <summary>
        /// Gets message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="siteId">Site identifier; pass null to load all records</param>
        /// <returns>List of message templates</returns>
        public virtual IList<MessageTemplate> GetMessageTemplatesByName(string messageTemplateName, Guid? siteId = null)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException(nameof(messageTemplateName));

            var key = string.Format(MESSAGETEMPLATES_BY_NAME_KEY, messageTemplateName, siteId ?? default(Guid));
            return _cacheManager.Get(key, () =>
            {
                //get message templates with the passed name
                var templates = _messageTemplateRepository.Table
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName))
                    .OrderBy(messageTemplate => messageTemplate.Id).ToList();

                //filter by the site
                if (siteId.HasValue && siteId.Value != default(Guid))
                    templates = templates.Where(messageTemplate => _siteMappingService.Authorize(messageTemplate, siteId.Value)).ToList();

                return templates;
            });
        }

        /// <summary>
        /// Gets all message templates
        /// </summary>
        /// <param name="siteId">Site identifier; pass 0 to load all records</param>
        /// <returns>Message template list</returns>
        public virtual IList<MessageTemplate> GetAllMessageTemplates(Guid siteId)
        {
            var key = string.Format(MESSAGETEMPLATES_ALL_KEY, siteId);
            return _cacheManager.Get(key, () =>
            {
                var query = _messageTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                //Site mapping
                if (siteId != default(Guid))
                {
                    query = from t in query
                            join sm in _siteMappingRepository.Table
                            on new { c1 = t.Id, c2 = "MessageTemplate" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into tSm
                            from sm in tSm.DefaultIfEmpty()
                            where !t.LimitedToSites || siteId == sm.SiteId
                            select t;
                    
                    query = query.Distinct().OrderBy(t => t.Name);
                }

                return query.ToList();
            });
        }

        #endregion
    }
}
