using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.ServerSync;

namespace SocialPoint.Notifications
{
    public abstract class BaseNotificationServices : INotificationServices, IDisposable
    {
        const string kPushTokenKey = "notifications_push_token";
        const string kPlayerAllowsNotificationKey = "player_allow_notification";

        protected ICoroutineRunner _runner;
        protected string _pushToken;
        bool _validPushToken;

        ICommandQueue _commandQueue;
        IList<Action<bool, string>> _pushTokenReceivedListeners;

        protected BaseNotificationServices(ICoroutineRunner runner, ICommandQueue commandqueue = null)
        {
            if(runner == null)
            {
                throw new ArgumentNullException("runner", "ICoroutineRunner cannot be null or empty!");
            }

            _runner = runner;
            _commandQueue = commandqueue;
            _pushTokenReceivedListeners = new List<Action<bool, string>>();
        }

        public virtual void Dispose()
        {
            
        }

        protected void OnRequestPermissionsSuccess()
        {
            _validPushToken = true;
            SendPushToken();
            NotifyPushTokenReceived();
        }

        protected void OnRequestPermissionsFail()
        {
            _validPushToken = false;
            SendPushToken();
            NotifyPushTokenReceived();
        }

        void SendPushToken()
        {
            if(_commandQueue == null || _pushToken == null)
            {
                return;
            }

            // This if needs to be removed when PushEnabledCommand supports notifying about disabled notifications
            if(string.IsNullOrEmpty(_pushToken))
            {
                return;
            }

            string currentPushToken = PlayerPrefs.GetString(kPushTokenKey);
            bool userAllowedNotifications = PlayerPrefs.GetInt(kPlayerAllowsNotificationKey, 0) != 0;

            bool pushTokenChanged = _pushToken != currentPushToken;
            bool allowNotificationsChanged = userAllowedNotifications != UserAllowsNofitication;

            if(pushTokenChanged || allowNotificationsChanged)
            {
                _commandQueue.Add(new PushEnabledCommand(_pushToken), (data, err) => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        PlayerPrefs.SetString(kPushTokenKey, _pushToken);
                        PlayerPrefs.SetInt(kPlayerAllowsNotificationKey, UserAllowsNofitication ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                });
            }
        }

        void NotifyPushTokenReceived()
        {
            for(int i = 0; i < _pushTokenReceivedListeners.Count; ++i)
            {
                var listener = _pushTokenReceivedListeners[i];
                if(listener != null)
                {
                    listener(_validPushToken, _pushToken);
                }
            }
            _pushTokenReceivedListeners.Clear();
        }

        public void RegisterForRemoteToken(Action<bool, string> callback)
        {
            if(callback != null)
            {
                if(_pushToken != null)
                {
                    callback(_validPushToken, _pushToken);
                }
                else
                {
                    _pushTokenReceivedListeners.Add(callback);
                }
            }
        }

        public bool UserAllowsNofitication
        {
            get
            {
                return NativeUtils.UserAllowNotification;
            }
        }

        #region INotificationServices implementation

        public abstract void Schedule(Notification notif);

        public abstract void ClearReceived();

        public abstract void CancelPending();

        public abstract void RequestPermissions();

        #endregion
    }
}