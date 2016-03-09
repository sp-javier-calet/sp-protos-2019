using System.Collections.Generic;
using SocialPoint.ServerSync;
using SocialPoint.Utils;
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

        public AndroidNotificationServices(ICoroutineRunner runner, ICommandQueue commandqueue)
            : base(runner, commandqueue)
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

        protected override void RequestPushNotificationToken()
        {
            if(_notifClass != null)
            {
                _notifClass.CallStatic("registerForRemote");

                WaitForRemoteToken(() => _notifClass.CallStatic<string>("getNotificationToken"));
            }
        }

        public override void RequestLocalNotification()
        {
        }

    }


#else
    public class AndroidNotificationServices : EmptyNotificationServices
    {
    }

    #endif // UNITY_ANDROID
}
