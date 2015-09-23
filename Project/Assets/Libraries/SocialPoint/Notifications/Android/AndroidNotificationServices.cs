using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Notifications
{
#if UNITY_ANDROID
    public class AndroidNotificationServices : INotificationServices
    {
        private const string PlayerPrefsIdsKey = "AndroidNotificationScheduledList";
        private const string FullClassName = "es.socialpoint.unity.notifications.NotificationBridge";

        private List<int> _notifications = new List<int>();
        private AndroidJavaClass _notifClass = null;

        public AndroidNotificationServices()
        {
            _notifClass = new AndroidJavaClass(FullClassName);
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

            long delayTime = notif.FireDelay;
            string title = notif.AlertAction;
            string message = notif.AlertBody;
            string ticker = string.Empty;
            long rep = notif.RepeatingSeconds;
            string largeIcon = "notify_icon_big";
            string smallIcon = "notify_icon_small";

            _notifClass.CallStatic("Schedule", notifId, delayTime, title, message, ticker, rep, largeIcon, smallIcon);
        }

        public void ClearReceived()
        {
            _notifications.Clear();
            SavePlayerPrefs();
            _notifClass.CallStatic("ClearReceived");
        }

        public void CancelPending()
        {
            _notifClass.CallStatic("CancelPending", _notifications.ToArray());
        }

        public void RegisterForRemote()
        {
            _notifClass.CallStatic("RegisterForRemote");
        }
    }
#else
    public class AndroidNotificationServices : EmptyNotificationServices
    {
    }
#endif
}