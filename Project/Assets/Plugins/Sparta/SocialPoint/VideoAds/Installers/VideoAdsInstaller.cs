using System;
using SocialPoint.Dependency;
using SocialPoint.Login;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

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

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelVideoAds>(CreateAdminPanelVideoAds);
            #endif
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

        #if ADMIN_PANEL
        AdminPanelVideoAds CreateAdminPanelVideoAds()
        {
            return new AdminPanelVideoAds(
                Container.Resolve<IVideoAdsManager>());
        }
        #endif
    }
}
