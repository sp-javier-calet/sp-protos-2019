using System;
using UnityEngine;
using System.Runtime.InteropServices;
using SocialPoint.Login;
using SocialPoint.Hardware;

namespace SocialPoint.Utils
{

    public sealed class IosNativeUtils : UnityNativeUtils
    {
        public IosNativeUtils(IAppInfo appInfo) : base(appInfo)
        {
        }

        #if (UNITY_IOS || UNITY_TVOS)
        
        public override void OpenApp(string appId)
        {
            Application.OpenURL(appId);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IosShortcutItem
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
            public void Init(ShortcutItem item)
            {
                Type = item.Type ?? string.Empty;
                Title = item.Title ?? string.Empty;
                Subtitle = item.Subtitle ?? string.Empty;
                IconPath = string.IsNullOrEmpty(item.Icon) ? "" : string.Concat("Data/Raw/", item.Icon);
            }
        };

        [DllImport("__Internal")]
        static extern bool SPUnityNativeUtilsIsInstalled(string appId);

        public override bool IsInstalled(string appId)
        {
            return SPUnityNativeUtilsIsInstalled(appId);
        }

        string GetAppUrl(string appId, string suffix=null)
        {
            return string.Format("itms-apps://itunes.apple.com/app/id{0}{1}", appId, suffix);
        }

        public override void OpenStore(string appId)
        {
            Application.OpenURL(GetAppUrl(appId));
        }

        public override void OpenReview()
        {
            Application.OpenURL(GetAppUrl(_appInfo.Id, "?action=write-review"));
        }

        [DllImport("__Internal")]
        static extern bool SPUnityNativeUtilsSupportsReviewDialog();


        public override bool SupportsReviewDialog
        {
            get
            {
                return SPUnityNativeUtilsSupportsReviewDialog();
            }

        }

        [DllImport("__Internal")]
        static extern void SPUnityNativeUtilsDisplayReviewDialog();

        public override void DisplayReviewDialog()
        {
            SPUnityNativeUtilsDisplayReviewDialog();
        }

        [DllImport("__Internal")]
        static extern bool SPUnityNativeUtilsUserAllowNotification();

        public override bool UserAllowNotification
        {
            get
            {
                return SPUnityNativeUtilsUserAllowNotification();
            }
        }

        [DllImport("__Internal")]
        public static extern void SPUnitySetForceTouchShortcutItems(IosShortcutItem[] shortcuts, int itemsCount);

        ShortcutItem[] _shortcutItems;

        public override ShortcutItem[] ShortcutItems
        {
            get
            {
                return _shortcutItems;
            }

            set
            {
                _shortcutItems = value;
                var ios = new IosShortcutItem[value.Length];
                for(var i = 0; i < value.Length; i++)
                {
                    ios[i].Init(value[i]);
                }
                SPUnitySetForceTouchShortcutItems(ios, ios.Length);
            }
        }  


#if ADMIN_PANEL
        [DllImport("__Internal")]
        public static extern void SPUnityNativeUtilsClearDataAndKillApp();

        public override void ClearDataAndKillApp()
        {
            SPUnityNativeUtilsClearDataAndKillApp();
        }
#endif
        
#endif
    }
}