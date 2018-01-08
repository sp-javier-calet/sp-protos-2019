using System;

namespace SocialPoint.Notifications
{
    [Serializable]
    public struct NotificationChannel
    {
        public string Identifier;
        public string Name;
        public string Description;
    }

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

        /// <summary>
        /// Initializes the notification channels with the given configuration.
        /// </summary>
        /// <param name="channels">An array with configuration of the notification channels.</param>
        void SetupChannels(NotificationChannel[] channels);
    }
}
