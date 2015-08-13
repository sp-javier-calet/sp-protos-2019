using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.Base
{
#if UNITY_ANDROID
    public class AndroidContext 
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

        public static void RunOnMainThread(AndroidJavaRunnable runnable)
        {
            CurrentActivity.Call("runOnUiThread", runnable);
        }
    }
#endif
}