﻿using System;
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
        // default params from DL
        public const string DefaultApiKey = "e80e75c9fd498d3274f3cdbc637b3866";
        public const string DefaultDomainName = "socialpoint.helpshift.com";
        public const string DefaultIosAppId = "socialpoint_platform_20151021095745155-1b0b401a75542a3";
        public const string DefaultAndroidAppId = "socialpoint_platform_20151021095745169-3fd755eb172b848";


        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;

            public string ApiKey = DefaultApiKey;

            public string DomainName = DefaultDomainName;

            public string IosAppId = DefaultIosAppId;

            public string AndroidAppId = DefaultAndroidAppId;

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
                Container.Rebind<IHelpshift>().ToMethod<UnityHelpshift>(CreateUnityHelpshift, SetupUnityHelpshift);
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

            var hs = new UnityHelpshift(hsconfig);
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
