#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define HELPSHIFT_SUPPORTED
using System;
using System.Collections.Generic;
using Helpshift;
using SocialPoint.Base;
using SocialPoint.Attributes;
#endif

using SocialPoint.Dependency;
using SocialPoint.Hardware;

using SocialPoint.Locale;
using SocialPoint.Notifications;

#if HELPSHIFT_SUPPORTED

namespace SocialPoint.Helpshift
{
    public sealed class UnityHelpshift : IHelpshift
    {
        const string DisableErrorLoggingKey = "disableErrorLogging";
        const string EnableInAppNotificationsKey = "enableInAppNotification";
        const string EnableContactUsKey = "enableContactUs";
        const string ShowSarchOnNewConversationKey = "showSearchOnNewConversation";
        const string ShowConversationResolutionQuestionKey = "showConversationResolutionQuestion";
        const string YesKey = "yes";
        const string NoKey = "no";
       
        readonly IDeviceInfo _deviceInfo;
        HelpshiftConfiguration _config;

        public ILocalizationManager LocalizationManager { private get; set; }
        public INotificationServices NotificationServices { private get; set; }

        HelpshiftSdk _helpshift;
        Dictionary<string, object> _configMap;

        HelpshiftCustomer _userData;

        public UnityHelpshift(HelpshiftConfiguration config, IDeviceInfo deviceInfo)
        {
            _config = config;
            _deviceInfo = deviceInfo;
        }

        void Setup()
        {
            // Generate config
            CreateConfigMap();

            UpdateCustomerData();

            // Initialize Helpshift sdk
            _helpshift = HelpshiftSdk.getInstance();

#if UNITY_ANDROID
            // Install is only called from c# in Android.
            // For iOS, the config is deployed directly in a json file in the bundle to be read from native code
            _helpshift.install();
#endif
        }

        public void AddFlows(AttrDic flows)
        {
            Log.e("UnityHelpshift", "Not implemented!!");
        }

        void CreateConfigMap()
        {
            _configMap = new Dictionary<string, object>();

            // Controls the visibility of Contact Us button
            _configMap.Add(EnableContactUsKey, GetContactModeString(_config.Mode));

            // If showSearchOnNewConversation flag is set to yes, the user will be taken to a view which shows the 
            // search results relevant to the conversation text that he has entered upon clicking the ‘Send’ button. 
            // This is to avoid tickets which are already answered in the FAQs.
            _configMap.Add(ShowSarchOnNewConversationKey, _config.SearchOnNewConversationEnabled ? YesKey : NoKey);


            // By default the Helpshift SDK will not show the conversation resolution question to the user, to confirm 
            // if the conversation was resolved. On resolving the conversation from the admin dashboard will now take the 
            // user directly to the “Start a new conversation” state. If you want to enable the conversation resolution question, 
            // set showConversationResolutionQuestion to yes
            _configMap.Add(ShowConversationResolutionQuestionKey, _config.ConversationResolutionQuestionEnabled ? YesKey : NoKey);
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
            
            _userData.CustomMetaData.Add("ifda", _deviceInfo.AdvertisingId);

            foreach(var kpv in _userData.CustomMetaData)
            {
                customerMetaData.Add(kpv.Key, kpv.Value);
            }

            _configMap[HelpshiftSdk.HSCUSTOMMETADATAKEY] = customerMetaData;
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

            _helpshift.setSDKLanguage(LocalizationManager.SelectedLanguage);
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
            get
            {
                return _userData;
            }
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
            DebugUtils.Assert(LocalizationManager != null);
            DebugUtils.Assert(NotificationServices != null);

            if(!IsEnabled)
            {
                Setup();

                if(_helpshift == null)
                {
                    return;
                }

                // Listen push notification token
                NotificationServices.RegisterForRemoteToken(OnDeviceTokenReceived);

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

        public int PendingNotificationsCount
        {
            get
            {
                if(_helpshift == null)
                {
                    return 0;
                }

                return _helpshift.getNotificationCount(false);
            }
        }

        #endregion
    }
}

#else

namespace SocialPoint.Helpshift
{
    public sealed class UnityHelpshift : EmptyHelpshift
    {
        public UnityHelpshift(HelpshiftConfiguration config)
        {
        }

        public ILocalizationManager LocalizationManager { private get; set; }

        public INotificationServices NotificationServices { private get; set; }
    }
}

#endif
