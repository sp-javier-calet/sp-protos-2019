using Zenject;
using SocialPoint.Hardware;
using System;

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

        Container.Bind<IDeviceInfo>().ToSingle<DeviceInfo>();
        Container.Bind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
        Container.Bind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
        Container.Bind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
        Container.Bind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);
	}
}