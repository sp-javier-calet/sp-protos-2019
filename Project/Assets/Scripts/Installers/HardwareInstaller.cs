using Zenject;
using SocialPoint.Hardware;
using System;

public class HardwareInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        Container.Bind<IDeviceInfo>().ToSingle<SocialPointDeviceInfo>();
        Container.Bind<IMemoryInfo>().ToGetter<IDeviceInfo>(x => x.MemoryInfo);
        Container.Bind<IStorageInfo>().ToGetter<IDeviceInfo>(x => x.StorageInfo);
        Container.Bind<IAppInfo>().ToGetter<IDeviceInfo>(x => x.AppInfo);
        Container.Bind<INetworkInfo>().ToGetter<IDeviceInfo>(x => x.NetworkInfo);
	}
}