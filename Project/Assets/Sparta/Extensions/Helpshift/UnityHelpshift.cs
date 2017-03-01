#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define HELPSHIFT_SUPPORTED
using System;
using System.Collections.Generic;
using Helpshift;
using SocialPoint.Base;
using UnityEngine;
#endif

using SocialPoint.Locale;
using SocialPoint.Notifications;

#if HELPSHIFT_SUPPORTED

namespace SocialPoint.Extension.Helpshift
{
    public sealed class UnityHelpshift : IHelpshift
    {
        const string ENABLE_IN_APP_NOTIFICATIONS_KEY = "enableInAppNotification";
        const string UNITY_GAMEOBJECT_NAME_KEY = "unityGameObject";
        const string ENABLE_CONTACT_US_KEY = "enableContactUs";
        const string SHOW_SEARCH_ON_NEW_CONVERSATION_KEY = "showSearchOnNewConversation";
        const string SHOW_CONVERSATION_RESOLUTION_QUESTION_KEY = "showConversationResolutionQuestion";
        const string YES_KEY = "yes";
        const string NO_KEY = "no";

        const string GameObjectName = "SocialPointHelpshift";

        GameObject _gameObject;

        HelpshiftConfiguration _config;
        ILocalizationManager _localizationManager;
        INotificationServices _notificationServices;

        HelpshiftSdk _helpshift;
        Dictionary<string, object> _configMap;

        HelpshiftCustomer _userData;

        public UnityHelpshift(HelpshiftConfiguration config, ILocalizationManager localizationManager, INotificationServices notificationServices)
        {
            _config = config;
            _localizationManager = localizationManager;
            _notificationServices = notificationServices;

            DebugUtils.Assert(_localizationManager != null);
            DebugUtils.Assert(_notificationServices != null);
        }

        void Setup()
        {
            if(string.IsNullOrEmpty(_config.ApiKey) ||
               string.IsNullOrEmpty(_config.DomainName) ||
               string.IsNullOrEmpty(_config.AppId))
            {
                throw new ArgumentException("Invalid Helshift configuration");
            }

            // Generate config
            CreateConfigMap();

            UpdateCustomerData();

            // Initialize Helpshift sdk
            _helpshift = HelpshiftSdk.getInstance();
            _helpshift.install(_config.ApiKey, _config.DomainName, _config.AppId, _configMap);
        }

        void CreateConfigMap()
        {
            _configMap = new Dictionary<string, object>();

            _configMap.Add(UNITY_GAMEOBJECT_NAME_KEY, GameObjectName);

            // Controls the visibility of Contact Us button
            _configMap.Add(ENABLE_CONTACT_US_KEY, GetContactModeString(_config.Mode));

            // Use in-app notification support provided by the Helpshift 
            _configMap.Add(ENABLE_IN_APP_NOTIFICATIONS_KEY, _config.InAppNotificationEnabled ? YES_KEY : NO_KEY);

            // If showSearchOnNewConversation flag is set to yes, the user will be taken to a view which shows the 
            // search results relevant to the conversation text that he has entered upon clicking the ‘Send’ button. 
            // This is to avoid tickets which are already answered in the FAQs.
            _configMap.Add(SHOW_SEARCH_ON_NEW_CONVERSATION_KEY, _config.SearchOnNewConversationEnabled ? YES_KEY : NO_KEY);


            // By default the Helpshift SDK will not show the conversation resolution question to the user, to confirm 
            // if the conversation was resolved. On resolving the conversation from the admin dashboard will now take the 
            // user directly to the “Start a new conversation” state. If you want to enable the conversation resolution question, 
            // set showConversationResolutionQuestion to yes
            _configMap.Add(SHOW_CONVERSATION_RESOLUTION_QUESTION_KEY, _config.ConversationResolutionQuestionEnabled ? YES_KEY : NO_KEY);
        }

        void UpdateCustomerData()
        {
            if(_helpshift == null || _userData == null)
            {
                return;
            }

            // Configure user data
            if(!string.IsNullOrEmpty(_userData.UserId))
            {
                _helpshift.setUserIdentifier(_userData.UserId);
            }

            // Customer Meta-data
            var customerMetaData = new Dictionary<string, object>();

            if(_userData.CustomerTags != null && _userData.CustomerTags.Length > 0)
            {
                customerMetaData.Add(HelpshiftSdk.HSTAGSKEY, _userData.CustomerTags);
            }

            foreach(var kpv in _userData.CustomMetaData)
            {
                customerMetaData.Add(kpv.Key, kpv.Value);
            }

            _configMap.Add(HelpshiftSdk.HSCUSTOMMETADATAKEY, customerMetaData);
        }

        static string GetContactModeString(HelpshiftConfiguration.ContactMode mode)
        {
            switch(mode)
            {
            case HelpshiftConfiguration.ContactMode.CONTACT_US_AFTER_VIEWING_FAQS:
                return HelpshiftSdk.CONTACT_US_AFTER_VIEWING_FAQS;
            case HelpshiftConfiguration.ContactMode.CONTACT_US_NEVER:
                return HelpshiftSdk.CONTACT_US_NEVER;
            default:
                return HelpshiftSdk.CONTACT_US_ALWAYS;
            }
        }

        void UpdateLanguage()
        {
            if(_helpshift == null)
            {
                return;
            }

            _helpshift.setSDKLanguage(_localizationManager.SelectedLanguage);
        }

        void OnDeviceTokenReceived(bool validToken, string deviceToken)
        {
            if(_helpshift == null)
            {
                return;
            }

            #if UNITY_ANDROID
            _helpshift.registerDelegates();
            #endif

            if(validToken)
            {
                _helpshift.registerDeviceToken(deviceToken);
            }
        }

        #region IHelpshift implementation

        public HelpshiftConfiguration Configuration
        {
            get
            {
                return _config;
            }
        }

        public HelpshiftCustomer UserData
        {
            set
            {
                _userData = value;
                UpdateCustomerData();
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _helpshift != null;
            }
        }

        public void Enable()
        {
            if(_gameObject == null)
            {
                _gameObject = new GameObject(GameObjectName);
                UnityEngine.Object.DontDestroyOnLoad(_gameObject);

                Setup();

                if(_helpshift == null)
                {
                    return;
                }

                // Listen push notification token
                _notificationServices.RegisterForRemoteToken(OnDeviceTokenReceived);

                UpdateLanguage();
            }
            else
            {
                throw new InvalidOperationException("Already initialized Helpshift");
            }
        }

        public void ShowFAQ(string sectionId = null)
        {
            if(_helpshift == null)
            {
                return;
            }
 
            UpdateLanguage();

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
            if(_helpshift == null)
            {
                return;
            }

            UpdateLanguage();

            _helpshift.showConversation(_configMap);
        }

        public void OpenFromPush(string issueId)
        {
            if(_helpshift == null)
            {
                return;
            }

            _helpshift.handlePushNotification(issueId);
        }

        #endregion
    }
}

#else

namespace SocialPoint.Extension.Helpshift
{
    public sealed class UnityHelpshift : EmptyHelpshift
    {
        public UnityHelpshift(HelpshiftConfiguration config, ILocalizationManager localizationManager, INotificationServices notificationServices)
        {

        }
    }
}

#endif
