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
                .WithOption(new ConsoleCommandOption("t|title")
                    .withDescription("title of the notification"))
                .WithOption(new ConsoleCommandOption("m|message")
                    .withDescription("message of the notification"))
                .WithOption(new ConsoleCommandOption("d|delay")
                    .withDescription("seconds after now when to show"))
                .WithDelegate(OnNotifyCommand);
            adminPanel.RegisterCommand("notify", cmd);
        }

        void OnNotifyCommand(ConsoleCommand cmd)
        {
            var notify = new Notification(false);
            notify.Title = cmd["title"].Value;
            notify.Message = cmd["message"].Value;
            notify.FireDelay = cmd["delay"].IntValue;
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
            _messageInput = flayout.CreateTextInput();
            _messageInput.text = "This is a test notification.";
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Action");
            _actionInput = flayout.CreateTextInput();
            _actionInput.text = "Test Action";
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Fire seconds");
            _secondsInput = flayout.CreateTextInput();
            _secondsInput.text = "2";

            layout.CreateButton("Set Notification", () => {
                var notif = new Notification(false);
                notif.Message = _messageInput.text;
                notif.Title = _actionInput.text;
                int secs = 0;
                int.TryParse(_secondsInput.text, out secs);
                notif.FireDelay = secs;
                _services.Schedule(notif);
                _adminPanel.Console.Print(string.Format("A test notification should appear in {0} seconds.", secs));
            });
        }
    }
}
