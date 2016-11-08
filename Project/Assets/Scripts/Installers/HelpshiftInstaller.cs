#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
#define HELPSHIFT_SUPPORTED
#endif

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

public class HelpshiftInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty;

        public string IosApiKey = HelpshiftConfiguration.DefaultIosApiKey;

        public string IosDomainName = HelpshiftConfiguration.DefaultIosDomainName;

        public string IosAppId = HelpshiftConfiguration.DefaultIosAppId;

        public string AndroidApiKey = HelpshiftConfiguration.DefaultAndroidApiKey;

        public string AndroidDomainName = HelpshiftConfiguration.DefaultAndroidDomainName;

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
        string apiKey = Settings.IosApiKey;
        string domainName = Settings.IosDomainName;
        string appId = Settings.IosAppId;

        #if UNITY_ANDROID
        apiKey = Settings.AndroidApiKey;
        domainName = Settings.AndroidDomainName;
        appId = Settings.AndroidAppId;
        #endif
           
        var hsconfig = new HelpshiftConfiguration(apiKey, appId, domainName) {
            Mode = Settings.Mode,
            InAppNotificationEnabled = Settings.InAppNotificationEnabled,
            SearchOnNewConversationEnabled = Settings.SearchOnNewConversationEnabled,
            ConversationResolutionQuestionEnabled = Settings.ConversationResolutionQuestionEnabled
        };

        var hs = new UnityHelpshift(hsconfig, Container.Resolve<ILocalizationManager>(), Container.Resolve<INotificationServices>());

        var login = Container.Resolve<ILogin>();
        login.NewGenericDataEvent -= OnNewGenericData;
        login.NewGenericDataEvent += OnNewGenericData;

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
