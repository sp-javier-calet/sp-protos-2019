using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;

namespace SocialPoint.VideoAds
{
    public class VideoAdsInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public string AppID;
            public string SecurityToken;
        }

        public SettingsData iOSSettings;
        public SettingsData AndroidSettings;

        public override void InstallBindings()
        {
            Container.BindUnityComponent<SocialPointVideoAdsManager>();
            Container.Bind<IVideoAdsManager>().ToMethod<SocialPointVideoAdsManager>(CreateVideoAdManager);
            Container.Bind<IDisposable>().ToLookup<IVideoAdsManager>();
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelVideoAds>(CreateAdminPanelVideoAds);
        }

        SocialPointVideoAdsManager CreateVideoAdManager()
        {
            var videoAdsManager = Container.Resolve<SocialPointVideoAdsManager>();

            SettingsData settings = null;
            #if UNITY_IOS
        settings = iOSSettings;
            #elif UNITY_ANDROID
            settings = AndroidSettings;
            #endif

            videoAdsManager.LoginData = Container.Resolve<ILoginData>();
            if(settings != null)
            {
                videoAdsManager.AppId = settings.AppID;
                videoAdsManager.SecurityToken = settings.SecurityToken;
            }

            return videoAdsManager;
        }

        AdminPanelVideoAds CreateAdminPanelVideoAds()
        {
            return new AdminPanelVideoAds(
                Container.Resolve<IVideoAdsManager>());
        }
    }
}