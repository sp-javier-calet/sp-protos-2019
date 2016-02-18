using UnityEngine;

namespace SocialPoint.Hardware
{
#if UNITY_ANDROID
    public class AndroidMemoryInfo : IMemoryInfo
    {
        public AndroidMemoryInfo()
        {
        }

        public static AndroidJavaObject MemoryInfo
        {
            get
            {
                var info = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                AndroidDeviceInfo.ActivityManager.Call("getMemoryInfo", info);
                return info;
            }
        }

        public ulong TotalMemory
        {
            get
            {
                return (ulong)MemoryInfo.Get<long>("totalMem");
            }
        }

        public ulong FreeMemory
        {
            get
            {
                return (ulong)MemoryInfo.Get<long>("availMem");
            }
        }

        public ulong UsedMemory
        {
            get
            {
                var info = MemoryInfo;
                return (ulong)(info.Get<long>("totalMem") - info.Get<long>("availMem"));
            }
        }

        public ulong ActiveMemory
        {
            get
            {
                return UsedMemory;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
#else
    public class AndroidMemoryInfo : EmptyMemoryInfo
    {
    }
#endif
}

