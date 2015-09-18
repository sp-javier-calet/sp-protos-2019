using UnityEngine;
using System.Collections;

namespace SocialPoint.Notifications
{
    public class EmptyNotificationServices : INotificationServices
    {

        public void ScheduleLocalNotification(Notification notif)
        {
        }
        
        public void ClearLocalNotifications()
        {
        }

        public void CancelAllLocalNotifications()
        {
        }

        public void RegisterForRemoteNotificationTypes()
        {
        }

        public void UnregisterForRemoteNotifications()
        {
        }
        
        public void ClearRemoteNotifications()
        {
        }
        
        public void RegisterForLocalNotificationTypes()
        {
        }
        
        public void ResetIconBadgeNumber()
        {
        }

    }
}
