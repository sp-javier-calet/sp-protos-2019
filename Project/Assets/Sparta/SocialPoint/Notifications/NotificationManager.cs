using UnityEngine;
using System;
using System.Collections.Generic;

using SocialPoint.Utils;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;

namespace SocialPoint.Notifications
{
    public abstract class NotificationManager : IDisposable
    {
        public INotificationServices Services{ protected set; get; }

        protected IAppEvents _appEvents;

        List<Notification> _notifications = new List<Notification>();

        public NotificationManager(ICoroutineRunner coroutineRunner, IAppEvents appEvents, ICommandQueue commandQueue)
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
            _appEvents = appEvents;
            Services = services;
            Init();
        }

        protected void Init()
        {
            if(Services == null)
            {
                throw new ArgumentNullException("services", "services cannot be null or empty!");
            }
            if(_appEvents == null)
            {
                throw new ArgumentNullException("appEvents", "appEvents cannot be null or empty!");
            }
            _appEvents.WillGoBackground.Add(-50, ScheduleNotifications);
            _appEvents.ApplicationQuit += ScheduleNotifications;
            _appEvents.WasOnBackground += ClearNotifications;
            _appEvents.WasCovered += ClearNotifications;
            Reset();
        }

        virtual public void Dispose()
        {
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
            AddNotification(action, message, TimeUtils.ToDateTime(timeStamp), numBadge);
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

        void ScheduleNotifications()
        {
            AddGameNotifications();
            foreach(var notif in _notifications)
            {
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
