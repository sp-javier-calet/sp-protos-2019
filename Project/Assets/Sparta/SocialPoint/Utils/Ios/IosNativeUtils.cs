using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace SocialPoint.Utils
{
    public class IosNativeUtils : INativeUtils
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ForceTouchShortcutItem
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string Type;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Title;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Subtitle;
            [MarshalAs(UnmanagedType.LPStr)]
            public string IconPath;

            /// <summary>
            /// Struct used for the iOS Force Touch shortcuts.
            /// The Type and Title are mandatory, while the Subtitle and IconPath are not
            /// </summary>
            /// <param name="type">When retrieving the the used shortcut will receive the given Type</param>
            /// <param name="title">The localized Title</param>
            /// <param name="subtitle">The localized Subtitle</param>
            /// <param name="iconPath">The icon must be 70x70 and should be placed within a StreamingAssets folder</param>
            public ForceTouchShortcutItem(string type, string title, string subtitle = "", string iconPath = "")
            {
                Type = type;
                Title = title;
                Subtitle = subtitle;
                IconPath = string.IsNullOrEmpty(iconPath) ? "" : string.Concat("Data/Raw/", iconPath);
            }
        };

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

        #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
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

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void SPUnitySetForceTouchShortcutItems(ForceTouchShortcutItem[] shortcuts, int itemsCount);
#endif
        public static ForceTouchShortcutItem[] ForceTouchShortcutItems
        {
            set
            {
                #if UNITY_IOS && !UNITY_EDITOR
                
                SPUnitySetForceTouchShortcutItems(value, value.Length);
                
                #endif
            }
        }
    }
}