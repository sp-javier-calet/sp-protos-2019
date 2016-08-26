
namespace SocialPoint.Hardware
{
    public sealed class IosMemoryInfo : IMemoryInfo
    {
        public IosMemoryInfo ()
        {
        }

        public ulong TotalMemory
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetTotalMemory();
            }
        }

        public ulong FreeMemory
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetFreeMemory();
            }
        }

        public ulong UsedMemory
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetUsedMemory();
            }
        }

        public ulong ActiveMemory
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetActiveMemory();
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

