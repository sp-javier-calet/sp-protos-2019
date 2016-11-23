using System.Collections.Generic;

namespace SocialPoint.Extension.Helpshift
{
    public struct HelpshiftConfiguration
    {
        // default params from DL
        public const string DefaultApiKey = "e80e75c9fd498d3274f3cdbc637b3866";
        public const string DefaultDomainName = "socialpoint.helpshift.com";
        public const string DefaultIosAppId = "socialpoint_platform_20151021095745155-1b0b401a75542a3";
        public const string DefaultAndroidAppId = "socialpoint_platform_20151021095745169-3fd755eb172b848";

        public enum ContactMode
        {
            CONTACT_US_ALWAYS,
            CONTACT_US_NEVER,
            CONTACT_US_AFTER_VIEWING_FAQS
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
            Mode = ContactMode.CONTACT_US_ALWAYS;
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
        HelpshiftConfiguration Configuration { get; }

        HelpshiftCustomer UserData { set; }

        bool IsEnabled{ get; }

        void Enable();

        void ShowFAQ(string sectionId = null);

        void ShowConversation();

        void OpenFromPush(string issueId);
    }
}