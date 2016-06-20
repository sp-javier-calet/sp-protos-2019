using System;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    #if UNITY_ANDROID
    public class AndroidNativeUtils : INativeUtils
    {
        static AndroidJavaObject GetLaunchIntentForPackage(string packageName)
        {
            using(var packageManager = AndroidContext.PackageManager)
            {
                return packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            }
        }

        public bool IsInstalled(string appId)
        {
            try
            {
                using(var intent = GetLaunchIntentForPackage(appId))
                {
                    return intent != null;
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        public void OpenApp(string appId)
        {
            try
            {

                using(var intent = GetLaunchIntentForPackage(appId))
                {
                    if(intent != null)
                    {
                        AndroidContext.CurrentActivity.Call("startActivity", intent);
                    }
                }
            }
            catch(Exception)
            {
            }
        }

        public void OpenStore(string appId)
        {
            OpenUrl(string.Format("market://details?id={0}", appId));
        }

        public void OpenUrl(string url)
        {
            Application.OpenURL(url);
        }

        public bool UserAllowNotification
        {
            get
            {
                return true; // This only makes sense on IOS
            }
        }
    }
    #else
    public class AndroidNativeUtils : EmptyNativeUtils
    {
    }
#endif
}
