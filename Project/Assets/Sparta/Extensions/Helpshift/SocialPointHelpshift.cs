using UnityEngine;
using SocialPoint.Notifications;
using SocialPoint.Locale;

namespace SocialPoint.Extension.Helpshift
{
    #if UNITY_EDITOR
    using HelpshiftClass = EmptyHelpshift;

    #else
    using HelpshiftClass = UnityHelpshift;
    #endif

    public class SocialPointHelpshift
    {
        const string GameObjectName = "SocialPointHelpshift";

        GameObject _gameObject;
        IHelpshift _helpshift;

        HelpshiftConfiguration _config;
        ILocalizationManager _localizationManager;
        INotificationServices _notificationServices;


        public SocialPointHelpshift(HelpshiftConfiguration config, ILocalizationManager localizationManager, INotificationServices notificationServices)
        {
            _config = config;
            _localizationManager = localizationManager;
            _notificationServices = notificationServices;
        }

        public bool IsEnabled
        {
            get
            {
                return _helpshift != null;
            }
        }

        HelpshiftCustomer _customerData;

        public HelpshiftCustomer UserData
        {
            set
            {
                _customerData = value;
                if(IsEnabled)
                {
                    _helpshift.SetCustomerData(_customerData);
                }
            }
        }

        public void Enable()
        {
            if(_gameObject == null)
            {
                _gameObject = new GameObject(GameObjectName);
                GameObject.DontDestroyOnLoad(_gameObject);

                _helpshift = _gameObject.AddComponent<HelpshiftClass>();
                _helpshift.Setup(GameObjectName, _config);

                if(_customerData != null)
                {
                    _helpshift.SetCustomerData(_customerData);
                }

                // Listen push notification token and set selected language
                _notificationServices.RegisterForRemoteToken((isTokenValid, token) => {
                    if(isTokenValid)
                    {
                        _helpshift.RegisterPushNotificationToken(token);
                    }
                });

                UpdateLanguage();
            }
            else
            {
                throw new System.InvalidOperationException("Already initialized SocialPointHelpshift");
            }
        }

        void UpdateLanguage()
        {
            if(IsEnabled)
            {
                _helpshift.SetLanguage(_localizationManager.CurrentLanguage);
            }
        }

        public void ShowFAQ(string sectionId = null)
        {
            if(IsEnabled)
            {
                UpdateLanguage();
                _helpshift.ShowFAQ(sectionId);
            }
        }

        public void ShowConversation()
        {
            if(IsEnabled)
            {
                UpdateLanguage();
                _helpshift.ShowConversation();
            }
        }
    }
}