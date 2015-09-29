using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Notifications
{
    public partial class AndroidNotificationServices
    {
        public const string DefaultLargeIcon = "default_notify_icon_large";
        public const string DefaultSmallIcon = "default_notify_icon_small";
        public static readonly Color DefaultIconBackgroundColor = Color.grey;

        public string LargeIcon = DefaultLargeIcon;
        public string SmallIcon = DefaultSmallIcon;
        public Color IconBrackgroundColor = DefaultIconBackgroundColor;
    }

#if UNITY_ANDROID
    public partial class AndroidNotificationServices : INotificationServices
    {
        private const string PlayerPrefsIdsKey = "AndroidNotificationScheduledList";
        private const string FullClassName = "es.socialpoint.unity.notifications.NotificationBridge";

        private List<int> _notifications = new List<int>();

#if !UNITY_EDITOR
        private AndroidJavaClass _notifClass = null;
#endif
        
        public AndroidNotificationServices()
        {
#if !UNITY_EDITOR
            _notifClass = new AndroidJavaClass(FullClassName);
#endif
            LoadPlayerPrefs();
        }

        private void SavePlayerPrefs()
        {
            if(_notifications.Count == 0)
            {
                if(PlayerPrefs.HasKey(PlayerPrefsIdsKey))
                {
                    PlayerPrefs.DeleteKey(PlayerPrefsIdsKey);
                }
            }
            else
            {
                var sb = new StringBuilder();
                for(int i = 0; i < _notifications.Count; i++)
                {
                    sb.Append(_notifications[i]);
                    if(i < (_notifications.Count - 1))
                    {
                        sb.Append("|");
                    }
                }
                PlayerPrefs.SetString(PlayerPrefsIdsKey, sb.ToString());
            }

            PlayerPrefs.Save();
        }

        private void LoadPlayerPrefs()
        {
            if(PlayerPrefs.HasKey(PlayerPrefsIdsKey))
            {
                string[] strArray = PlayerPrefs.GetString(PlayerPrefsIdsKey).Split("|"[0]);
                for(int i = 0; i < strArray.Length; i++)
                {
                    _notifications.Add(int.Parse(strArray[i]));
                }
            }
        }

        int ColorToInt(Color c)
        {
            return (((int)(c.r * 255)) << 16) + ((int)(c.g * 255) << 8) + (int)(c.b * 255);
        }

        public void Schedule(Notification notif)
        {
            var notifId = 0;
            foreach(var id in _notifications)
            {
                if(id >= notifId)
                {
                    notifId = id+1;
                }
            }
            _notifications.Add(notifId);
            SavePlayerPrefs();

#if !UNITY_EDITOR
            long delayTime = notif.FireDelay;
            string title = notif.Title;
            string message = notif.Message;
            int color = ColorToInt(IconBrackgroundColor);
            _notifClass.CallStatic("Schedule", notifId, delayTime, title, message, LargeIcon, SmallIcon, color);
#endif
        }

        public void ClearReceived()
        {
            _notifications.Clear();
            SavePlayerPrefs();
#if !UNITY_EDITOR
            _notifClass.CallStatic("ClearReceived");
#endif
        }

        public void CancelPending()
        {
#if !UNITY_EDITOR
            _notifClass.CallStatic("CancelPending", _notifications.ToArray());
#endif
        }

        public void RegisterForRemote()
        {
#if !UNITY_EDITOR
            _notifClass.CallStatic("RegisterForRemote");
#endif
        }
    }
#else
    public partial class AndroidNotificationServices : EmptyNotificationServices
    {
    }
#endif
}