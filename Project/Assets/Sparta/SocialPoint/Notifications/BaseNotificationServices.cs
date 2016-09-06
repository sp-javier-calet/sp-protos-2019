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
        protected ICoroutineRunner _runner;
        protected string _pushToken;
        bool _validPushToken;

        IList<Action<bool, string>> _pushTokenReceivedListeners;

        protected BaseNotificationServices(ICoroutineRunner runner)
        {
            if(runner == null)
            {
                throw new ArgumentNullException("runner", "ICoroutineRunner cannot be null or empty!");
            }

            _runner = runner;
            _pushTokenReceivedListeners = new List<Action<bool, string>>();
        }

        public virtual void Dispose()
        {
            
        }

        protected void OnRequestPermissionsSuccess()
        {
            _validPushToken = true;
            NotifyPushTokenReceived();
        }

        protected void OnRequestPermissionsFail()
        {
            _validPushToken = false;
            NotifyPushTokenReceived();
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