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

        public void RegisterForRemote(Action<string> onTokenReceivedCallback = null)
        {
        }

        public void RequestPushNotification()
        {
        }

        public void RequestLocalNotification()
        {
        }


    }
}
