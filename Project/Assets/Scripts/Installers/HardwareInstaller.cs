
using System;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.AdminPanel;

public class HardwareInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool FakeAppData = false;
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
        #if UNITY_EDITOR
        if(Settings.FakeAppData)
        {
            var appInfo = new EmptyAppInfo();
            appInfo.SeedId = Settings.AppSeedId;
            appInfo.Id = Settings.AppId;
            appInfo.Version = Settings.AppVersion;
            appInfo.ShortVersion = Settings.AppShortVersion;
            appInfo.Language = Settings.AppLanguage;
            appInfo.Country = Settings.AppCountry;
            info.AppInfo = appInfo;
        }
        #endif
    }

    AdminPanelHardware CreateAdminPanel()
    {
        return new AdminPanelHardware(
            Container.Resolve<IDeviceInfo>());
    }
}