
namespace SocialPoint.Hardware
{
    public sealed class IosStorageInfo : IStorageInfo
    {
        public IosStorageInfo ()
        {
        }

        public ulong TotalStorage
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetTotalStorage();
            }
        }

        public ulong FreeStorage
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetFreeStorage();
            }
        }

        public ulong UsedStorage
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetUsedStorage();
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}