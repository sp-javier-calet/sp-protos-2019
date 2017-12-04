using System;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Hardware;

namespace SocialPoint.Utils
{
    public sealed class AndroidNativeUtils : UnityNativeUtils
    {
        public AndroidNativeUtils(IAppInfo appInfo) : base(appInfo)
        {
        }

#if UNITY_ANDROID

        static AndroidJavaObject GetLaunchIntentForPackage(string packageName)
        {
            using(var packageManager = AndroidContext.PackageManager)
            {
                return packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            }
        }

        public override bool IsInstalled(string appId)
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

        public override void OpenApp(string appId)
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

        public override void OpenStore(string appId)
        {
            Application.OpenURL(string.Format("market://details?id={0}", appId));
        }

#endif

    }

}
