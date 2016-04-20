#if UNITY_IOS
using UnityEngine;
using System;
using System.Collections;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

namespace SocialPoint.Notifications
{
    #if UNITY_IOS
    public class IosNotificationServices : BaseNotificationServices
    {
        const string TokenSeparator = "-";
        const NotificationType NotificationTypes = NotificationType.Alert | NotificationType.Badge | NotificationType.Sound;

        IEnumerator _checkPermissionStatusCoroutine;

        public IosNotificationServices(ICoroutineRunner runner, ICommandQueue commandqueue)
            : base(runner, commandqueue)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _runner.StopCoroutine(_checkPermissionStatusCoroutine);
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

        public override void ClearReceived()
        {
            NotificationServices.ClearRemoteNotifications();
            NotificationServices.ClearLocalNotifications();
            var unotif = new LocalNotification();
            unotif.fireDate = DateTime.Now.ToLocalTime();
            unotif.applicationIconBadgeNumber = -1;
            NotificationServices.PresentLocalNotificationNow(unotif);
        }
            
        public override void RequestPermissions()
        {
            if(_checkPermissionStatusCoroutine != null)
            {
                NotificationServices.RegisterForNotifications(NotificationTypes);
                _checkPermissionStatusCoroutine = _runner.StartCoroutine(CheckPermissionStatus());
            }
        }

        IEnumerator CheckPermissionStatus()
        {
            byte[] byteToken = NotificationServices.deviceToken;
            if(byteToken == null)
            {
                string registrationError = NotificationServices.registrationError;
                if(String.IsNullOrEmpty(registrationError))
                {
                    yield return null;
                }
                else
                {
                    _pushToken = "";
                    OnRequestPermissionsFail();
                }
            }
            else
            {
                _pushToken = BitConverter.ToString(byteToken).Replace(TokenSeparator, string.Empty).ToLower();
                OnRequestPermissionsSuccess();
            }
        }
    }

    #else
    public class IosNotificationServices : EmptyNotificationServices
    {
    }
    #endif
}
