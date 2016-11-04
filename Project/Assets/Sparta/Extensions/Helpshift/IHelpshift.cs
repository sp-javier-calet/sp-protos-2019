using System.Collections.Generic;

namespace SocialPoint.Extension.Helpshift
{
    public struct HelpshiftConfiguration
    {
        public enum ContactMode
        {
            ALWAYS,
            AFTER_VIEWING_FAQS,
            AFTER_UNHELPFUL_ANSWER,
            NEVER
        }

        public string ApiKey { get; private set; }

        public string AppId { get; private set; }

        public string DomainName { get; private set; }

        public ContactMode Mode { get; set; }

        public bool InAppNotificationEnabled { get; set; }

        public bool SearchOnNewConversationEnabled { get; set; }

        public bool ConversationResolutionQuestionEnabled { get; set; }

        public HelpshiftConfiguration(string apiKey, string appId, string domainName) : this()
        {
            ApiKey = apiKey;
            AppId = appId;
            DomainName = domainName;
            Mode = ContactMode.AFTER_VIEWING_FAQS;
        }
    }

    public class HelpshiftCustomer
    {
        public string UserId { get; private set; }

        public string[] CustomerTags { get; set; }

        public Dictionary<string, object> CustomMetaData { get; private set; }

        public HelpshiftCustomer(string userId, string[] tags, Dictionary<string, object> customMetaData)
        {
            UserId = userId;
            CustomerTags = tags;
            CustomMetaData = new Dictionary<string, object>(customMetaData);
        }
    }

    public interface IHelpshift
    {
        void Setup(string gameObjectName, HelpshiftConfiguration config, HelpshiftCustomer customerData = null);

        void SetCustomerData(HelpshiftCustomer customerData);

        void SetLanguage(string locale);

        void ShowFAQ(string sectionId = null);

        void ShowConversation();

        void RegisterPushNotificationToken(string token);
    }
}