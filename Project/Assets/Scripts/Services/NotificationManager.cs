using Zenject;
using UnityEngine;
using System;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.Notifications;

public class NotificationManager : SocialPoint.Notifications.NotificationManager
{
    public NotificationManager(INotificationServices services, IAppEvents appEvents) :
        base(services, appEvents)
    {
    }

    override protected void AddGameNotifications()
    {
        var notify = new Notification(false);
        notify.Title = "Notification!";
        notify.Message = "This is a notification manager notification.";
        notify.FireDelay = 10;
        AddNotification(notify);
    }
}
