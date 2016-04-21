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
        protected delegate string PollPushNotificationToken();

        const string kPushTokenKey = "notifications_push_token";
        const string kPlayerAllowsNotificationKey = "player_allow_notification";
        ICoroutineRunner _runner;
        ICommandQueue _commandQueue;

        string _pushToken = null;
        IEnumerator _checkPushTokenCoroutine;
        readonly IList<Action<string>> _pushTokenReceivedListeners;
        bool _requestPushNotificationAutomatically;

        protected BaseNotificationServices(ICoroutineRunner runner, ICommandQueue commandqueue = null, bool requestPushNotificationAutomatically = true)
        {
            if(runner == null)
            {
                throw new ArgumentNullException("runner", "ICoroutineRunner cannot be null or empty!");
            }

            _runner = runner;
            _commandQueue = commandqueue;
            _requestPushNotificationAutomatically = requestPushNotificationAutomatically;
            _pushTokenReceivedListeners = new List<Action<string>>();
        }

        public void Dispose()
        {
            _runner.StopCoroutine(_checkPushTokenCoroutine);
        }

        protected void WaitForRemoteToken(PollPushNotificationToken pollDelegate)
        {
            _checkPushTokenCoroutine = CheckPushNotificationToken(pollDelegate);
            _runner.StartCoroutine(_checkPushTokenCoroutine);
        }

        IEnumerator CheckPushNotificationToken(PollPushNotificationToken pollDelegate)
        {
            while(_pushToken == null)
            {
                _pushToken = pollDelegate();
                yield return null;
            }
            SendPushToken(_pushToken);
            NotifyPushTokenReceived(_pushToken);
        }

        void SendPushToken(string pushToken)
        {
            string currentPushToken = PlayerPrefs.GetString(kPushTokenKey);
            bool userAllowedNotifications = PlayerPrefs.GetInt(kPlayerAllowsNotificationKey, 0) != 0;
            if(_commandQueue != null && !string.IsNullOrEmpty(pushToken) && (pushToken != currentPushToken || userAllowedNotifications != UserAllowsNofitication))
            {
                //*** TEST
                /*_commandQueue.Add(new PushEnabledCommand(pushToken), (data, err) => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        PlayerPrefs.SetString(kPushTokenKey, pushToken);
                        PlayerPrefs.SetInt(kPlayerAllowsNotificationKey, UserAllowsNofitication ? 1 : 0);
                        PlayerPrefs.Save();
                    }
                });*/
            }
        }

        void NotifyPushTokenReceived(string token)
        {
            foreach(var cbk in _pushTokenReceivedListeners)
            {
                cbk(token);
            }
            _pushTokenReceivedListeners.Clear();
        }

        public void RegisterForRemote(Action<string> onTokenReceivedCallback = null)
        {
            if(onTokenReceivedCallback != null)
            {
                if(_pushToken != null)
                {
                    onTokenReceivedCallback(_pushToken);
                }
                else
                {
                    _pushTokenReceivedListeners.Add(onTokenReceivedCallback);
                }
            }

            if(_requestPushNotificationAutomatically)
            {
                RequestPushNotification();
            }
        }

        public void RequestPushNotification()
        {
            // Start registering proccess if it is not already running
            if(_checkPushTokenCoroutine == null)
            {
                RequestPushNotificationToken();
            }
        }

        protected abstract void RequestPushNotificationToken();


        #region INotificationServices implementation

        public abstract void Schedule(Notification notif);

        public abstract void ClearReceived();

        public abstract void CancelPending();

        public abstract void RequestLocalNotification();

        public abstract bool UserAllowsNofitication{ get; }

        #endregion
    }
}