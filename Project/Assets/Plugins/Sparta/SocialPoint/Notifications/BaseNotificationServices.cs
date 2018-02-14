using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Notifications
{
    public abstract class BaseNotificationServices : INotificationServices, IDisposable
    {
        protected ICoroutineRunner _runner;
        protected INativeUtils _nativeUtils;
        protected string _pushToken;
        bool _validPushToken;

        IList<Action<bool, string>> _pushTokenReceivedListeners;

        protected BaseNotificationServices(ICoroutineRunner runner, INativeUtils nativeUtils)
        {
            DebugUtils.Assert(runner != null);
            DebugUtils.Assert(nativeUtils != null);

            _runner = runner;
            _nativeUtils = nativeUtils;
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
                return _nativeUtils.UserAllowNotification;
            }
        }

        #region INotificationServices implementation

        public abstract void Schedule(Notification notif);

        public abstract void ClearReceived();

        public abstract void CancelPending();

        public abstract void RequestPermissions();

        public abstract void SetupChannels(NotificationChannel[] channels);

        #endregion
    }
}