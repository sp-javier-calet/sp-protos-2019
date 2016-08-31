using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

namespace SocialPoint.Notifications
{
    public abstract class NotificationManager : IDisposable
    {
        public INotificationServices Services{ protected set; get; }

        protected IAppEvents _appEvents;

        List<Notification> _notifications = new List<Notification>();

        bool _gameLoaded;
        bool _pushTokenReceived;

        protected NotificationManager(ICoroutineRunner coroutineRunner, IAppEvents appEvents, ICommandQueue commandQueue)
        {
            if(coroutineRunner == null)
            {
                throw new ArgumentNullException("coroutineRunner", "coroutineRunner cannot be null or empty!");
            }
            if(appEvents == null)
            {
                throw new ArgumentNullException("appEvents", "appEvents cannot be null or empty!");
            }
            _appEvents = appEvents;

#if UNITY_IOS && !UNITY_EDITOR
            Services = new IosNotificationServices(coroutineRunner, commandQueue);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Services = new AndroidNotificationServices(coroutineRunner, commandQueue);
#else
            Services = new EmptyNotificationServices();
#endif
            Init();
        }

        protected NotificationManager(INotificationServices services, IAppEvents appEvents)
        {
            UnityEngine.Debug.Log("*** TEST NotificationManager Constructor");
            _appEvents = appEvents;
            Services = services;
            Init();
        }

        protected void Init()
        {
            UnityEngine.Debug.Log("*** TEST NotificationManager Init");
            if(Services == null)
            {
                throw new ArgumentNullException("services", "services cannot be null or empty!");
            }
            if(_appEvents == null)
            {
                throw new ArgumentNullException("appEvents", "appEvents cannot be null or empty!");
            }
            UnityEngine.Debug.Log("*** TEST NotificationManager Init OK");
            _appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
            _appEvents.WillGoBackground.Add(-50, ScheduleNotifications);
            _appEvents.ApplicationQuit += ScheduleNotifications;
            _appEvents.WasOnBackground += ClearNotifications;
            _appEvents.WasCovered += ClearNotifications;
            Services.RegisterForRemoteToken(OnPushTokenReceived);
            Reset();
        }

        virtual public void Dispose()
        {
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.WillGoBackground.Remove(ScheduleNotifications);
            _appEvents.ApplicationQuit -= ScheduleNotifications;
            _appEvents.WasOnBackground -= ClearNotifications;
            _appEvents.WasCovered -= ClearNotifications;
        }

        protected virtual void AddGameNotifications()
        {
        }

        [Obsolete("Use AddNotification(Notification notification)")]
        protected void AddNotification(string action, string message, DateTime dateTime, int numBadge = 0)
        {
            var ln = new Notification(0, Notification.OffsetType.None);
            ln.Title = action;
            ln.Message = message;
            ln.FireDate = dateTime;
        }

        [Obsolete("Use AddNotification(Notification notification)")]
        protected void AddNotification(string action, string message, long timeStamp, int numBadge = 0)
        {
            AddNotification(action, message, timeStamp.ToDateTime(), numBadge);
        }

        protected void AddNotification(Notification notification)
        {
            _notifications.Add(notification);
        }

        void Reset()
        {
            if(Services == null)
            {
                return;
            }
            
            Services.CancelPending();
            Services.ClearReceived();
        }

        #region App Events

        void OnGameWasLoaded()
        {
            UnityEngine.Debug.Log("*** TEST NotificationManager OnGameWasLoaded");
            _gameLoaded = true;
            VerifyPushReady();
        }

        void OnPushTokenReceived(bool valid, string token)
        {
            UnityEngine.Debug.Log("*** TEST NotificationManager OnPushTokenReceived");
            _pushTokenReceived = true;
            VerifyPushReady();
        }

        void VerifyPushReady()
        {
            if(_gameLoaded && _pushTokenReceived)
            {
                Services.SendPushToken();
            }
        }

        void ScheduleNotifications()
        {
            AddGameNotifications();
            for(int i = 0, _notificationsCount = _notifications.Count; i < _notificationsCount; i++)
            {
                var notif = _notifications[i];
                Services.Schedule(notif);
            }
            _notifications.Clear();
        }

        void ClearNotifications()
        {
            Reset();
        }

        #endregion
    }

    [Obsolete("Use NotificationManager instead")]
    abstract class LocalNotificationManager : NotificationManager
    {
        protected LocalNotificationManager(ICoroutineRunner coroutineRunner, IAppEvents appEvents, ICommandQueue commandQueue) :
            base(coroutineRunner, appEvents, commandQueue)
        {
        }
    }
}
