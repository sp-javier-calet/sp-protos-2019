#if UNITY_IOS && !UNITY_EDITOR
#define IOS_DEVICE
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
#define ANDROID_DEVICE
#endif

using System;
using SocialPoint.Dependency;

namespace  SocialPoint.Marketing
{
    public class AppsFlyerInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool ActiveOnIOS;
            public string IosAppsFlyerKey;
            public string IosAppID;

            public bool ActiveOnAndroid;
            public string AndroidAppsFlyerKey;
            public string AndroidAppID;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            #if ANDROID_DEVICE
            if(!Settings.ActiveOnAndroid) return;
            #elif IOS_DEVICE
            if(!Settings.ActiveOnIOS) return;
            #endif

            #if ANDROID_DEVICE || IOS_DEVICE
            Container.Rebind<SocialPointAppsFlyer>().ToMethod<SocialPointAppsFlyer>(CreateAppsFlyer);
            Container.Bind<IMarketingTracker>().ToLookup<SocialPointAppsFlyer>();
            Container.Bind<IDisposable>().ToLookup<SocialPointAppsFlyer>();
            #else
            Container.Rebind<EmptyAppsFlyer>().ToSingle<EmptyAppsFlyer>();
            Container.Bind<IMarketingTracker>().ToLookup<EmptyAppsFlyer>();
            Container.Bind<IDisposable>().ToLookup<EmptyAppsFlyer>();
            #endif
        }

        SocialPointAppsFlyer CreateAppsFlyer()
        {
            var tracker = new SocialPointAppsFlyer();
            #if IOS_DEVICE
            tracker.AppsFlyerKey = Settings.IosAppsFlyerKey;
            tracker.AppID = Settings.IosAppID;
            #elif ANDROID_DEVICE
            tracker.AppsFlyerKey = Settings.AndroidAppsFlyerKey;
            tracker.AppID = Settings.AndroidAppID;
            #endif
            tracker.Init();
            return tracker;
        }
    }
}