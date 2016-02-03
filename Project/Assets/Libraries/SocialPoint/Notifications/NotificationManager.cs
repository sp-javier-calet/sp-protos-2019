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
        public INotificationServices Services{ private set; get; }

        /// <summary>
        /// Max offset in seconds to apply to notifications' fire date if they require it.
        /// It should be assigned with a default value greater than 0.
        /// Each game must be responsible of setting it.
        /// WARNING: Take into account each notification event. Avoid cases when the offset may schedule the notification after it stops being relevant.
        /// </summary>
        public int MaxNotificationOffset { get; set; }

        IAppEvents _appEvents;
        List<Notification> _notifications = new List<Notification>();

        public NotificationManager(MonoBehaviour behaviour, IAppEvents appEvents, ICommandQueue commandQueue)
        {
            if(behaviour == null)
            {
                throw new ArgumentNullException("behaviour", "behaviour cannot be null or empty!");
            }
            if(appEvents == null)
            {
                throw new ArgumentNullException("appEvents", "appEvents cannot be null or empty!");
            }
            _appEvents = appEvents;

#if UNITY_IOS && !UNITY_EDITOR
            Services = new IosNotificationServices(behaviour, commandQueue);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Services = new AndroidNotificationServices(behaviour, commandQueue);
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

        void Init()
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
            var ln = new Notification(false);
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
            Services.CancelPending();
            Services.ClearReceived();
        }

        #region App Events

        void ScheduleNotifications()
        {
            AddGameNotifications();
            foreach(var notif in _notifications)
            {
                //If notification must use a time offset, change its fire delay
                if(notif.RequiresOffset)
                {
                    int randomOffset = UnityEngine.Random.Range(0, MaxNotificationOffset + 1);//Second param is exclusive for ints, adding 1 to include it 
                    notif.FireDelay += randomOffset;//Offset must be added only, to avoid scheduling notifications before the actual event happens
                }

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
        protected LocalNotificationManager(MonoBehaviour behaviour, IAppEvents appEvents, ICommandQueue commandQueue) :
            base(behaviour, appEvents, commandQueue)
        {
        }
    }
}
