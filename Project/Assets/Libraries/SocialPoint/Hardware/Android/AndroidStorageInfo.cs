using UnityEngine;

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public class AndroidStorageInfo : IStorageInfo
    {
        public AndroidStorageInfo()
        {
        }

        private static string _rootPath;

        public static string RootPath
        {
            get
            {
                if(_rootPath == null)
                {
                    var env = new AndroidJavaClass("android.os.Environment");
                    _rootPath = env.CallStatic<AndroidJavaObject>("getRootDirectory").Call<string>("getAbsolutePath");
                }
                return _rootPath;
            }
        }

        private static string _dataPath;

        public static string DataPath
        {
            get
            {
                if(_dataPath == null)
                {
                    var env = new AndroidJavaClass("android.os.Environment");
                    _dataPath = env.CallStatic<AndroidJavaObject>("getDataDirectory").Call<string>("getAbsolutePath");
                }
                return _dataPath;
            }
        }

        public static AndroidJavaObject StatFs(string path)
        {
            return new AndroidJavaObject("android.os.StatFs", path);
        }

        public ulong TotalStorage
        {
            get
            {
                var fs = StatFs(DataPath);
                return (ulong)fs.Call<long>("getTotalBytes");
            }
        }

        public ulong FreeStorage
        {
            get
            {
                var fs = StatFs(DataPath);
                return (ulong)fs.Call<long>("getFreeBytes");
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
    public class AndroidStorageInfo : EmptyStorageInfo
    {
    }
    #endif
}