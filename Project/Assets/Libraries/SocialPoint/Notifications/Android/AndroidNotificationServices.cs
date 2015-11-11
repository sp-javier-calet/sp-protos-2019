using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.ServerSync;
using UnityEngine;

namespace SocialPoint.Notifications
{

#if UNITY_ANDROID
    public class AndroidNotificationServices : BaseNotificationServices
    {
        private const string PlayerPrefsIdsKey = "AndroidNotificationScheduledList";
        private const string FullClassName = "es.socialpoint.unity.notification.NotificationBridge";

        private List<int> _notifications = new List<int>();

        private AndroidJavaClass _notifClass = null;
        
        public AndroidNotificationServices(MonoBehaviour behaviour, ICommandQueue commandqueue)
        : base(behaviour, commandqueue)
        {
#if !UNITY_EDITOR
            _notifClass = new AndroidJavaClass(FullClassName);
#endif
        }

        int ColorToInt(Color c)
        {
            return (((int)(c.r * 255)) << 16) + ((int)(c.g * 255) << 8) + (int)(c.b * 255);
        }

        public override void Schedule(Notification notif)
        {
            if(_notifClass != null)
            {
                _notifClass.CallStatic("schedule", 0, notif.FireDelay, notif.Title, notif.Message);
            }
        }

        public override void ClearReceived()
        {
            _notifications.Clear();
            if(_notifClass != null)
            {
                _notifClass.CallStatic("clearReceived");
            }
        }

        public override void CancelPending()
        {
            if(_notifClass != null)
            {
                _notifClass.CallStatic("cancelPending");
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
    public class AndroidNotificationServices : EmptyNotificationServices
    {
    }
#endif // UNITY_ANDROID
}