using UnityEngine;
using UnityEngine.UI;
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using SocialPoint.Utils;

namespace SocialPoint.Notifications
{
    public class AdminPanelNotifications : IAdminPanelConfigurer, IAdminPanelGUI
    {
        private INotificationServices _services;
        private AdminPanel.AdminPanel _adminPanel;
        private InputField _messageInput;
        private InputField _actionInput;
        private InputField _secondsInput;

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

            layout.CreateButton("Clear Received", () => {
                _services.ClearReceived();
            });

            layout.CreateButton("Cancel Pending", () => {
                _services.CancelPending();
            });

            layout.CreateButton("Register For Remote", () => {
                _services.RegisterForRemote();
            });

            layout.CreateMargin();

            layout.CreateLabel("Test Notification");

            var flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Message");
            _messageInput = flayout.CreateTextInput("This is a test notification.");
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Action");
            _actionInput = flayout.CreateTextInput("Test Action");
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Fire seconds");
            _secondsInput = flayout.CreateTextInput("10");

            layout.CreateButton("Set Notification", () => {
                var notif = new Notification();
                notif.AlertBody = _messageInput.text;
                notif.AlertAction = _actionInput.text;
                notif.IconBadgeNumber = 1;
                int secs = 10;
                int.TryParse(_secondsInput.text, out secs);
                notif.FireDate = DateTime.Now.AddSeconds(secs);
                _services.Schedule(notif);
                _adminPanel.Console.Print(string.Format("A test notification should appear in {0} seconds.", secs));
            });
        }
    }
}
