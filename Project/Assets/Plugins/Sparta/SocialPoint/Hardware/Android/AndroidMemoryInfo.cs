#if UNITY_ANDROID
using SocialPoint.Base;
using UnityEngine;
#endif

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public sealed class AndroidMemoryInfo : IMemoryInfo
    {
        public static AndroidJavaObject MemoryInfo
        {
            get
            {
                var info = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                using(var activityManager = AndroidDeviceInfo.ActivityManager)
                {
                    activityManager.Call("getMemoryInfo", info);
                    return info;
                }
            }
        }

        public ulong TotalMemory
        {
            get
            {
                if(AndroidContext.SDKVersion >= 16)
                {
                    try
                    {
                        using(var memoryInfoObject = MemoryInfo)
                        {
                            return (ulong)memoryInfoObject.Get<long>("totalMem"); // API level 16
                        }
                    }
                    catch(AndroidJavaException)
                    {
                        return 0;
                    }
                }
                try
                {
                    int memorySizeInMegaBytes = SystemInfo.systemMemorySize;
                    return (ulong)(1024 * 1024 * memorySizeInMegaBytes);
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
                    using(var memoryInfoObject = MemoryInfo)
                    {
                        return (ulong)memoryInfoObject.Get<long>("availMem"); // API level 1
                    }
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
    public sealed class AndroidMemoryInfo : EmptyMemoryInfo
    {
    }
#endif
}

