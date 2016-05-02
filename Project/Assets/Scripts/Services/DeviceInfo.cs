using System;
using SocialPoint.Hardware;
using SocialPoint.Dependency;

class DeviceInfo : SocialPointDeviceInfo
{
    public DeviceInfo()
    {
        #if UNITY_EDITOR
        AppInfo = ServiceLocator.Instance.OptResolve<IAppInfo>("hardware_fake_app_info");
        #endif
    }
}
