using System;
using System.Collections.Generic;
using Helpshift;
using SocialPoint.Attributes;

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

        public bool SearchOnNewConversationEnabled { get; set; }

        public bool ConversationResolutionQuestionEnabled { get; set; }

        public Dictionary<string, object>[] Flows;

        public void ParseHelpshiftTopics(AttrDic topics)
        {
            var currentflows = new List<Dictionary<string, object>>();

            AttrList list = topics[GameData.HelpshiftKey].AsList;
            for(int i = 0; i < list.Count; ++i)
            {
                AttrDic currentTopic = list[i].AsDic;
                string topicTid = currentTopic.GetFallback("topic_tid", "");
                LocalizeKey keyTitle = new LocalizeKey(topicTid);

                string predefinedTextTid = currentTopic.GetFallback("predefined_text_tid", "");
                LocalizeKey keyText = new LocalizeKey(predefinedTextTid);

                Dictionary<string, object> conversationFlow = new Dictionary<string, object>();
                conversationFlow.Add(HelpshiftSdk.HsFlowType, HelpshiftSdk.HsFlowTypeConversation);
                conversationFlow.Add(HelpshiftSdk.HsFlowTitle, keyTitle.Localize());

                Dictionary<string, object> conversationConfig = new Dictionary<string, object>();
                // custom tags
                AttrList tagsList = currentTopic.GetFallback("tags", AttrEmpty.InvalidList);
                if(tagsList.Count > 0)
                {
                    var tagStrings = new String[tagsList.Count];
                    for(int t = 0; t < tagsList.Count; ++t)
                    {
                        tagStrings[t] = tagsList[t].AsValue.ToString();
                    }
                    Dictionary<string, object> customMeta = new Dictionary<string, object>();
                    customMeta.Add(HelpshiftSdk.HSTAGSKEY, tagStrings);
                    conversationConfig.Add(HelpshiftSdk.HSCUSTOMMETADATAKEY, customMeta);
                }

                //predefined text
                if(predefinedTextTid.Length > 0)
                {
                    conversationConfig.Add("conversationPrefillText", keyText.Localize());
                }

                conversationFlow.Add(HelpshiftSdk.HsFlowConfig, conversationConfig);

                currentflows.Add(conversationFlow);
            }

            Flows = currentflows.ToArray();
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

        HelpshiftCustomer UserData { get; set; }

        bool IsEnabled{ get; }

        void Enable();

        void ShowFAQ(string sectionId = null);

        void ShowConversation();

        void OpenFromPush(string issueId);

        int PendingNotificationsCount { get; }

        void AddFlows(AttrDic flows);
    }
}
