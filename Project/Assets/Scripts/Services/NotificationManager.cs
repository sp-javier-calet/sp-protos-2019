using Zenject;
using UnityEngine;
using System;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.Notifications;

public class NotificationManager : SocialPoint.Notifications.NotificationManager
{
    public NotificationManager(MonoBehaviour behaviour, IAppEvents appEvents, ICommandQueue commandQueue):
        base(behaviour, appEvents, commandQueue)
    {
    }

    override protected void AddGameNotifications()
    {
        var notify = new Notification();
        notify.AlertBody = "This is a notification manager notification.";
        notify.FireDelay = 10;
        AddNotification(notify);
    }
}
