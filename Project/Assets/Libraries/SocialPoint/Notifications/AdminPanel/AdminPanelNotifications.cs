using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;

namespace SocialPoint.Notifications
{
    public class AdminPanelNotifications : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly INotificationServices _services;
        AdminPanel.AdminPanel _adminPanel;
        InputField _messageInput;
        InputField _actionInput;
        InputField _secondsInput;

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
            var notify = new Notification(cmd["delay"].IntValue, Notification.OffsetType.None);
            notify.Title = cmd["title"].Value;
            notify.Message = cmd["message"].Value;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Notification Services");

            layout.CreateButton("Clear Received", _services.ClearReceived);

            layout.CreateButton("Cancel Pending", _services.CancelPending);

            layout.CreateButton("Register For Remote", () => _services.RegisterForRemote(token => { 
                var msg = string.Format("Retrieved push token: {0}", token);
                layout.AdminPanel.Console.Print(msg);
            }));

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
                int secs = 0;
                int.TryParse(_secondsInput.text, out secs);
                var notif = new Notification(secs, Notification.OffsetType.None);
                notif.Message = _messageInput.text;
                notif.Title = _actionInput.text;
                _services.Schedule(notif);
                _adminPanel.Console.Print(string.Format("A test notification should appear in {0} seconds.", secs));
            });
        }
    }
}
