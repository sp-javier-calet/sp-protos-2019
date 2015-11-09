using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
using LocalNotification = UnityEngine.LocalNotification;
using NotificationServices = UnityEngine.NotificationServices;
using LocalNotificationType = UnityEngine.LocalNotificationType;
using RemoteNotificationType = UnityEngine.RemoteNotificationType;
#else
using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using LocalNotificationType = UnityEngine.iOS.NotificationType;
using RemoteNotificationType = UnityEngine.iOS.NotificationType;
#endif
#endif

namespace SocialPoint.Notifications
{
#if UNITY_IOS
    public class IosNotificationServices : BaseNotificationServices
    {
        private const string TokenSeparator = "-";
        private const LocalNotificationType _localNotifyTypes = LocalNotificationType.Alert | LocalNotificationType.Badge | LocalNotificationType.Sound;
        private const RemoteNotificationType _remoteNotifyTypes = RemoteNotificationType.Alert | RemoteNotificationType.Badge | RemoteNotificationType.Sound;

        public IosNotificationServices(MonoBehaviour behaviour, ICommandQueue commandQueue = null)
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

        public override void RegisterForRemote()
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            NotificationServices.RegisterForRemoteNotificationTypes(_remoteNotifyTypes);
#else
            NotificationServices.RegisterForNotifications(_remoteNotifyTypes, true);
#endif

            WaitForRemoteToken(()=> {
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

        private void RegisterForLocal()
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            NotificationServices.RegisterForLocalNotificationTypes(_localNotifyTypes);
#else
            NotificationServices.RegisterForNotifications(_localNotifyTypes, false);
#endif
        }

    }

#else
    public class IosNotificationServices : EmptyNotificationServices
    {
    }
#endif
}