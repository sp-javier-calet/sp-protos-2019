using UnityEngine;
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using SocialPoint.Utils;

namespace SocialPoint.Notifications
{
    public class AdminPanelNotifications : IAdminPanelConfigurer, IAdminPanelGUI
    {
        private INotificationServices _services;
        AdminPanel.AdminPanel _adminPanel;

        public AdminPanelNotifications(INotificationServices services)
        {
            _services = services;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_services == null)
            {
                return;
            }
            _adminPanel = adminPanel;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Notifications", this));

            var cmd = new ConsoleCommand()
                .WithDescription("set a local notification")
                .WithOption(new ConsoleCommandOption("a|action")
                    .withDescription("action text to show under the notification"))
                .WithOption(new ConsoleCommandOption("b|body")
                    .withDescription("notification main text"))
                .WithOption(new ConsoleCommandOption("n|number")
                    .withDescription("number to show over the app icon"))
                .WithOption(new ConsoleCommandOption("t|time")
                    .withDescription("seconds after now when to show the notification"))
                .WithOption(new ConsoleCommandOption("r|repeat")
                    .withDescription("repeat notification after given seconds"))
                .WithDelegate(OnNotifyCommand);
            adminPanel.RegisterCommand("notify", cmd);
        }

        void OnNotifyCommand(ConsoleCommand cmd)
        {
            var notify = new Notification();
            notify.AlertAction = cmd["action"].Value;
            notify.AlertBody = cmd["body"].Value;
            notify.IconBadgeNumber = cmd["number"].IntValue;

            var ts = TimeUtils.GetTimestamp(DateTime.Now) + cmd["time"].IntValue;
            notify.FireDate = TimeUtils.GetDateTime(ts);
            notify.RepeatingSeconds = cmd["repeat"].IntValue;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Notification Services");

            layout.CreateButton("Clear Local Notifications", () => {
                _services.ClearLocalNotifications();
            });

            layout.CreateButton("Cancel Local Notifications", () => {
                _services.CancelAllLocalNotifications();
            });

            layout.CreateButton("Register Remote Notifications", () => {
                _services.RegisterForRemoteNotificationTypes();
            });

            layout.CreateButton("Unregister Remote Notifications", () => {
                _services.UnregisterForRemoteNotifications();
            });

            layout.CreateButton("Clear Remote Notifications", () => {
                _services.ClearRemoteNotifications();
            });

            layout.CreateButton("Register Local Notifications", () => {
                _services.RegisterForLocalNotificationTypes();
            });

            layout.CreateButton("Reset Badge Number", () => {
                _services.ResetIconBadgeNumber();
            });
            layout.CreateButton("Set Test Notification", () => {
                _adminPanel.Console.Print("A test notification should appear in 10 seconds.");
                var notif = new Notification();
                notif.AlertBody = "This is a test notification";
                notif.AlertAction = "Test Action";
                notif.IconBadgeNumber = 2;
                notif.FireDate = DateTime.Now.AddSeconds(10);
                _services.ScheduleLocalNotification(notif);
            });
        }
    }
}
