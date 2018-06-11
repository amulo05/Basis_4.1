using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Users;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Sites;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Users;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Sites;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly ISiteService _siteService;
        private readonly ISiteContext _siteContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        private readonly MessageTemplatesSettings _templatesSettings;

        private readonly IEventPublisher _eventPublisher;
        private readonly SiteInformationSettings _siteInformationSettings;

        private Dictionary<string, IEnumerable<string>> _allowedTokens;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="languageService">Language service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="dateTimeHelper">Datetime helper</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="downloadService">Download service</param>
        /// <param name="orderService">Order service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="siteService">Site service</param>
        /// <param name="siteContext">Site context</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="addressAttributeFormatter">Address attribute formatter</param>
        /// <param name="userAttributeFormatter">User attribute formatter</param>
        /// <param name="vendorAttributeFormatter">Vendor attribute formatter</param>
        /// <param name="urlHelperFactory">URL Helper factory</param>
        /// <param name="actionContextAccessor">Action context accessor</param>
        /// <param name="templatesSettings">Templates settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="siteInformationSettings">SiteInformation settings</param>
        public MessageTokenProvider(IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            IDownloadService downloadService,
            ISiteService siteService,
            ISiteContext siteContext,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            MessageTemplatesSettings templatesSettings,
            IEventPublisher eventPublisher,
            SiteInformationSettings siteInformationSettings)
        {
            this._dateTimeHelper = dateTimeHelper;
            this._workContext = workContext;
            this._downloadService = downloadService;
            this._urlHelperFactory = urlHelperFactory;
            this._actionContextAccessor = actionContextAccessor;
            this._siteService = siteService;
            this._siteContext = siteContext;

            this._templatesSettings = templatesSettings;
            this._eventPublisher = eventPublisher;
            this._siteInformationSettings = siteInformationSettings;
        }

        #endregion

        #region Allowed tokens

        /// <summary>
        /// Get all available tokens by token groups
        /// </summary>
        protected Dictionary<string, IEnumerable<string>> AllowedTokens
        {
            get
            {
                if (_allowedTokens != null)
                    return _allowedTokens;

                _allowedTokens = new Dictionary<string, IEnumerable<string>>();

                //site tokens
                _allowedTokens.Add(TokenGroupNames.SiteTokens, new[]
                {
                    "%Site.Name%",
                    "%Site.URL%",
                    "%Site.Email%",
                    "%Site.CompanyName%",
                    "%Site.CompanyAddress%",
                    "%Site.CompanyPhoneNumber%",
                    "%Site.CompanyVat%",
                    "%Facebook.URL%",
                    "%Twitter.URL%",
                    "%YouTube.URL%",
                    "%GooglePlus.URL%"
                });

                //user tokens
                _allowedTokens.Add(TokenGroupNames.UserTokens, new[]
                {
                    "%User.Email%",
                    "%User.Username%",
                    "%User.FullName%",
                    "%User.FirstName%",
                    "%User.LastName%",
                    "%User.VatNumber%",
                    "%User.VatNumberStatus%",
                    "%User.CustomAttributes%",
                    "%User.PasswordRecoveryURL%",
                    "%User.AccountActivationURL%",
                    "%User.EmailRevalidationURL%",
                    "%Wishlist.URLForUser%"
                });

                //order tokens
                _allowedTokens.Add(TokenGroupNames.OrderTokens, new[]
                {
                    "%Order.OrderNumber%",
                    "%Order.UserFullName%",
                    "%Order.UserEmail%",
                    "%Order.BillingFirstName%",
                    "%Order.BillingLastName%",
                    "%Order.BillingPhoneNumber%",
                    "%Order.BillingEmail%",
                    "%Order.BillingFaxNumber%",
                    "%Order.BillingCompany%",
                    "%Order.BillingAddress1%",
                    "%Order.BillingAddress2%",
                    "%Order.BillingCity%",
                    "%Order.BillingCounty%",
                    "%Order.BillingStateProvince%",
                    "%Order.BillingZipPostalCode%",
                    "%Order.BillingCountry%",
                    "%Order.BillingCustomAttributes%",
                    "%Order.Shippable%",
                    "%Order.ShippingMethod%",
                    "%Order.ShippingFirstName%",
                    "%Order.ShippingLastName%",
                    "%Order.ShippingPhoneNumber%",
                    "%Order.ShippingEmail%",
                    "%Order.ShippingFaxNumber%",
                    "%Order.ShippingCompany%",
                    "%Order.ShippingAddress1%",
                    "%Order.ShippingAddress2%",
                    "%Order.ShippingCity%",
                    "%Order.ShippingCounty%",
                    "%Order.ShippingStateProvince%",
                    "%Order.ShippingZipPostalCode%",
                    "%Order.ShippingCountry%",
                    "%Order.ShippingCustomAttributes%",
                    "%Order.PaymentMethod%",
                    "%Order.VatNumber%",
                    "%Order.CustomValues%",
                    "%Order.Product(s)%",
                    "%Order.CreatedOn%",
                    "%Order.OrderURLForUser%"
                });

                //shipment tokens
                _allowedTokens.Add(TokenGroupNames.ShipmentTokens, new[]
                {
                    "%Shipment.ShipmentNumber%",
                    "%Shipment.TrackingNumber%",
                    "%Shipment.TrackingNumberURL%",
                    "%Shipment.Product(s)%",
                    "%Shipment.URLForUser%"
                });

                //refunded order tokens
                _allowedTokens.Add(TokenGroupNames.RefundedOrderTokens, new[]
                {
                    "%Order.AmountRefunded%"
                });

                //order note tokens
                _allowedTokens.Add(TokenGroupNames.OrderNoteTokens, new[]
                {
                    "%Order.NewNoteText%",
                    "%Order.OrderNoteAttachmentUrl%"
                });

                //recurring payment tokens
                _allowedTokens.Add(TokenGroupNames.RecurringPaymentTokens, new[]
                {
                    "%RecurringPayment.ID%",
                    "%RecurringPayment.CancelAfterFailedPayment%",
                    "%RecurringPayment.RecurringPaymentType%"
                });

                //newsletter subscription tokens
                _allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new[]
                {
                    "%NewsLetterSubscription.Email%",
                    "%NewsLetterSubscription.ActivationUrl%",
                    "%NewsLetterSubscription.DeactivationUrl%"
                });

                //product tokens
                _allowedTokens.Add(TokenGroupNames.ProductTokens, new[]
                {
                    "%Product.ID%",
                    "%Product.Name%",
                    "%Product.ShortDescription%",
                    "%Product.ProductURLForUser%",
                    "%Product.SKU%",
                    "%Product.StockQuantity%"
                });

                //return request tokens
                _allowedTokens.Add(TokenGroupNames.ReturnRequestTokens, new[]
                {
                    "%ReturnRequest.CustomNumber%",
                    "%ReturnRequest.OrderId%",
                    "%ReturnRequest.Product.Quantity%",
                    "%ReturnRequest.Product.Name%",
                    "%ReturnRequest.Reason%",
                    "%ReturnRequest.RequestedAction%",
                    "%ReturnRequest.UserComment%",
                    "%ReturnRequest.StaffNotes%",
                    "%ReturnRequest.Status%"
                });

                //forum tokens
                _allowedTokens.Add(TokenGroupNames.ForumTokens, new[]
                {
                    "%Forums.ForumURL%",
                    "%Forums.ForumName%"
                });

                //forum topic tokens
                _allowedTokens.Add(TokenGroupNames.ForumTopicTokens, new[]
                {
                    "%Forums.TopicURL%",
                    "%Forums.TopicName%"
                });

                //forum post tokens
                _allowedTokens.Add(TokenGroupNames.ForumPostTokens, new[]
                {
                    "%Forums.PostAuthor%",
                    "%Forums.PostBody%"
                });

                //private message tokens
                _allowedTokens.Add(TokenGroupNames.PrivateMessageTokens, new[]
                {
                    "%PrivateMessage.Subject%",
                    "%PrivateMessage.Text%"
                });

                //vendor tokens
                _allowedTokens.Add(TokenGroupNames.VendorTokens, new[]
                {
                    "%Vendor.Name%",
                    "%Vendor.Email%",
                    "%Vendor.VendorAttributes%"
                });

                //gift card tokens
                _allowedTokens.Add(TokenGroupNames.GiftCardTokens, new[]
                {
                    "%GiftCard.SenderName%",
                    "%GiftCard.SenderEmail%",
                    "%GiftCard.RecipientName%",
                    "%GiftCard.RecipientEmail%",
                    "%GiftCard.Amount%",
                    "%GiftCard.CouponCode%",
                    "%GiftCard.Message%"
                });

                //product review tokens
                _allowedTokens.Add(TokenGroupNames.ProductReviewTokens, new[]
                {
                    "%ProductReview.ProductName%",
                    "%ProductReview.Title%",
                    "%ProductReview.IsApproved%",
                    "%ProductReview.ReviewText%",
                    "%ProductReview.ReplyText%"
                });

                //attribute combination tokens
                _allowedTokens.Add(TokenGroupNames.AttributeCombinationTokens, new[]
                {
                    "%AttributeCombination.Formatted%",
                    "%AttributeCombination.SKU%",
                    "%AttributeCombination.StockQuantity%"
                });

                //blog comment tokens
                _allowedTokens.Add(TokenGroupNames.BlogCommentTokens, new[]
                {
                    "%BlogComment.BlogPostTitle%"
                });

                //news comment tokens
                _allowedTokens.Add(TokenGroupNames.NewsCommentTokens, new[]
                {
                    "%NewsComment.NewsTitle%"
                });

                //product back in stock tokens
                _allowedTokens.Add(TokenGroupNames.ProductBackInStockTokens, new[]
                {
                    "%BackInStockSubscription.ProductName%",
                    "%BackInStockSubscription.ProductUrl%"
                });

                //email a friend tokens
                _allowedTokens.Add(TokenGroupNames.EmailAFriendTokens, new[]
                {
                    "%EmailAFriend.PersonalMessage%",
                    "%EmailAFriend.Email%"
                });

                //wishlist to friend tokens
                _allowedTokens.Add(TokenGroupNames.WishlistToFriendTokens, new[]
                {
                    "%Wishlist.PersonalMessage%",
                    "%Wishlist.Email%"
                });

                //VAT validation tokens
                _allowedTokens.Add(TokenGroupNames.VatValidation, new[]
                {
                    "%VatValidationResult.Name%",
                    "%VatValidationResult.Address%"
                });

                //contact us tokens
                _allowedTokens.Add(TokenGroupNames.ContactUs, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                //contact vendor tokens
                _allowedTokens.Add(TokenGroupNames.ContactVendor, new[]
                {
                    "%ContactUs.SenderEmail%",
                    "%ContactUs.SenderName%",
                    "%ContactUs.Body%"
                });

                return _allowedTokens;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Generates an absolute URL for the specified site, routeName and route values
        /// </summary>
        /// <param name="siteId">Site identifier; Pass 0 to load URL of the current site</param>
        /// <param name="routeName">The name of the route that is used to generate URL</param>
        /// <param name="routeValues">An object that contains route values</param>
        /// <returns>Generated URL</returns>
        protected virtual string RouteUrl(int siteId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a site by the passed identifier
            var site = _siteService.GetSiteById(siteId) ?? _siteContext.CurrentSite
                ?? throw new Exception("No site could be loaded");

            //ensure that the site URL is specified
            if (string.IsNullOrEmpty(site.Url))
                throw new Exception("URL cannot be null");
            
            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);
            
            //compose the result
            return Uri.EscapeUriString(WebUtility.UrlDecode($"{site.Url.TrimEnd('/')}{url}"));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add site tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="site">Site</param>
        /// <param name="emailAccount">Email account</param>
        public virtual void AddSiteTokens(IList<Token> tokens, Site site, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            tokens.Add(new Token("Site.Name", site.Name));
            tokens.Add(new Token("Site.URL", site.Url, true));
            tokens.Add(new Token("Site.Email", emailAccount.Email));

            //event notification
            _eventPublisher.EntityTokensAdded(site, tokens);
        }

        /// <summary>
        /// Add user tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="user">User</param>
        public virtual void AddUserTokens(IList<Token> tokens, User user)
        {
            tokens.Add(new Token("User.Email", user.Email));
            tokens.Add(new Token("User.Username", user.Username));
            tokens.Add(new Token("User.FullName", user.GetFullName()));
            tokens.Add(new Token("User.FirstName", user.GetAttribute<string>(SystemUserAttributeNames.FirstName)));
            tokens.Add(new Token("User.LastName", user.GetAttribute<string>(SystemUserAttributeNames.LastName)));
            
            //note: we do not use SEO friendly URLS for these links because we can get errors caused by having .(dot) in the URL (from the email address)
            var passwordRecoveryUrl = RouteUrl(routeName: "PasswordRecoveryConfirm", routeValues: new { token = user.GetAttribute<string>(SystemUserAttributeNames.PasswordRecoveryToken), email = user.Email });
            var accountActivationUrl = RouteUrl(routeName: "AccountActivation", routeValues: new { token = user.GetAttribute<string>(SystemUserAttributeNames.AccountActivationToken), email = user.Email });
            var emailRevalidationUrl = RouteUrl(routeName: "EmailRevalidation", routeValues: new { token = user.GetAttribute<string>(SystemUserAttributeNames.EmailRevalidationToken), email = user.Email });
            var wishlistUrl = RouteUrl(routeName: "Wishlist", routeValues: new { userGuid = user.UserGuid });
            tokens.Add(new Token("User.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("User.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("User.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForUser", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(user, tokens);
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>Collection of allowed (supported) message tokens for campaigns</returns>
        public virtual IEnumerable<string> GetListOfCampaignAllowedTokens()
        {
            var additionTokens = new CampaignAdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = GetListOfAllowedTokens(new[] { TokenGroupNames.SiteTokens, TokenGroupNames.SubscriptionTokens }).ToList();
            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>Collection of allowed message tokens</returns>
        public virtual IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups = null)
        {
            var additionTokens = new AdditionTokensAddedEvent();
            _eventPublisher.Publish(additionTokens);

            var allowedTokens = AllowedTokens.Where(x => tokenGroups == null || tokenGroups.Contains(x.Key))
                .SelectMany(x => x.Value).ToList();

            allowedTokens.AddRange(additionTokens.AdditionTokens);

            return allowedTokens.Distinct();
        }
        
        #endregion
    }
}
