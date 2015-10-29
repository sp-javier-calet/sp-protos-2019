using Zenject;
using System;
using SocialPoint.Hardware;
using SocialPoint.AdminPanel;

public class HardwareInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsDataApp
    {
        public bool fakeAppData = false;
        public string SeedId;
        public string Id;
        public string Version;
        public string ShortVersion;
        public string Language;
        public string Country;
    }

    public SettingsDataApp SettingsAppInfo;

	public override void InstallBindings()
	{
        if(SettingsAppInfo.fakeAppData)
        {
            var appInfo = new EmptyAppInfo();
            appInfo.SeedId = SettingsAppInfo.SeedId;
            appInfo.Id = SettingsAppInfo.Id;
            appInfo.Version = SettingsAppInfo.Version;
            appInfo.ShortVersion = SettingsAppInfo.ShortVersion;
            appInfo.Language = SettingsAppInfo.Language;
            appInfo.Country = SettingsAppInfo.Country;
            Container.BindInstance("hardware_fake_app_info", appInfo);
        }

        Container.Rebind<IDeviceInfo>().ToSingle<DeviceInfo>();
        Container.Rebind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
        Container.Rebind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
        Container.Rebind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
        Container.Rebind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);


	}
}