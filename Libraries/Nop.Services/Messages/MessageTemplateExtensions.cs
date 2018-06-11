using System.Collections.Generic;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Represents message template  extensions
    /// </summary>
    public static class MessageTemplateExtensions
    {
        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public static IEnumerable<string> GetTokenGroups(this MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            switch (messageTemplate.Name)
            {
                case MessageTemplateSystemNames.UserRegisteredNotification:
                case MessageTemplateSystemNames.UserWelcomeMessage:
                case MessageTemplateSystemNames.UserEmailValidationMessage:
                case MessageTemplateSystemNames.UserEmailRevalidationMessage:
                case MessageTemplateSystemNames.UserPasswordRecoveryMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderPlacedVendorNotification:
                case MessageTemplateSystemNames.OrderPlacedSiteOwnerNotification:
                case MessageTemplateSystemNames.OrderPlacedAffiliateNotification:
                case MessageTemplateSystemNames.OrderPaidSiteOwnerNotification:
                case MessageTemplateSystemNames.OrderPaidUserNotification:
                case MessageTemplateSystemNames.OrderPaidVendorNotification:
                case MessageTemplateSystemNames.OrderPaidAffiliateNotification:
                case MessageTemplateSystemNames.OrderPlacedUserNotification:
                case MessageTemplateSystemNames.OrderCompletedUserNotification:
                case MessageTemplateSystemNames.OrderCancelledUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.ShipmentSentUserNotification:
                case MessageTemplateSystemNames.ShipmentDeliveredUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ShipmentTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.OrderRefundedSiteOwnerNotification:
                case MessageTemplateSystemNames.OrderRefundedUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.RefundedOrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewOrderNoteAddedUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.OrderNoteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.RecurringPaymentCancelledSiteOwnerNotification:
                case MessageTemplateSystemNames.RecurringPaymentCancelledUserNotification:
                case MessageTemplateSystemNames.RecurringPaymentFailedUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens, TokenGroupNames.RecurringPaymentTokens };

                case MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage:
                case MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.SubscriptionTokens };

                case MessageTemplateSystemNames.EmailAFriendMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductTokens, TokenGroupNames.EmailAFriendTokens };

                case MessageTemplateSystemNames.WishlistToFriendMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens, TokenGroupNames.WishlistToFriendTokens };

                case MessageTemplateSystemNames.NewReturnRequestSiteOwnerNotification:
                case MessageTemplateSystemNames.NewReturnRequestUserNotification:
                case MessageTemplateSystemNames.ReturnRequestStatusChangedUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.UserTokens, TokenGroupNames.ReturnRequestTokens };

                case MessageTemplateSystemNames.NewForumTopicMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewForumPostMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ForumPostTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.PrivateMessageNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewVendorAccountApplySiteOwnerNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.VendorInformationChangeNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.VendorTokens };

                case MessageTemplateSystemNames.GiftCardNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.GiftCardTokens};

                case MessageTemplateSystemNames.ProductReviewSiteOwnerNotification:
                case MessageTemplateSystemNames.ProductReviewReplyUserNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ProductReviewTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.QuantityBelowSiteOwnerNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ProductTokens };

                case MessageTemplateSystemNames.QuantityBelowAttributeCombinationSiteOwnerNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ProductTokens, TokenGroupNames.AttributeCombinationTokens };

                case MessageTemplateSystemNames.NewVatSubmittedSiteOwnerNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens, TokenGroupNames.VatValidation };

                case MessageTemplateSystemNames.BlogCommentNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.BlogCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.NewsCommentNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.UserTokens };

                case MessageTemplateSystemNames.BackInStockNotification:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.UserTokens, TokenGroupNames.ProductBackInStockTokens };

                case MessageTemplateSystemNames.ContactUsMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ContactUs };

                case MessageTemplateSystemNames.ContactVendorMessage:
                    return new[] { TokenGroupNames.SiteTokens, TokenGroupNames.ContactVendor };

                default:
                    return new string[] { };
            }
        }
    }
}