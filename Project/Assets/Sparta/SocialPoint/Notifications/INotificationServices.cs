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
        /// Requests permissions for local and remote notifications
        /// </summary>
        void RequestPermissions();

        /// <summary>
        /// Register to receive remote notifications of the specified types from a provider
        /// </summary>
        /// <param name="callback">On token received callback.</param>
        void RegisterForRemoteToken(Action<bool, string> callback);

        /// <summary>
        /// Gets a value indicating whether this <see cref="SocialPoint.Notifications.INotificationServices"/> user
        /// allows nofitication.
        /// </summary>
        /// <value><c>true</c> if user allows nofitication; otherwise, <c>false</c>.</value>
        bool UserAllowsNofitication { get; }
    }
}
