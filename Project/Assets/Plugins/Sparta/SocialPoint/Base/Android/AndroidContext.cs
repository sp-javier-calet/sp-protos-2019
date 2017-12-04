#if UNITY_ANDROID
using UnityEngine;

namespace SocialPoint.Base
{
    public static class AndroidContext
    {
        static AndroidJavaObject _currentActivity;

        public static AndroidJavaObject CurrentActivity
        {
            get
            {
                if(_currentActivity == null)
                {
                    using(var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        _currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                    }
                }
                return _currentActivity;
            }
        }

        static AndroidJavaObject _currentApplication;

        public static AndroidJavaObject CurrentApplication
        {
            get
            {
                if(_currentApplication == null)
                {
                    _currentApplication = CurrentActivity.Call<AndroidJavaObject>("getApplication");
                }
                return _currentApplication;
            }
        }

        static AndroidJavaObject _contentResolver;

        public static AndroidJavaObject ContentResolver
        {
            get
            {
                if(_contentResolver == null)
                {
                    _contentResolver = AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getContentResolver");
                }
                return _contentResolver;
            }
        }

        public static AndroidJavaObject PackageManager
        {
            get
            {
                return AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getPackageManager"); // API level 1
            }
        }

        public static void RunOnMainThread(AndroidJavaRunnable runnable)
        {
            CurrentActivity.Call("runOnUiThread", runnable);
        }

        static int _sdkVersion;

        public static int SDKVersion
        {
            get
            {
                if(_sdkVersion == 0)
                {
                    using(var buildVersionObject = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        _sdkVersion = buildVersionObject.GetStatic<int>("SDK_INT"); // API level 4
                    }
                }
                return _sdkVersion;
            }
        }
    }
}
#endif
