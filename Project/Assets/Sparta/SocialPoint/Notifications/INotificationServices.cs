using System;

namespace SocialPoint.Notifications
{
    public interface INotificationServices
    {
        /// <summary>
        /// Schedules a local notification
        /// </summary>
        /// <param name="notif">Notification to schedule</param>
        void Schedule(Notification notif);

        /// <summary>
        /// Discards of all received local notifications
        /// </summary>
        void ClearReceived();

        /// <summary>
        /// Cancels the delivery of all scheduled local notifications
        /// </summary>
        void CancelPending();

        /// <summary>
        /// Register to receive remote notifications of the specified types from a provider
        /// </summary>
        /// <param name="onTokenReceivedCallback">On token received callback.</param>
        void RegisterForRemote(Action<string> onTokenReceivedCallback = null);
    }
}
