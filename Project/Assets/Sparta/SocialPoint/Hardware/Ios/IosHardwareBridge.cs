using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SocialPoint.Hardware
{
    public static class IosHardwareBridge
    {
        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetDeviceString();
        #else
        public static string SPUnityHardwareGetDeviceString()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetDevicePlatformVersion();
        #else
        public static string SPUnityHardwareGetDevicePlatformVersion()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetDeviceArchitecture();
        #else
        public static string SPUnityHardwareGetDeviceArchitecture()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif


        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetDeviceAdvertisingId();
        #else
        public static string SPUnityHardwareGetDeviceAdvertisingId()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern bool SPUnityHardwareGetDeviceAdvertisingIdEnabled();
        #else
        public static bool SPUnityHardwareGetDeviceAdvertisingIdEnabled()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern bool SPUnityHardwareGetDeviceRooted();
        #else
        public static bool SPUnityHardwareGetDeviceRooted()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetTotalMemory();
        #else
        public static ulong SPUnityHardwareGetTotalMemory()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetFreeMemory();
        #else
        public static ulong SPUnityHardwareGetFreeMemory()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetUsedMemory();
        #else
        public static ulong SPUnityHardwareGetUsedMemory()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetActiveMemory();
        #else
        public static ulong SPUnityHardwareGetActiveMemory()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetAppId();
        #else
        public static string SPUnityHardwareGetAppId()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetAppVersion();
        #else
        public static string SPUnityHardwareGetAppVersion()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetAppShortVersion();
        #else
        public static string SPUnityHardwareGetAppShortVersion()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetAppLanguage();
        #else
        public static string SPUnityHardwareGetAppLanguage()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetAppCountry();
        #else
        public static string SPUnityHardwareGetAppCountry()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetNetworkConnectivity();
        #else
        public static string SPUnityHardwareGetNetworkConnectivity()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetNetworkProxy();
        #else
        public static string SPUnityHardwareGetNetworkProxy()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityHardwareGetNetworkIpAddress();
        #else
        public static string SPUnityHardwareGetNetworkIpAddress()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetTotalStorage();
        #else
        public static ulong SPUnityHardwareGetTotalStorage()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetFreeStorage();
        #else
        public static ulong SPUnityHardwareGetFreeStorage()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

        #if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern ulong SPUnityHardwareGetUsedStorage();
        #else
        public static ulong SPUnityHardwareGetUsedStorage()
        {
            throw new NotImplementedException("Only iOS Supported");
        }
        #endif

    }
}