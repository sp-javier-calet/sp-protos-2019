using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.ServerSync;

namespace SocialPoint.Notifications
{
    public abstract class BaseNotificationServices : INotificationServices
    {
        protected delegate string PollPushNotificationToken();

        const string kPushTokenKey = "notifications_push_token";
        MonoBehaviour _behaviour;
        ICommandQueue _commandQueue;

        string _pushToken = null;
        Coroutine _checkPushTokenCoroutine;
        readonly IList<Action<string>> _pushTokenReceivedListeners;


        public BaseNotificationServices(MonoBehaviour behaviour, ICommandQueue commandqueue = null)
        {
            if(behaviour == null)
            {
                throw new ArgumentNullException("behaviour", "behaviour cannot be null or empty!");
            }

            _behaviour = behaviour;
            _commandQueue = commandqueue;
            _pushTokenReceivedListeners = new List<Action<string>>();
        }

        protected void WaitForRemoteToken(PollPushNotificationToken pollDelegate)
        {
            _checkPushTokenCoroutine = _behaviour.StartCoroutine(CheckPushNotificationToken(pollDelegate));
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
            if(_commandQueue != null && !string.IsNullOrEmpty(pushToken) && pushToken != currentPushToken)
            {
                _commandQueue.Add(new PushEnabledCommand(pushToken), err => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        PlayerPrefs.SetString(kPushTokenKey, pushToken);
                    }
                });
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

        #endregion
    }
}