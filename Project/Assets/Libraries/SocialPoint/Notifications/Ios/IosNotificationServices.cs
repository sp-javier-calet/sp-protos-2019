using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.ServerSync;

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
    public class IosNotificationServices : INotificationServices
    {
        public delegate void DeviceTokenReceived();

        // Event fired when the device token is returned by Apple Push Service
        public event DeviceTokenReceived OnDeviceTokenReceived;
             
        private byte[] _byteToken = null;
        private string _stringToken = null;
        private MonoBehaviour _behaviour;
        private ICommandQueue _commandQueue;                
        private const string TokenSeparator = "-";
        private const LocalNotificationType _localNotifyTypes = LocalNotificationType.Alert | LocalNotificationType.Badge | LocalNotificationType.Sound;
        private const RemoteNotificationType _remoteNotifyTypes = RemoteNotificationType.Alert | RemoteNotificationType.Badge | RemoteNotificationType.Sound;

        public IosNotificationServices(MonoBehaviour behaviour, ICommandQueue commandQueue=null)
        {
            if(behaviour == null)
            {
                throw new ArgumentNullException("behaviour", "behaviour cannot be null or empty!");
            }
            _behaviour = behaviour;
            _commandQueue = commandQueue;
            RegisterForLocal();
        }

        public void Schedule(Notification notif)
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

        public void CancelPending()
        {
            NotificationServices.CancelAllLocalNotifications();
        }

        public void RegisterForRemote()
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            NotificationServices.RegisterForRemoteNotificationTypes(_remoteNotifyTypes);
#else
            NotificationServices.RegisterForNotifications(_remoteNotifyTypes, true);
#endif
            _behaviour.StartCoroutine(CheckDeviceToken());
        }
        
        public void ClearReceived()
        {
            NotificationServices.ClearRemoteNotifications();
            NotificationServices.ClearLocalNotifications();
            var unotif = new LocalNotification();
            unotif.fireDate = DateTime.Now.ToLocalTime();
            unotif.applicationIconBadgeNumber = -1;
            NotificationServices.PresentLocalNotificationNow(unotif);
        }       

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