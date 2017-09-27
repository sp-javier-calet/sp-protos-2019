namespace SocialPoint.Utils
{
    public struct ShortcutItem
    {
        public string Type;
        public string Title;
        public string Subtitle;
        public string Icon;
    }

    public interface INativeUtils
    {
        /// <summary>
        /// Determines whether a specific app is installed
        /// </summary>
        /// <returns><c>true</c> if app is installed the specified appId; otherwise, <c>false</c>.</returns>
        /// <param name="appId">App identifier.</param>
        bool IsInstalled(string appId);

        /// <summary>
        /// Opens the app.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        void OpenApp(string appId);

        /// <summary>
        /// Opens the store page of the app.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        void OpenStore(string appId);

        /// <summary>
        /// Opens the upgrade page of the current app.
        /// </summary>
        void OpenUpgrade();

        /// <summary>
        /// Opens the review page for the current app.
        /// </summary>
        void OpenReview();

        /// <summary>
        /// Return true if the native platform supports showing review dialogs
        /// </summary>
        bool SupportsReviewDialog { get; }

        /// <summary>
        /// Tries to open the native review dialog
        /// </summary>
        void DisplayReviewDialog();

        /// <summary>
        /// Checks if the user is allowing notifications
        /// </summary>
        /// <value><c>true</c> if user allows notification; otherwise, <c>false</c>.</value>
        bool UserAllowNotification { get; }

        /// <summary>
        /// Sets the shortcut items.
        /// </summary>
        /// <value>The shortcut items.</value>
        ShortcutItem[] ShortcutItems{ get; set; }
    }
}