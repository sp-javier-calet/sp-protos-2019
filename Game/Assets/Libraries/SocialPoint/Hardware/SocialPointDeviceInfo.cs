
namespace SocialPoint.Hardware
{
    
#if UNITY_IOS && !UNITY_EDITOR
    using BaseDeviceInfo = IosDeviceInfo;
#elif UNITY_ANDROID && !UNITY_EDITOR
    using BaseDeviceInfo = AndroidDeviceInfo;
#else
    using BaseDeviceInfo = UnityDeviceInfo;
#endif

    public class SocialPointDeviceInfo : BaseDeviceInfo
    {
    }
}

