using System;
using UnityEngine;
using System.Collections;
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

        public BaseNotificationServices(MonoBehaviour behaviour, ICommandQueue commandqueue = null)
        {
            if(behaviour == null)
            {
                throw new ArgumentNullException("behaviour", "behaviour cannot be null or empty!");
            }

            _behaviour = behaviour;
            _commandQueue = commandqueue;
        }

        protected void WaitForRemoteToken(PollPushNotificationToken pollDelegate)
        {
            _behaviour.StartCoroutine(CheckPushNotificationToken(pollDelegate));
        }

        IEnumerator CheckPushNotificationToken(PollPushNotificationToken pollDelegate)
        {
            while(_pushToken == null)
            {
                _pushToken = pollDelegate();
                yield return null;
            }
            SendPushToken(_pushToken);
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

        /**
         * Interface methods
         */
        public abstract void Schedule(Notification notif);

        public abstract void ClearReceived();

        public abstract void CancelPending();

        public abstract void RegisterForRemote();
    }
}