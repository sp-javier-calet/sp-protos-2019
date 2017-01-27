using System;
using SocialPoint.Dependency;

namespace SocialPoint.Marketing
{
    public class MobileAppTrackingInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool ActiveOnIOS;
            public bool ActiveOnAndroid;
            public string AdvertiserID;
            public string ConversionKey;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            #if UNITY_ANDROID
            if(!Settings.ActiveOnAndroid)
                return;
            #elif UNITY_IOS
        if(!Settings.ActiveOnIOS) return;
            #endif

            Container.Rebind<SocialPointMobileAppTracking>().ToMethod<SocialPointMobileAppTracking>(CreateMobileAppTracking);
            Container.Bind<IMarketingTracker>().ToLookup<SocialPointMobileAppTracking>();
            Container.Bind<IDisposable>().ToLookup<SocialPointMobileAppTracking>();
        }

        SocialPointMobileAppTracking CreateMobileAppTracking()
        {
            var tracker = new SocialPointMobileAppTracking();
            tracker.AdvertiserID = Settings.AdvertiserID;
            tracker.ConversionKey = Settings.ConversionKey;
            tracker.Init();
            return tracker;
        }
    }
}