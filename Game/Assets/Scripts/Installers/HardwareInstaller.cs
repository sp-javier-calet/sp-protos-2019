using Zenject;
using SocialPoint.Hardware;
using System;

public class HardwareInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{

	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        var devInfo = new SocialPointDeviceInfo();
		Container.Bind<IDeviceInfo>().ToInstance(devInfo);
	}
}
