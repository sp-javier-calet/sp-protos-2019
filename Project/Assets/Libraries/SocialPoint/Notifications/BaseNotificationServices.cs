using System;
using UnityEngine;
using System.Collections;
using SocialPoint.ServerSync;

namespace SocialPoint.Notifications
{
    public abstract class BaseNotificationServices : INotificationServices {

        protected delegate string PollPushNotificationToken();

        private const string kPushTokenKey = "notifications_push_token";
        private MonoBehaviour _behaviour;
        private ICommandQueue _commandQueue;
        private string _pushToken = null;

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

        private IEnumerator CheckPushNotificationToken(PollPushNotificationToken pollDelegate)
        {
            while(_pushToken == null)
            {
                _pushToken = pollDelegate();
                yield return null;
            }
            SendPushToken(_pushToken);
        }
        
        private void SendPushToken(string pushToken)
        {
            string currentPushToken = PlayerPrefs.GetString(kPushTokenKey);
            if(_commandQueue != null && !string.IsNullOrEmpty(pushToken) && pushToken != currentPushToken)
            {
                _commandQueue.Add(new PushEnabledCommand(pushToken), () => {
                    PlayerPrefs.SetString(kPushTokenKey, pushToken);
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