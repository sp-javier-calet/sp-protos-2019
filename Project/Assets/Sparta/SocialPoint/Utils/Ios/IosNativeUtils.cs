using System;
using UnityEngine;
using System.Runtime.InteropServices;
using SocialPoint.Login;

namespace SocialPoint.Utils
{
    public sealed class IosNativeUtils : UnityNativeUtils
    {
        public IosNativeUtils(ILoginData loginData): base(loginData)
        {
        }

#if (UNITY_IOS || UNITY_TVOS)

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

        [DllImport ("__Internal")]
        static extern bool SPUnityNativeUtilsIsInstalled(string appId);


        public override bool IsInstalled(string appId)
        {
            return SPUnityNativeUtilsIsInstalled(appId);
        }

        public override void OpenStore(string appId)
        {
            Application.OpenURL(string.Format("itms-apps://itunes.apple.com/app/id{0}", appId));
        }

        [DllImport ("__Internal")]
        static extern bool SPUnityNativeUtilsOpenReview();

        public override void OpenReview()
        {
            try
            {
                base.OpenReview();
            }
            catch(Exception)
            {
                SPUnityNativeUtilsOpenReview();
            }
        }

        [DllImport ("__Internal")]
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

#endif

    }
}