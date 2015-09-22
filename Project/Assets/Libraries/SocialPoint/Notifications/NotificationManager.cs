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

        private IAppEvents _appEvents;
        private List<Notification> _notifications = new List<Notification>();


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
            Services = new AndroidNotificationServices();
#else
            Services = new EmptyNotificationServices();
#endif
            Init();
        }

        public NotificationManager(INotificationServices services, IAppEvents appEvents)
        {
            _appEvents = appEvents;
            Services = services;
            Init();
        }

        private void Init()
        {
            if(Services == null)
            {
                throw new ArgumentNullException("services", "services cannot be null or empty!");
            }
            if(_appEvents == null)
            {
                throw new ArgumentNullException("appEvents", "appEvents cannot be null or empty!");
            }
            _appEvents.WillGoBackground += OnGoToBackground;
            _appEvents.WasOnBackground += OnComeFromBackground;
            Reset();
        }

        public void Dispose()
        {
            _appEvents.WillGoBackground -= OnGoToBackground;
            _appEvents.WasOnBackground -= OnComeFromBackground;
        }

        protected virtual void AddGameNotifications()
        {
        }

        [Obsolete("Use AddNotification(Notification notification)")]
        protected void AddNotification(string action, string message, DateTime dateTime, int numBadge = 0)
        {
            var ln = new Notification();
            ln.AlertAction = action;
            ln.FireDate = dateTime;
            ln.AlertBody = message;
            //I'm using this to know if it has to show a +1 on the badge, but I really set up it later
            ln.IconBadgeNumber = numBadge;
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

        private void Reset()
        {
            Services.CancelPending();
            Services.ClearReceived();
        }

        #region App Events

        private void OnGoToBackground()
        {
            AddGameNotifications();
            int numBadge = 0;
            foreach(var notif in _notifications)
            {
                //Using the previous seted Number, I know wether I have to add a +1 to this notification or not
                numBadge += notif.IconBadgeNumber;
                notif.IconBadgeNumber = numBadge;
                Services.Schedule(notif);
            }
            _notifications.Clear();
        }

        private void OnComeFromBackground()
        {
            Reset();
        }

        #endregion
    }

    [Obsolete("Use NotificationManager instead")]
    abstract class LocalNotificationManager : NotificationManager
    {
        public LocalNotificationManager(MonoBehaviour behaviour, IAppEvents appEvents, ICommandQueue commandQueue):
            base(behaviour, appEvents, commandQueue)
        {
        }
    }
}
