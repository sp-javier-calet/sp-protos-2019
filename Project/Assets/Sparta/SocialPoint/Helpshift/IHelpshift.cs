﻿using System.Collections.Generic;

namespace SocialPoint.Helpshift
{
    public struct HelpshiftConfiguration
    {
        public enum ContactMode
        {
            CONTACT_US_ALWAYS,
            CONTACT_US_NEVER,
            CONTACT_US_AFTER_VIEWING_FAQS
        }

        public ContactMode Mode { get; set; }

        public bool InAppNotificationEnabled { get; set; }

        public bool SearchOnNewConversationEnabled { get; set; }

        public bool ConversationResolutionQuestionEnabled { get; set; }
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
    }
}