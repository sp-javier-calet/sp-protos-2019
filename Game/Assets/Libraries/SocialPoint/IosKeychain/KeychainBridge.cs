using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace SocialPoint.IosKeychain
{
    public static class KeychainBridge {

        [StructLayout(LayoutKind.Sequential)]
        public struct ItemStruct
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
            Id;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
            Service;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
            AccessGroup;
        };

        #if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern int SPUnityKeychainSet(ItemStruct item, string value);
        #else
        public static int SPUnityKeychainSet(ItemStruct item, string value)
        {
            throw new NotImplementedException("Keychain is only supported on Ios");
        }
        #endif

        #if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityKeychainGet(ItemStruct item);
        #else
        public static string SPUnityKeychainGet(ItemStruct item)
        {
            throw new NotImplementedException("Keychain is only supported on Ios");
        }
        #endif

        #if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern int SPUnityKeychainClear(ItemStruct item);
        #else
        public static int SPUnityKeychainClear(ItemStruct item)
        {
            throw new NotImplementedException("Keychain is only supported on Ios");
        }
        #endif

        #if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport ("__Internal")]
        public static extern string SPUnityKeychainGetDefaultAccessGroup();
        #else
        public static string SPUnityKeychainGetDefaultAccessGroup()
        {
            throw new NotImplementedException("Keychain is only supported on Ios");
        }
        #endif

    }
}
