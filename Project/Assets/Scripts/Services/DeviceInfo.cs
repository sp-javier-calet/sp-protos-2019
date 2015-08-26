using System;
using SocialPoint.Hardware;
using Zenject;

class DeviceInfo : SocialPointDeviceInfo
{
    [InjectOptional("hardware_fake_app_info")]
    public EmptyAppInfo InjectAppInfo
    {
        set
        {
            AppInfo = value;
        }
    }

    public DeviceInfo()
    {
    }
}
