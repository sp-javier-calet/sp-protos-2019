using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Notifications
{
    public interface INotificationServices
    {
        /**
         * Schedules a local notification
         */
        void ScheduleLocalNotification(Notification notif);
        
        /**
         * Discards of all received local notifications
         */
        void ClearLocalNotifications();
        
        /**
         * Cancels the delivery of all scheduled local notifications
         */
        void CancelAllLocalNotifications();
        
        /**
         * Register to receive remote notifications of the specified types from a provider
         */
        void RegisterForRemoteNotificationTypes();
        
        /**
         * Unregister for remote notifications
         */
        void UnregisterForRemoteNotifications();
        
        /**
         * Discards of all received remote notifications
         */
        void ClearRemoteNotifications();
        
        /**
         * Register to receive remote notifications of the specified types from a provider
         */
        void RegisterForLocalNotificationTypes();

        /**
         * Remove Number on the Icon Badge Number
         */
        void ResetIconBadgeNumber();

    }
   
}
