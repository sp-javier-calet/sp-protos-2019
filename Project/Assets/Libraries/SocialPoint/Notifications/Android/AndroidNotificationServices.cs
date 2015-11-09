using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.ServerSync;
using UnityEngine;

namespace SocialPoint.Notifications
{
    public struct AndroidNotificationSettings 
    {
        public const string DefaultLargeIcon = "default_notify_icon_large";
        public const string DefaultSmallIcon = "default_notify_icon_small";
        public static readonly Color DefaultIconBackgroundColor = Color.grey;

        public string LargeIcon;
        public string SmallIcon;
        public Color IconBackgroundColor;


        public static AndroidNotificationSettings Default
        {
            get
            {
                return new AndroidNotificationSettings{
                    LargeIcon = DefaultLargeIcon,
                    SmallIcon = DefaultSmallIcon,
                    IconBackgroundColor = DefaultIconBackgroundColor
                };
            }
        }
    }

#if UNITY_ANDROID
    public partial class AndroidNotificationServices : BaseNotificationServices
    {
        private const string PlayerPrefsIdsKey = "AndroidNotificationScheduledList";
        private const string FullClassName = "es.socialpoint.unity.notification.NotificationBridge";

        private List<int> _notifications = new List<int>();

        private AndroidJavaClass _notifClass = null;
        private AndroidNotificationSettings _settings;
        
        public AndroidNotificationServices(MonoBehaviour behaviour, ICommandQueue commandqueue, AndroidNotificationSettings settings)
        : base(behaviour, commandqueue)
        {
#if !UNITY_EDITOR
            _notifClass = new AndroidJavaClass(FullClassName);
#endif
            _settings = settings;
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

        public override void Schedule(Notification notif)
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
            string title = notif.Title;
            string message = notif.Message;
            int color = ColorToInt(_settings.IconBackgroundColor);
            if(_notifClass != null)
            {
                _notifClass.CallStatic("schedule", notifId, delayTime, title, message, _settings.LargeIcon, _settings.SmallIcon, color);
            }
        }

        public override void ClearReceived()
        {
            _notifications.Clear();
            SavePlayerPrefs();
            if(_notifClass != null)
            {
                _notifClass.CallStatic("clearReceived");
            }
        }

        public override void CancelPending()
        {
            if(_notifClass != null)
            {
                _notifClass.CallStatic("cancelPending", _notifications.ToArray());
            }
        }

        public override void RegisterForRemote()
        {
            if(_notifClass != null)
            {
                _notifClass.CallStatic("registerForRemote");

                WaitForRemoteToken(() => {
                    return _notifClass.CallStatic<string>("getNotificationToken");
                });
            }
        }
    }
#else
    public partial class AndroidNotificationServices : EmptyNotificationServices
    {
    }
#endif // UNITY_ANDROID
}