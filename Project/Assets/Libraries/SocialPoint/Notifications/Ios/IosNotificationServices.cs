using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.ServerSync;

using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;

namespace SocialPoint.Notifications
{
    public class IosNotificationServices : INotificationServices
    {
        public delegate void DeviceTokenReceived();

        // Event fired when the device token is returned by Apple Push Service
        public event DeviceTokenReceived OnDeviceTokenReceived;
             
        private byte[] _byteToken = null;
        private string _stringToken = null;
        private MonoBehaviour _behaviour;
        private ICommandQueue _commandQueue;

        public IosNotificationServices(MonoBehaviour behaviour, ICommandQueue commandQueue=null)
        {
            if(behaviour == null)
            {
                throw new ArgumentNullException("behaviour", "behaviour cannot be null or empty!");
            }
            _behaviour = behaviour;
            _commandQueue = commandQueue;
        }

        public void ScheduleLocalNotification(Notification notif)
        {
            var unotif = new LocalNotification();
            unotif.fireDate = notif.FireDate;
            unotif.applicationIconBadgeNumber = notif.IconBadgeNumber;
            unotif.alertBody = notif.AlertBody;
            unotif.alertAction = notif.AlertAction;
            var localTime = DateTime.Now.ToLocalTime();
            int result = DateTime.Compare(notif.FireDate, localTime);
            
            if (result > 0)
            {
                NotificationServices.ScheduleLocalNotification(unotif);
            }
            else
            {
                NotificationServices.PresentLocalNotificationNow(unotif);
            }
        }

        public void ClearLocalNotifications()
        {
            NotificationServices.ClearLocalNotifications();
        }

        public void CancelAllLocalNotifications()
        {
            NotificationServices.CancelAllLocalNotifications();
        }

        const string TokenSeparator = "-";

        private IEnumerator CheckDeviceToken()
        {
            while(_byteToken == null)
            {
                _byteToken = NotificationServices.deviceToken;
                yield return null;
            }

            _stringToken = BitConverter.ToString(_byteToken).Replace(TokenSeparator, string.Empty).ToLower();
            SendPushToken();
            if(OnDeviceTokenReceived != null)
            {
                OnDeviceTokenReceived();
            }
        }

        private void SendPushToken()
        {
            if(_commandQueue != null && !string.IsNullOrEmpty(_stringToken))
            {
                _commandQueue.Add(new PushEnabledCommand(_stringToken));
            }
        }

        const NotificationType _notifyTypes = NotificationType.Alert | NotificationType.Badge | NotificationType.Sound;

        public void RegisterForRemoteNotificationTypes()
        {
            NotificationServices.RegisterForNotifications(_notifyTypes, true);
            _behaviour.StartCoroutine(CheckDeviceToken());
        }

        public void UnregisterForRemoteNotifications()
        {
            NotificationServices.UnregisterForRemoteNotifications();
        }

        public void ClearRemoteNotifications()
        {
            NotificationServices.ClearRemoteNotifications();
        }

        public void RegisterForLocalNotificationTypes()
        {
            NotificationServices.RegisterForNotifications(_notifyTypes, false);
        }

        public void ResetIconBadgeNumber()
        {
            //This is supposed to  remove the Icon Badge Number: http://forum.unity3d.com/threads/using-the-new-notification-system.127016/
            var unotif = new LocalNotification();
            unotif.fireDate = DateTime.Now.ToLocalTime();
            unotif.applicationIconBadgeNumber = -1;
            unotif.alertBody = string.Empty;
            NotificationServices.PresentLocalNotificationNow(unotif);
        }
    }
}