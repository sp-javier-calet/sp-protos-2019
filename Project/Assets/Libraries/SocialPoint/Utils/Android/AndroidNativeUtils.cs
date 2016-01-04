using System;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
#if UNITY_ANDROID
    public class AndroidNativeUtils : INativeUtils
    {
        static AndroidJavaObject PackageManager
        {
            get
            {
                return AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getPackageManager");
            }
        }

        static AndroidJavaObject GetLaunchIntentForPackage(string packageName)
        {
            return PackageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        }

        public bool IsInstalled(string appId)
        {
            try
            {
                return GetLaunchIntentForPackage(appId) != null;
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
                var intent = GetLaunchIntentForPackage(appId);
                if(intent != null)
                {
                    AndroidContext.CurrentActivity.Call("startActivity", intent);
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
    }
#else
    public class AndroidNativeUtils : EmptyNativeUtils
    {
    }
#endif
}
