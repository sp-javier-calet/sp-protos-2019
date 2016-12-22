using System;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.AdminPanel;

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
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<IDeviceInfo>().ToMethod<SocialPointDeviceInfo>(CreateDeviceInfo, SetupDeviceInfo);
            Container.Rebind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
            Container.Rebind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
            Container.Rebind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
            Container.Rebind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHardware>(CreateAdminPanel);
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

        AdminPanelHardware CreateAdminPanel()
        {
            return new AdminPanelHardware(
                Container.Resolve<IDeviceInfo>());
        }
    }
}