using System;
using SocialPoint.Hardware;
using Zenject;

class DeviceInfo : SocialPointDeviceInfo
{
    [InjectOptional("hardware_fake_app_info")]
    EmptyAppInfo injectFakeAppInfo
    {
        set
        {
            #if UNITY_EDITOR
            AppInfo = value;
            #endif
        }
    }

    public DeviceInfo()
    {
    }
}
