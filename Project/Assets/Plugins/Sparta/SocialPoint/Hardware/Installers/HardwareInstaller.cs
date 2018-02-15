using System;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using SocialPoint.Login;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Hardware
{
    public class HardwareInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public string AppSeedId;
            public string AppId;
            public string AppVersion;
            public string AppShortVersion;
            public string AppLanguage;
            public string AppCountry;

            public StorageAnalyzerConfig StorageAnalyzer;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<IDeviceInfo>().ToMethod<SocialPointDeviceInfo>(CreateDeviceInfo, SetupDeviceInfo);
            Container.Rebind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
            Container.Rebind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
            Container.Rebind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
            Container.Rebind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);
            Container.Rebind<IStorageAnalyzer>().ToMethod<StorageAnalyzer>(CreateStorageAnalyzer);

            #if UNITY_ANDROID && !UNITY_EDITOR
            Container.Rebind<INativeUtils>().ToMethod<AndroidNativeUtils>(CreateAndroidNativeUtils);
            #elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            Container.Rebind<INativeUtils>().ToMethod<IosNativeUtils>(CreateIosNativeUtils);
            #elif UNITY_EDITOR
            Container.Rebind<INativeUtils>().ToMethod<UnityNativeUtils>(CreateUnityNativeUtils);
            #else
            Container.Rebind<INativeUtils>().ToMethod<EmptyNativeUtils>(CreateEmptyNativeUtils);
            #endif

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHardware>(CreateAdminPanel);
            #endif
        }

        IosNativeUtils CreateIosNativeUtils()
        {
            return new IosNativeUtils(
                Container.Resolve<IAppInfo>()
            );
        }

        AndroidNativeUtils CreateAndroidNativeUtils()
        {
            return new AndroidNativeUtils(
                Container.Resolve<IAppInfo>()
            );
        }

        UnityNativeUtils CreateUnityNativeUtils()
        {
            return new UnityNativeUtils(
                Container.Resolve<IAppInfo>()
            );
        }

        EmptyNativeUtils CreateEmptyNativeUtils()
        {
            return new EmptyNativeUtils();
        }

        StorageAnalyzer CreateStorageAnalyzer()
        {
            return new StorageAnalyzer(
                Container.Resolve<IStorageInfo>(),
                Container.Resolve<IUpdateScheduler>(),
                Settings.StorageAnalyzer
            );
        }

        SocialPointDeviceInfo CreateDeviceInfo()
        {
            return new SocialPointDeviceInfo();
        }

        void SetupDeviceInfo(SocialPointDeviceInfo info)
        {
            if(info.AppInfo is EmptyAppInfo)
            {
                var appInfo = info.AppInfo as EmptyAppInfo;
                if(!string.IsNullOrEmpty(Settings.AppSeedId))
                {
                    appInfo.SeedId = Settings.AppSeedId;
                }
                if(!string.IsNullOrEmpty(Settings.AppId))
                {
                    appInfo.Id = Settings.AppId;
                }
                if(!string.IsNullOrEmpty(Settings.AppVersion))
                {
                    appInfo.Version = Settings.AppVersion;
                }
                if(!string.IsNullOrEmpty(Settings.AppShortVersion))
                {
                    appInfo.ShortVersion = Settings.AppShortVersion;
                }
                if(!string.IsNullOrEmpty(Settings.AppLanguage))
                {
                    appInfo.Language = Settings.AppLanguage;
                }
                if(!string.IsNullOrEmpty(Settings.AppCountry))
                {
                    appInfo.Country = Settings.AppCountry;
                }
            }
            if(info.AppInfo is UnityAppInfo)
            {
                var appInfo = info.AppInfo as UnityAppInfo;
                if(!string.IsNullOrEmpty(Settings.AppShortVersion))
                {
                    appInfo.ShortVersion = Settings.AppShortVersion;
                }
            }
        }

        #if ADMIN_PANEL
        AdminPanelHardware CreateAdminPanel()
        {
            return new AdminPanelHardware(
                Container.Resolve<IDeviceInfo>(),
                Container.Resolve<IStorageAnalyzer>());
        }
        #endif
    }
}
