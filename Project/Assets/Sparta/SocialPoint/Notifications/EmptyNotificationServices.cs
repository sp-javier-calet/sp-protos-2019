using System;

namespace SocialPoint.Notifications
{
    public class EmptyNotificationServices : INotificationServices
    {
        public void Schedule(Notification notif)
        {
        }

        public void ClearReceived()
        {
        }

        public void CancelPending()
        {
        }

        public void RegisterForRemoteToken(Action<bool, string> callback)
        {
            if(callback != null)
            {
                callback(false, "");
            }
        }

        public void RequestPermissions()
        {
        }

        public bool UserAllowsNofitication
        {
            get
            {
                return false;
            }
        }
    }
}
