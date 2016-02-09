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
        /// Default max offset in seconds to apply to notifications' fire date if they require it.
        /// It would be initialized with a default value greater than 0 (Check Init method). Any game can override this value through the property's setter.
        /// Each notification can override this value for itself if a special case is needed. (Use the notification's setters for this)
        /// </summary>
        public int MaxNotificationOffset
        { 
            get; 
            set;
        }

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

            MaxNotificationOffset = 7200;//Default value of 2 hours

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
                    //Select max offset between default or notification's own desired value if any
                    int maxOffset = notif.MaxDesiredOffset > 0 ? notif.MaxDesiredOffset : MaxNotificationOffset;
                    int randomOffset = RandomUtils.Range(0, maxOffset + 1);//Second param is exclusive for ints, adding 1 to include it 
                    notif.ApplyOffset(randomOffset);
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
