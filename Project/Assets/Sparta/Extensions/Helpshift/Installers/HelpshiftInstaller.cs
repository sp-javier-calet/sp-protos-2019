using System;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Extension.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Notifications;

namespace SocialPoint.Extension.Helpshift
{
    public class HelpshiftInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;

            public string ApiKey = HelpshiftConfiguration.DefaultApiKey;

            public string DomainName = HelpshiftConfiguration.DefaultDomainName;

            public string IosAppId = HelpshiftConfiguration.DefaultIosAppId;

            public string AndroidAppId = HelpshiftConfiguration.DefaultAndroidAppId;

            public HelpshiftConfiguration.ContactMode Mode;

            public bool InAppNotificationEnabled;

            public bool SearchOnNewConversationEnabled;

            public bool ConversationResolutionQuestionEnabled;
        }

        public SettingsData Settings = new SettingsData();

        IHelpshift _helpshift;

        public override void InstallBindings()
        {
            if(Settings.UseEmpty)
            {
                Container.Rebind<IHelpshift>().ToSingle<EmptyHelpshift>();
            }
            else
            {
                Container.Rebind<IHelpshift>().ToMethod<UnityHelpshift>(CreateUnityHelpshift);
            }

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHelpshift>(CreateAdminPanel);
        }

        AdminPanelHelpshift CreateAdminPanel()
        {
            return new AdminPanelHelpshift(Container.Resolve<IHelpshift>());
        }

        UnityHelpshift CreateUnityHelpshift()
        {
            string appId = Settings.IosAppId;
            #if UNITY_ANDROID
            appId = Settings.AndroidAppId;
            #endif
           
            var hsconfig = new HelpshiftConfiguration(Settings.ApiKey, appId, Settings.DomainName) {
                Mode = Settings.Mode,
                InAppNotificationEnabled = Settings.InAppNotificationEnabled,
                SearchOnNewConversationEnabled = Settings.SearchOnNewConversationEnabled,
                ConversationResolutionQuestionEnabled = Settings.ConversationResolutionQuestionEnabled
            };

            var hs = new UnityHelpshift(hsconfig, Container.Resolve<ILocalizationManager>(), Container.Resolve<INotificationServices>());

            var login = Container.Resolve<ILogin>();
            if(login != null)
            {
                login.NewGenericDataEvent -= OnNewGenericData;
                login.NewGenericDataEvent += OnNewGenericData;
            }

            hs.Enable();

            _helpshift = hs;
            return hs;
        }

        void OnNewGenericData(Attr data)
        {
            var login = Container.Resolve<ILogin>();
            var userImportance = login.Data.UserImportance ?? string.Empty;

            string userId = login.UserId.ToString();
            DebugUtils.Assert(!string.IsNullOrEmpty(userId)); 
            if(_helpshift != null && !string.IsNullOrEmpty(userId))
            {
                _helpshift.UserData = new HelpshiftCustomer(userId, new []{ userImportance }, new Dictionary<string, object>());
            }

        }
    }
}
