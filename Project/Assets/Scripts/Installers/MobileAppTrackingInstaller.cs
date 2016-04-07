using System;
using Zenject;
using SocialPoint.Marketing;

public class MobileAppTrackingInstaller : MonoInstaller
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
        #elif UNITY_IOS
        if(!Settings.ActiveOnIOS) return;
        #endif
        Container.Bind<IMarketingTracker>().ToSingleMethod<SocialPointMobileAppTracking>(CreateMobileAppTracking);
        Container.Bind<IDisposable>().ToSingleMethod<SocialPointMobileAppTracking>(CreateMobileAppTracking);
    }

    SocialPointMobileAppTracking CreateMobileAppTracking(InjectContext ctx)
    {
        var tracker = new SocialPointMobileAppTracking();
        tracker.AdvertiserID = Settings.AdvertiserID;
        tracker.ConversionKey = Settings.ConversionKey;
        tracker.Init();
        return tracker;
    }
}


