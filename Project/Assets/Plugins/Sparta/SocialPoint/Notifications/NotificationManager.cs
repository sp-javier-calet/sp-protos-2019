using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.ServerSync;
using SocialPoint.Utils;
using UnityEngine;

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

        protected NotificationManager(ICoroutineRunner coroutineRunner, INativeUtils nativeUtils, IAppEvents appEvents, ICommandQueue commandQueue)
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
            Services = new IosNotificationServices(coroutineRunner, nativeUtils);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Services = new AndroidNotificationServices(coroutineRunner, nativeUtils);
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
            _appEvents.WasOnBackground.Add(0, ClearNotifications);
            _appEvents.WasCovered += ClearNotifications;
            Services.RegisterForRemoteToken(OnPushTokenReceived);
            Reset();

            DebugLog("Init");
        }

        virtual public void Dispose()
        {
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.WillGoBackground.Remove(ScheduleNotifications);
            _appEvents.ApplicationQuit -= ScheduleNotifications;
            _appEvents.WasOnBackground.Remove(ClearNotifications);
            _appEvents.WasCovered -= ClearNotifications;
        }

        protected virtual void AddGameNotifications()
        {
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
            DebugLog("OnPushTokenReceived\n\tvalid: " + valid + "\n\ttoken: " + token);
            _pushTokenReceived = true;
            _pushToken = token;
            VerifyPushReady();
        }

        void VerifyPushReady()
        {
            DebugLog("VerifyPushReady\n\tgameLoaded: " + _gameLoaded + "\n\tpushTokenReceived: " + _pushTokenReceived);
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
            bool userAllowsNotifications = Services.UserAllowsNofitication;

            bool pushTokenChanged = _pushToken != currentPushToken;
            bool allowNotificationsChanged = userAllowedNotifications != userAllowsNotifications;

            DebugLog("SendPushToken step1\n\tpushTokenChanged: " + pushTokenChanged + "\n\tallowNotificationsChanged: " + allowNotificationsChanged);

            if(pushTokenChanged || allowNotificationsChanged)
            {
                string pushTokenToSend = userAllowsNotifications ? _pushToken : currentPushToken;

                if(string.IsNullOrEmpty(pushTokenToSend))
                {
                    return;
                }

                DebugLog("SendPushToken step2\n\tpushToken Sent: " + pushTokenToSend);

                _commandQueue.Add(new PushEnabledCommand(pushTokenToSend, userAllowsNotifications), (data, err) => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        DebugLog("SendPushToken step3\n\tPushEnabledCommand ACK OK");
                        PlayerPrefs.SetString(kPushTokenKey, _pushToken);
                        PlayerPrefs.SetInt(kPlayerAllowsNotificationKey, userAllowsNotifications ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                });
            }
        }

        void ScheduleNotifications()
        {
            ClearNotifications();
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

        [System.Diagnostics.Conditional(DebugFlags.DebugNotificationsFlag)]
        void DebugLog(string msg)
        {
            const string tag = "SocialPoint.Notifications-DebugLog";
            Log.i(tag, msg);
        }

        #endregion
    }
}
