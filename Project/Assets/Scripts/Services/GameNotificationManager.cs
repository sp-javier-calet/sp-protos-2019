using SocialPoint.AppEvents;
using SocialPoint.Notifications;
using SocialPoint.ServerSync;
using SocialPoint.Login;

public class GameNotificationManager : NotificationManager
{
    public GameNotificationManager(INotificationServices services, IAppEvents appEvents, ICommandQueue commandQueue, IUserService userService) :
        base(services, appEvents, commandQueue, userService)
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
