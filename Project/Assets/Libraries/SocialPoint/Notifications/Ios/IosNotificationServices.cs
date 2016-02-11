using UnityEngine;
using System;
using SocialPoint.ServerSync;

#if UNITY_IOS
using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using LocalNotificationType = UnityEngine.iOS.NotificationType;
using RemoteNotificationType = UnityEngine.iOS.NotificationType;
#endif

namespace SocialPoint.Notifications
{
    #if UNITY_IOS
    public class IosNotificationServices : BaseNotificationServices
    {
        const string TokenSeparator = "-";
        const LocalNotificationType _localNotifyTypes = LocalNotificationType.Alert | LocalNotificationType.Badge | LocalNotificationType.Sound;
        const RemoteNotificationType _remoteNotifyTypes = RemoteNotificationType.Alert | RemoteNotificationType.Badge | RemoteNotificationType.Sound;

        public IosNotificationServices(MonoBehaviour behaviour, ICommandQueue commandqueue)
            : base(behaviour, commandqueue)
        {
            RegisterForLocal();
        }

        public override void Schedule(Notification notif)
        {
            var unotif = new LocalNotification();
            unotif.fireDate = DateTime.Now.ToLocalTime().AddSeconds(notif.FireDelay);
            unotif.alertBody = notif.Message;
            unotif.alertAction = notif.Title;
            unotif.applicationIconBadgeNumber = 1;
            if(notif.FireDelay > 0)
            {
                NotificationServices.ScheduleLocalNotification(unotif);
            }
            else
            {
                NotificationServices.PresentLocalNotificationNow(unotif);
            }
        }

        public override void CancelPending()
        {
            NotificationServices.CancelAllLocalNotifications();
        }

        protected override void RequestPushNotificationToken()
        {
            NotificationServices.RegisterForNotifications(_remoteNotifyTypes, true);

            WaitForRemoteToken(() => {
                string token = null;
                byte[] byteToken = NotificationServices.deviceToken;
                if(byteToken != null)
                {
                    token = BitConverter.ToString(byteToken).Replace(TokenSeparator, string.Empty).ToLower();
                }
                return token;
            });
        }

        public override void ClearReceived()
        {
            NotificationServices.ClearRemoteNotifications();
            NotificationServices.ClearLocalNotifications();
            var unotif = new LocalNotification();
            unotif.fireDate = DateTime.Now.ToLocalTime();
            unotif.applicationIconBadgeNumber = -1;
            NotificationServices.PresentLocalNotificationNow(unotif);
        }

        void RegisterForLocal()
        {
            NotificationServices.RegisterForNotifications(_localNotifyTypes, false);
        }
    }

    #else
    public class IosNotificationServices : EmptyNotificationServices
    {
    }
    #endif
}
