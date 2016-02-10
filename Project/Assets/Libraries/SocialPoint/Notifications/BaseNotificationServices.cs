using System;
using System.Collections;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.ServerSync;
using UnityEngine;

namespace SocialPoint.Notifications
{
    public abstract class BaseNotificationServices : INotificationServices
    {
        protected delegate string PollPushNotificationToken();

        const string kPushTokenKey = "notifications_push_token";
        ICoroutineRunner _runner;
        ICommandQueue _commandQueue;
        string _pushToken = null;

        public BaseNotificationServices(ICoroutineRunner runner, ICommandQueue commandqueue = null)
        {
            if(runner == null)
            {
                throw new ArgumentNullException("runner", "ICoroutineRunner cannot be null!");
            }

            _runner = runner;
            _commandQueue = commandqueue;
        }

        protected void WaitForRemoteToken(PollPushNotificationToken pollDelegate)
        {
            _runner.StartCoroutine(CheckPushNotificationToken(pollDelegate));
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
            // TODO: pass a IAttrStorage instead of storing in PlayerPrefs
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