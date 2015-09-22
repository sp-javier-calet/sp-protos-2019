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
        private const string PlayerNotificationId = "AndroidNotificationId";
        private const string PlayerNotificationScheduledList = "AndroidNotificationScheduledList";
        private const string FullClassName = "es.socialpoint.androidnotifications.NotificationsManager";

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
                if(PlayerPrefs.HasKey(PlayerNotificationScheduledList))
                {
                    PlayerPrefs.DeleteKey(PlayerNotificationScheduledList);
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
                PlayerPrefs.SetString(PlayerNotificationScheduledList, sb.ToString());
            }

            PlayerPrefs.Save();
        }

        private void LoadPlayerPrefs()
        {
            if(PlayerPrefs.HasKey(PlayerNotificationScheduledList))
            {
                string[] strArray = PlayerPrefs.GetString(PlayerNotificationScheduledList).Split("|"[0]);
                for(int i = 0; i < strArray.Length; i++)
                {
                    _notifications.Add(int.Parse(strArray[i]));
                }
            }
        }
                
        // Schedules a local notification
        public void Schedule(Notification notif)
        {
            var localTime = DateTime.Now.ToLocalTime();
            double delayTime = notif.FireDate.Subtract(localTime).TotalSeconds;
            delayTime = delayTime < 0 ? 0 : delayTime;
            var notifId = 0;
            foreach(var id in _notifications)
            {
                if(id >= notifId)
                {
                    notifId = id+1;
                }
            }
            _notifClass.CallStatic("CreateLocalNotification", AndroidContext.CurrentActivity, notif.AlertAction, notif.AlertBody, (long)delayTime, notifId, notif.RepeatingSeconds);
            _notifications.Add(notifId);
            SavePlayerPrefs();
        }

        // Discards of all received local notifications
        public void ClearReceived()
        {
            _notifications.Clear();
            SavePlayerPrefs();
            // TODO TECH
            //_notifClass.CallStatic("ClearReceivedRemote", AndroidContext.CurrentActivity);
        }

        public void CancelPending()
        {
            var intArr = _notifications.ToArray();
            _notifClass.CallStatic("CancelAllLocalNotifications", AndroidContext.CurrentActivity, intArr);
        }

        public void RegisterForRemote()
        {
            // TODO TECH
            //_notifClass.CallStatic("RegisterForRemote", AndroidContext.CurrentActivity, "AppName", "Email");
        }
    }
#else
    public class AndroidNotificationServices : EmptyNotificationServices
    {
    }
#endif
}