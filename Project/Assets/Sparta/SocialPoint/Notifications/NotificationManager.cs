using System;
using UnityEngine;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

namespace SocialPoint.Notifications
{
    public abstract class NotificationManager : IDisposable
    {
        const string kPushTokenKey = "notifications_push_token";
        const string kPlayerAllowsNotificationKey = "player_allow_notification";

        public INotificationServices Services{ protected set; get; }

        protected IAppEvents _appEvents;
        protected ICommandQueue _commandQueue;

        List<Notification> _notifications = new List<Notification>();

        bool _gameLoaded;
        bool _pushTokenReceived;
        string _pushToken;

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
            _commandQueue = commandQueue;

#if UNITY_IOS && !UNITY_EDITOR
            Services = new IosNotificationServices(coroutineRunner);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Services = new AndroidNotificationServices(coroutineRunner);
#else
            Services = new EmptyNotificationServices();
#endif
            Init();
        }

        protected NotificationManager(INotificationServices services, IAppEvents appEvents, ICommandQueue commandQueue)
        {
            _appEvents = appEvents;
            _commandQueue = commandQueue;
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
            _gameLoaded = true;
            VerifyPushReady();
        }

        void OnPushTokenReceived(bool valid, string token)
        {
            _pushTokenReceived = true;
            _pushToken = token;
            VerifyPushReady();
        }

        void VerifyPushReady()
        {
            if(_gameLoaded && _pushTokenReceived)
            {
                SendPushToken();
            }
        }

        void SendPushToken()
        {
            if(_commandQueue == null || _pushToken == null)
            {
                return;
            }

            string currentPushToken = PlayerPrefs.GetString(kPushTokenKey);
            bool userAllowedNotifications = PlayerPrefs.GetInt(kPlayerAllowsNotificationKey, 0) != 0;

            bool pushTokenChanged = _pushToken != currentPushToken;
            bool allowNotificationsChanged = userAllowedNotifications != Services.UserAllowsNofitication;

            if(pushTokenChanged || allowNotificationsChanged)
            {
                string pushTokenToSend = Services.UserAllowsNofitication ? _pushToken : currentPushToken;
                if(string.IsNullOrEmpty(pushTokenToSend))
                {
                    return;
                }

                _commandQueue.Add(new PushEnabledCommand(pushTokenToSend, Services.UserAllowsNofitication), (data, err) => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        PlayerPrefs.SetString(kPushTokenKey, _pushToken);
                        PlayerPrefs.SetInt(kPlayerAllowsNotificationKey, Services.UserAllowsNofitication ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                });
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
