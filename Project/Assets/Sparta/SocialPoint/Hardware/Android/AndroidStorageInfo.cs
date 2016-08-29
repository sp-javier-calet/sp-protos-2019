#if UNITY_ANDROID
using SocialPoint.Base;
using UnityEngine;
#endif

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public sealed class AndroidStorageInfo : IStorageInfo
    {
        static string _rootPath;

        public static string RootPath
        {
            get
            {
                if(_rootPath == null)
                {
                    using(var env = new AndroidJavaClass("android.os.Environment")) // API level 1
                    {
                        _rootPath = env.CallStatic<AndroidJavaObject>("getRootDirectory").Call<string>("getAbsolutePath"); // API level 1
                    }
                }
                return _rootPath;
            }
        }

        static string _dataPath;

        public static string DataPath
        {
            get
            {
                if(_dataPath == null)
                {
                    using(var env = new AndroidJavaClass("android.os.Environment")) // API level 1
                    {
                        _dataPath = env.CallStatic<AndroidJavaObject>("getDataDirectory").Call<string>("getAbsolutePath"); // API level 1
                    }
                }
                return _dataPath;
            }
        }

        public static AndroidJavaObject StatFs(string path)
        {
            return new AndroidJavaObject("android.os.StatFs", path); // API level 1
        }

        public ulong TotalStorage
        {
            get
            {
                if(AndroidContext.SDKVersion >= 18)
                {
                    try
                    {
                        using(var fs = StatFs(DataPath))
                        {
                            return (ulong)fs.Call<long>("getTotalBytes"); // API level 18
                        }
                    }
                    catch(AndroidJavaException)
                    {
                        return 0;
                    }
                }
                try
                {
                    using(var fs = StatFs(DataPath))
                    {
                        int blockCount = fs.Call<int>("getBlockCount"); // API level 1
                        int blockSize = fs.Call<int>("getBlockSize"); // API level 1
                        return (ulong)(blockCount * blockSize);
                    }
                }
                catch(AndroidJavaException)
                {
                    return 0;
                }  
            }
        }

        public ulong FreeStorage
        {
            get
            {
                if(AndroidContext.SDKVersion >= 18)
                {
                    try
                    {
                        using(var fs = StatFs(DataPath))
                        {
                            return (ulong)fs.Call<long>("getFreeBytes"); // API level 18
                        }
                    }
                    catch(AndroidJavaException)
                    {
                        return 0;
                    }
                }
                try
                {
                    using(var fs = StatFs(DataPath))
                    {
                        int blockCount = fs.Call<int>("getFreeBlocks"); // API level 1
                        int blockSize = fs.Call<int>("getBlockSize"); // API level 1
                        return (ulong)(blockCount * blockSize);
                    }
                }
                catch(AndroidJavaException)
                {
                    return 0;
                }
            }
        }

        public ulong UsedStorage
        {
            get
            {
                return TotalStorage - FreeStorage;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
    #else
    public sealed class AndroidStorageInfo : EmptyStorageInfo
    {
    }
    #endif
}
