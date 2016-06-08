using System;
using SocialPoint.Dependency;
using SocialPoint.Marketing;

public class MobileAppTrackingInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool ActiveOnIOS;
        public string AdvertiserID;
        public string ConversionKey;
        public bool ActiveOnAndroid;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        #if UNITY_ANDROID
        if(!Settings.ActiveOnAndroid) return;
        #elif (UNITY_IOS || UNITY_TVOS)
        if(!Settings.ActiveOnIOS) return;
        #endif
        Container.Bind<IMarketingTracker>().ToMethod<SocialPointMobileAppTracking>(CreateMobileAppTracking);
        Container.Bind<IDisposable>().ToMethod<SocialPointMobileAppTracking>(CreateMobileAppTracking);
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


