using System.Collections.Generic;
using System.Collections;
using SocialPoint.ServerSync;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.Notifications
{
    #if UNITY_ANDROID
    public sealed class AndroidNotificationServices : BaseNotificationServices
    {
        const string PlayerPrefsIdsKey = "AndroidNotificationScheduledList";
        const string FullClassName = "es.socialpoint.unity.notification.NotificationBridge";
        const float PushTokenTimeout = 30.0f;
        List<int> _notifications = new List<int>();
        AndroidJavaClass _notifClass = null;
        IEnumerator _checkPermissionStatusCoroutine;

        public AndroidNotificationServices(ICoroutineRunner runner)
            : base(runner)
        {
#if !UNITY_EDITOR
            _notifClass = new AndroidJavaClass(FullClassName);
#endif
        }

        public override void Dispose()
        {
            base.Dispose();
            _runner.StopCoroutine(_checkPermissionStatusCoroutine);
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

        public override void RequestPermissions()
        {
            if(_notifClass != null)
            {
                if(_checkPermissionStatusCoroutine == null)
                {
                    _notifClass.CallStatic("registerForRemote");
                    _checkPermissionStatusCoroutine = _runner.StartCoroutine(CheckPermissionStatus());
                }
            }
        }

        IEnumerator CheckPermissionStatus()
        {
            if(_notifClass != null)
            {
                float startTime = Time.unscaledTime;
                float currentTime = startTime;
                string pushToken = null;
                string pushTokenError = null;
                while(string.IsNullOrEmpty(pushToken) && string.IsNullOrEmpty(pushTokenError) && (currentTime - startTime) < PushTokenTimeout)
                {
                    pushToken = _notifClass.CallStatic<string>("getNotificationToken");
                    pushTokenError = _notifClass.CallStatic<string>("getNotificationTokenError");
                    yield return new WaitForSeconds(1.0f);
                    currentTime = Time.unscaledTime;
                }

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
        }
    }
        
    #else
    public sealed class AndroidNotificationServices : EmptyNotificationServices
    {
    }

    #endif // UNITY_ANDROID
}
