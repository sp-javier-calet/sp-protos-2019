using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.Base
{
#if UNITY_ANDROID
    public static class AndroidContext 
    {
        private static AndroidJavaObject _currentActivity;
        public static AndroidJavaObject CurrentActivity
        {
            get
            {
                if(_currentActivity == null)
                {
                    var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    _currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                }
                return _currentActivity;
            }
        }
        
        private static AndroidJavaObject _currentApplication;
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


        private static AndroidJavaObject _contentResolver;
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

        public static void RunOnMainThread(AndroidJavaRunnable runnable)
        {
            CurrentActivity.Call("runOnUiThread", runnable);
        }
    }
#endif
}