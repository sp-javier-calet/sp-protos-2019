
using System;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.AdminPanel;

public class HardwareInstaller : MonoInstaller
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
        if(Settings.FakeAppData)
        {
            var appInfo = new EmptyAppInfo();
            appInfo.SeedId = Settings.AppSeedId;
            appInfo.Id = Settings.AppId;
            appInfo.Version = Settings.AppVersion;
            appInfo.ShortVersion = Settings.AppShortVersion;
            appInfo.Language = Settings.AppLanguage;
            appInfo.Country = Settings.AppCountry;
            Container.BindInstance("hardware_fake_app_info", appInfo);
        }

        Container.Rebind<IDeviceInfo>().ToSingleMethod<SocialPointDeviceInfo>(CreateDeviceInfo);
        Container.Rebind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
        Container.Rebind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
        Container.Rebind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
        Container.Rebind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);

        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelHardware>(CreateAdminPanel);
	}

    SocialPointDeviceInfo CreateDeviceInfo()
    {
        var info = new SocialPointDeviceInfo();
        #if UNITY_EDITOR
        info.AppInfo = Container.Resolve<IAppInfo>("hardware_fake_app_info");
        #endif
        return info;
    }

    AdminPanelHardware CreateAdminPanel()
    {
        return new AdminPanelHardware(
            Container.Resolve<IDeviceInfo>());
    }
}