using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SocialPoint.Utils
{
    public class IosNativeUtils : INativeUtils
    {
        #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        static extern bool SPUnityNativeUtilsIsInstalled(string appId);
        #else
        static bool SPUnityNativeUtilsIsInstalled(string appId)
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        public bool IsInstalled(string appId)
        {
            return SPUnityNativeUtilsIsInstalled(appId);
        }
        
        public void OpenApp(string appId)
        {
            if(IsInstalled(appId))
            {
                OpenUrl(appId);
            }
        }
        
        public void OpenStore(string appId)
        {
            OpenUrl(string.Format("itms-apps://itunes.apple.com/app/id{0}", appId));
        }
        
        public void OpenUrl(string url)
        {
            Application.OpenURL(url);
        }

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        static extern bool SPUnityNativeUtilsUserAllowNotification();
        #else
        public bool SPUnityNativeUtilsUserAllowNotification()
        {
            return true;
        }
        #endif

        public bool UserAllowNotification
        {
            get
            {
                return SPUnityNativeUtilsUserAllowNotification();
            }
        }
    }
}