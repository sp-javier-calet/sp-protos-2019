#if UNITY_IOS && !UNITY_EDITOR
#define IOS_DEVICE
#endif

#if UNITY_IOS
using System.Collections;
using System.Runtime.InteropServices;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
#endif

namespace SocialPoint.Notifications
{
    #if UNITY_IOS
    public sealed class IosNotificationServices : BaseNotificationServices
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct NotificationData
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Message;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string Title;
            [MarshalAs(UnmanagedType.I8)]
            public long FireDelay;
            [MarshalAs(UnmanagedType.I4)]
            public int IconBadgeNumber;
        };

        static class NotificationServices
        {
            #if IOS_DEVICE
            [DllImport("__Internal")] public static extern void SPUnityNotificationsScheduleLocalNotification(NotificationData data);

            [DllImport("__Internal")] public static extern void SPUnityNotificationsPresentLocalNotification(NotificationData data);

            [DllImport("__Internal")] public static extern void SPUnityNotificationsCancelAllLocalNotifications();

            [DllImport("__Internal")] public static extern void SPUnityNotificationsClearAllLocalNotifications();

            [DllImport("__Internal")] public static extern void SPUnityNotificationsRegisterForNotifications();
            
            [DllImport("__Internal")] public static extern string SPUnityNotificationsDeviceToken();

            [DllImport("__Internal")] public static extern string SPUnityNotificationsRegistrationError();

#else

            public static void SPUnityNotificationsScheduleLocalNotification(NotificationData data)
            {
            }

            public static void SPUnityNotificationsPresentLocalNotification(NotificationData data)
            {
            }

            public static void SPUnityNotificationsCancelAllLocalNotifications()
            {
            }

            public static void SPUnityNotificationsClearAllLocalNotifications()
            {
            }

            public static void SPUnityNotificationsRegisterForNotifications()
            {
            }

            public static string SPUnityNotificationsDeviceToken()
            {
                return null;
            }

            public static string SPUnityNotificationsRegistrationError()
            {
                return null;
            }
            #endif
        }

        const string TokenSeparator = "-";
        const float PushTokenTimeout = 30.0f;

        IEnumerator _checkPermissionStatusCoroutine;

        public IosNotificationServices(ICoroutineRunner runner, INativeUtils nativeUtils)
            : base(runner, nativeUtils)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            _runner.StopCoroutine(_checkPermissionStatusCoroutine);
        }

        public override void Schedule(Notification notif)
        {
            var notifData = new NotificationData {
                Message = string.Empty, Title = string.Empty, FireDelay = 0, IconBadgeNumber = 0
            };
            
            notifData.Message = notif.Message;
            notifData.Title = notif.Title;
            notifData.FireDelay = notif.FireDelay;
            notifData.IconBadgeNumber = 1;

            if(notif.FireDelay > 0)
            {
                NotificationServices.SPUnityNotificationsScheduleLocalNotification(notifData);
            }
            else
            {
                NotificationServices.SPUnityNotificationsPresentLocalNotification(notifData);
            }
        }

        public override void CancelPending()
        {
            NotificationServices.SPUnityNotificationsCancelAllLocalNotifications();
        }

        public override void ClearReceived()
        {
            NotificationServices.SPUnityNotificationsClearAllLocalNotifications();
        }

        public override void RequestPermissions()
        {
            DebugLog("RequestPermissions");

            if(_checkPermissionStatusCoroutine == null)
            {
                NotificationServices.SPUnityNotificationsRegisterForNotifications();
                _checkPermissionStatusCoroutine = _runner.StartCoroutine(CheckPermissionStatus());
            }
        }

        IEnumerator CheckPermissionStatus()
        {
            float startTime = Time.unscaledTime;
            float currentTime = startTime;
            string pushToken = string.Empty;
            string registrationError = string.Empty;
            var delay = new WaitForSeconds(1.0f);
            while(string.IsNullOrEmpty(pushToken) && string.IsNullOrEmpty(registrationError) && (currentTime - startTime) < PushTokenTimeout)
            {
                pushToken = NotificationServices.SPUnityNotificationsDeviceToken();
                registrationError = NotificationServices.SPUnityNotificationsRegistrationError();
                yield return delay;
                currentTime = Time.unscaledTime;
            }

            DebugLog("CheckPermissionStatus\n\tpushToken: " + pushToken + "\n\tregistrationError: " + registrationError);

            if(!string.IsNullOrEmpty(pushToken))
            {
                _pushToken = pushToken;
                OnRequestPermissionsSuccess();
            }
            else
            {
                _pushToken = "";
                OnRequestPermissionsFail();
            }

            _checkPermissionStatusCoroutine = null;
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugNotificationsFlag)]
        void DebugLog(string msg)
        {
            const string tag = "SocialPoint.Notifications-DebugLog";
            Log.i(tag, msg);
        }

        public override void SetupChannels(NotificationChannel[] channels)
        {
        }
    }
    
    #else
    public sealed class IosNotificationServices : EmptyNotificationServices
    {
    }
    #endif
}
