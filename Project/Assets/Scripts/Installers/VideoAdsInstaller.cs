using SocialPoint.AdminPanel;
using SocialPoint.VideoAds;
using System;
using Zenject;

public class VideoAdsInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string AppID;
        public string UserID;
    }

    public SettingsData iOSSettings;
    public SettingsData AndroidSettings;

    public override void InstallBindings()
    {
        SettingsData settings;
        #if UNITY_IOS
        settings = iOSSettings;
        #elif UNITY_ANDROID
        settings = AndroidSettings;
        #endif

        Container.BindInstance("videoads_appid", settings.AppID);
        Container.BindInstance("videoads_userid", settings.UserID);
        Container.Bind<VideoAds>().ToSingleGameObject();
        Container.Bind<IVideoAdsManager>().ToLookup<VideoAds>();
        Container.Bind<IDisposable>().ToLookup<IVideoAdsManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelVideoAds>();
    }
    
}


