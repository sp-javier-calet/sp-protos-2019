﻿using System;
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
            #if UNITY_ANDROID
            #if ADMIN_PANEL
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass = new AndroidJavaClass(FullClassName);
            }
            #endif
            #endif
        }

        #if UNITY_ANDROID

        #if ADMIN_PANEL
        const string FullClassName = "es.socialpoint.unity.base.NativeUtils";
        const string FunctionClearDataAndKillApp = "ClearDataAndKillApp";
        static AndroidJavaClass _notifClass;
        #endif

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

        public override void OpenReview()
        {
            Application.OpenURL(string.Format("market://details?id={0}", _appInfo.Id));
        }

        #if ADMIN_PANEL
        public override void ClearDataAndKillApp()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic<bool>(FunctionClearDataAndKillApp);
            }
        }
        #endif

        #endif
    }

}
