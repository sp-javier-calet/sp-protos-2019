using UnityEngine;

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public class AndroidMemoryInfo : IMemoryInfo
    {
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
                try
                {
                    try
                    {
                        return (ulong)MemoryInfo.Get<long>("totalMem"); // API level 16
                    }
                    catch(AndroidJavaException)
                    {
                        int memorySizeInMegaBytes = SystemInfo.systemMemorySize;
                        return (ulong)(1024 * 1024 * memorySizeInMegaBytes);
                    }
                }
                catch(AndroidJavaException)
                {
                    return 0;
                }
            }
        }

        public ulong FreeMemory
        {
            get
            {
                try
                {
                    return (ulong)MemoryInfo.Get<long>("availMem"); // API level 1
                }
                catch(AndroidJavaException)
                {
                    return 0;
                }
            }
        }

        public ulong UsedMemory
        {
            get
            {
                return TotalMemory - FreeMemory;
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

