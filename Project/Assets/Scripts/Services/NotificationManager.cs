using Zenject;
using UnityEngine;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;

public class NotificationManager : SocialPoint.Notifications.NotificationManager
{
    public NotificationManager(MonoBehaviour behaviour, IAppEvents appEvents, ICommandQueue commandQueue):
        base(behaviour, appEvents, commandQueue)
    {
    }

    override protected void AddGameNotifications()
    {
    }
}
