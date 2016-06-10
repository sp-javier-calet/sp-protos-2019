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
        const float PushTokenTimeout = 30.0f;

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
        }

        public override void RequestPermissions()
        {
            if(_checkPermissionStatusCoroutine == null)
            {
                NotificationServices.RegisterForNotifications(NotificationTypes, true);
                _checkPermissionStatusCoroutine = _runner.StartCoroutine(CheckPermissionStatus());
            }
        }

        IEnumerator CheckPermissionStatus()
        {
            float startTime = Time.unscaledTime;
            float currentTime = startTime;
            byte[] byteToken = null;
            string registrationError = null;
            while(byteToken == null && string.IsNullOrEmpty(registrationError) && (currentTime - startTime) < PushTokenTimeout)
            {
                byteToken = NotificationServices.deviceToken;
                registrationError = NotificationServices.registrationError;
                yield return new WaitForSeconds(1.0f);
                currentTime = Time.unscaledTime;
            }

            if(byteToken != null)
            {
                _pushToken = BitConverter.ToString(byteToken).Replace(TokenSeparator, string.Empty).ToLower();
                OnRequestPermissionsSuccess();
            }
            else
            {
                _pushToken = "";
                OnRequestPermissionsFail();
            }

            _checkPermissionStatusCoroutine = null;
        }
    }

    #else
    public class IosNotificationServices : EmptyNotificationServices
    {
    }
    #endif
}
