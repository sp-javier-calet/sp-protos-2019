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
        void Schedule(Notification notif);
        
        /**
         * Discards of all received local notifications
         */
        void ClearReceived();
        
        /**
         * Cancels the delivery of all scheduled local notifications
         */
        void CancelPending();
        
        /**
         * Register to receive remote notifications of the specified types from a provider
         */
        void RegisterForRemote();

    }
   
}
