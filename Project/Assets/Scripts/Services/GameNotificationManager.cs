using SocialPoint.AppEvents;
using SocialPoint.Notifications;
using SocialPoint.ServerSync;

public class GameNotificationManager : NotificationManager
{
    public GameNotificationManager(INotificationServices services, IAppEvents appEvents, ICommandQueue commandQueue) :
        base(services, appEvents, commandQueue)
    {
    }

    override protected void AddGameNotifications()
    {
        var notify = new Notification(10, Notification.OffsetType.None);
        notify.Title = "Notification!";
        notify.Message = "This is a notification manager notification.";
        AddNotification(notify);
    }
}
