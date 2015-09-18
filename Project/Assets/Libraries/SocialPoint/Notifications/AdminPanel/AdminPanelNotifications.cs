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

            layout.CreateConfirmButton("Clear Local Notifications", () => {
                _services.ClearLocalNotifications();
            });

            layout.CreateConfirmButton("Cancel Local Notifications", () => {
                _services.CancelAllLocalNotifications();
            });

            layout.CreateConfirmButton("Register Remote Notifications", () => {
                _services.RegisterForRemoteNotificationTypes();
            });

            layout.CreateConfirmButton("Unregister Remote Notifications", () => {
                _services.UnregisterForRemoteNotifications();
            });

            layout.CreateConfirmButton("Clear Remote Notifications", () => {
                _services.ClearRemoteNotifications();
            });

            layout.CreateConfirmButton("Register Local Notifications", () => {
                _services.RegisterForLocalNotificationTypes();
            });

            layout.CreateConfirmButton("Reset Badge Number", () => {
                _services.ResetIconBadgeNumber();
            });
        }
    }
}
