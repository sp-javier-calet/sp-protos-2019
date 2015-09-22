using UnityEngine;
using System.Collections;

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

        public void RegisterForRemote()
        {
        }

    }
}
