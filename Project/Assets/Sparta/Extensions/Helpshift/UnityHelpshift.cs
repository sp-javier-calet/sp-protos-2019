using UnityEngine;
using System;
using System.Collections.Generic;
using Helpshift;

namespace SocialPoint.Extension.Helpshift
{
    class UnityHelpshift : MonoBehaviour, IHelpshift
    {
        const string ENABLE_IN_APP_NOTIFICATIONS_KEY = "enableInAppNotification";
        const string UNITY_GAMEOBJECT_NAME_KEY = "unityGameObject";
        const string ENABLE_CONTACT_US_KEY = "enableContactUs";
        const string SHOW_SEARCH_ON_NEW_CONVERSATION_KEY = "showSearchOnNewConversation";
        const string SHOW_CONVERSATION_RESOLUTION_QUESTION_KEY = "showConversationResolutionQuestion";
        const string YES_KEY = "yes";
        const string NO_KEY = "no";


        HelpshiftSdk _helpshift;
        Dictionary<string, object> _configMap;

        public void Setup(string gameObjectName, HelpshiftConfiguration config, HelpshiftCustomer customerData = null)
        {
            if(string.IsNullOrEmpty(config.ApiKey) ||
               string.IsNullOrEmpty(config.AppId) ||
               string.IsNullOrEmpty(config.DomainName))
            {
                throw new ArgumentException("Invalid Helshift configuration", "config");
            }

            // Initialize Helpshift sdk
            _helpshift = HelpshiftSdk.getInstance();
            _helpshift.install(config.ApiKey, config.DomainName, config.AppId);

            // Generate config
            _configMap = CreateConfigMap(gameObjectName, config);

            if(customerData != null)
            {
                SetCustomerData(customerData);
            }
        }

        Dictionary<string, object> CreateConfigMap(string gameObjectName, HelpshiftConfiguration config)
        {
            var dic = new Dictionary<string, object>();

            dic.Add(UNITY_GAMEOBJECT_NAME_KEY, gameObjectName);

            // Controls the visibility of Contact Us button
            dic.Add(ENABLE_CONTACT_US_KEY, GetContactModeString(config.Mode));

            // Use in-app notification support provided by the Helpshift 
            dic.Add(ENABLE_IN_APP_NOTIFICATIONS_KEY, config.InAppNotificationEnabled ? YES_KEY : NO_KEY);

            // If showSearchOnNewConversation flag is set to yes, the user will be taken to a view which shows the 
            // search results relevant to the conversation text that he has entered upon clicking the ‘Send’ button. 
            // This is to avoid tickets which are already answered in the FAQs.
            dic.Add(SHOW_SEARCH_ON_NEW_CONVERSATION_KEY, config.SearchOnNewConversationEnabled ? YES_KEY : NO_KEY);


            // By default the Helpshift SDK will not show the conversation resolution question to the user, to confirm 
            // if the conversation was resolved. On resolving the conversation from the admin dashboard will now take the 
            // user directly to the “Start a new conversation” state. If you want to enable the conversation resolution question, 
            // set showConversationResolutionQuestion to yes
            dic.Add(SHOW_CONVERSATION_RESOLUTION_QUESTION_KEY, config.ConversationResolutionQuestionEnabled ? YES_KEY : NO_KEY);

            return dic;
        }

        void UpdateCustomerData(Dictionary<string, object> configMap, HelpshiftCustomer customerData)
        {
            // Customer Meta-data
            var customerMetaData = new Dictionary<string, object>();

            if(customerData.CustomerTags != null && customerData.CustomerTags.Length > 0)
            {
                customerMetaData.Add(HelpshiftSdk.HSTAGSKEY, customerData.CustomerTags);
            }

            foreach(var kpv in customerData.CustomMetaData)
            {
                customerMetaData.Add(kpv.Key, kpv.Value);
            }

            configMap.Add(HelpshiftSdk.HSCUSTOMMETADATAKEY, customerMetaData);
        }

        string GetContactModeString(HelpshiftConfiguration.ContactMode mode)
        {
            switch(mode)
            {
            case HelpshiftConfiguration.ContactMode.ALWAYS:
                return HelpshiftSdk.CONTACT_US_ALWAYS;
            
            case HelpshiftConfiguration.ContactMode.NEVER:
                return HelpshiftSdk.CONTACT_US_NEVER;
            
//            case HelpshiftConfiguration.ContactMode.AFTER_UNHELPFUL_ANSWER:
////                return HelpshiftSdk.CONTACT_US_AFTER_MARKING_ANSWER_UNHELPFUL;
//            
//            case HelpshiftConfiguration.ContactMode.AFTER_VIEWING_FAQS:            
            default:
                return HelpshiftSdk.CONTACT_US_AFTER_VIEWING_FAQS;
            }
        }

        #region IHelpshift implementation

        public void SetCustomerData(HelpshiftCustomer customerData)
        {
            // Configure user data
            if(!string.IsNullOrEmpty(customerData.UserId))
            {
                _helpshift.setUserIdentifier(customerData.UserId);
            }

            // Update config map with customer data
            UpdateCustomerData(_configMap, customerData);
        }

        public void SetLanguage(string locale)
        {
            _helpshift.setSDKLanguage(locale);
        }

        public void ShowFAQ(string sectionId = null)
        {
            if(!string.IsNullOrEmpty(sectionId))
            {
                _helpshift.showSingleFAQ(sectionId, _configMap);
            }
            else
            {
                _helpshift.showFAQs(_configMap);
            }
        }

        public void ShowConversation()
        {
            _helpshift.showConversation(_configMap);
        }

        public void HandlePushNotification(string issueId)
        {
            _helpshift.handlePushNotification(issueId);
        }

        public void RegisterPushNotificationToken(string token)
        {
            _helpshift.registerDeviceToken(token);
        }

        #endregion
    }
}
