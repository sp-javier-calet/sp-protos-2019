using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Notifications;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Helpshift
{
    public class HelpshiftInstaller : ServiceInstaller
    {
        // default params from DL
        public const string DefaultApiKey = "e80e75c9fd498d3274f3cdbc637b3866";
        public const string DefaultDomainName = "socialpoint.helpshift.com";
        public const string DefaultIosAppId = "socialpoint_platform_20151021095745155-1b0b401a75542a3";
        public const string DefaultAndroidAppId = "socialpoint_platform_20151021095745169-3fd755eb172b848";


        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;

            public HelpshiftConfiguration.ContactMode Mode = HelpshiftConfiguration.ContactMode.CONTACT_US_ALWAYS;

            public bool SearchOnNewConversationEnabled;

            public bool ConversationResolutionQuestionEnabled;
        }

        [Serializable]
        public class InstallData
        {
            public string ApiKey = DefaultApiKey;

            public string DomainName = DefaultDomainName;

            public string IosAppId = DefaultIosAppId;

            public string AndroidAppId = DefaultAndroidAppId;
        }

        public SettingsData Settings = new SettingsData();
        public InstallData InstallSettings = new InstallData();

        IHelpshift _helpshift;

        public override void InstallBindings()
        {
            if(Settings.UseEmpty)
            {
                Container.Rebind<IHelpshift>().ToSingle<EmptyHelpshift>();
            }
            else
            {
                Container.Rebind<IHelpshift>().ToMethod<UnityHelpshift>(CreateUnityHelpshift, SetupUnityHelpshift);
            }

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHelpshift>(CreateAdminPanel);
            #endif
        }

        #if ADMIN_PANEL
        AdminPanelHelpshift CreateAdminPanel()
        {
            return new AdminPanelHelpshift(Container.Resolve<IHelpshift>());
        }
        #endif

        UnityHelpshift CreateUnityHelpshift()
        {
            var hsconfig = new HelpshiftConfiguration()
            {
                Mode = Settings.Mode,
                SearchOnNewConversationEnabled = Settings.SearchOnNewConversationEnabled,
                ConversationResolutionQuestionEnabled = Settings.ConversationResolutionQuestionEnabled,
                Flows = null,
            };

            var hs = new UnityHelpshift(hsconfig, Container.Resolve<IDeviceInfo>());
            _helpshift = hs;
            return hs;
        }

        void SetupUnityHelpshift(UnityHelpshift helpshift)
        {
            helpshift.LocalizationManager = Container.Resolve<ILocalizationManager>();
            helpshift.NotificationServices = Container.Resolve<INotificationServices>();

            var login = Container.Resolve<ILogin>();
            if(login != null)
            {
                login.NewGenericDataEvent -= OnNewGenericData;
                login.NewGenericDataEvent += OnNewGenericData;
            }

            helpshift.Enable();
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
