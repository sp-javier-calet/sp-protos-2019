using System;
using SocialPoint.Dependency;
using SocialPoint.Marketing;

public class AppsFlyerInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool ActiveOnIOS;
        public string IOsAppsFlyerKey;
        public string IOsAppID;

        public bool ActiveOnAndroid;
        public string AndroidAppsFlyerKey;
        public string AndroidAppID;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        #if UNITY_ANDROID
        if(!Settings.ActiveOnAndroid) return;
        #elif UNITY_IOS
        if(!Settings.ActiveOnIOS) return;
        #endif

        #if UNITY_EDITOR
        Container.Bind<IMarketingTracker>().ToSingle<EmptyAppsFlyer>();
        Container.Bind<IDisposable>().ToSingle<EmptyAppsFlyer>();
        #else
        Container.Bind<IMarketingTracker>().ToMethod<SocialPointAppFlyer>(CreateMobileAppTracking);
        Container.Bind<IDisposable>().ToMethod<SocialPointAppFlyer>(CreateMobileAppTracking);
        #endif
    }

    SocialPointAppFlyer CreateMobileAppTracking()
    {
        var tracker = new SocialPointAppFlyer();
        #if UNITY_IOS
        tracker.AppsFlyerKey = Settings.IOsAppsFlyerKey;
        tracker.AppID = Settings.IOsAppID;
        #elif UNITY_ANDROID
        tracker.AppsFlyerKey = Settings.AndroidAppsFlyerKey;
        tracker.AppID = Settings.AndroidAppID;
        #endif
        tracker.Init();
        return tracker;
    }
}


