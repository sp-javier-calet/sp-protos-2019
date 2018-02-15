#if ADMIN_PANEL 

using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;

namespace SocialPoint.Notifications
{
    public sealed class AdminPanelNotifications : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly INotificationServices _services;
        InputField _messageInput;
        InputField _actionInput;
        InputField _secondsInput;
        InputField _channelInput;
        AdminPanelConsole _console;

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
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Notifications", this));

            var cmd = new ConsoleCommand()
                .WithDescription("set a local notification")
                .WithOption(new ConsoleCommandOption("t|title")
                    .WithDescription("title of the notification"))
                .WithOption(new ConsoleCommandOption("m|message")
                    .WithDescription("message of the notification"))
                .WithOption(new ConsoleCommandOption("d|delay")
                    .WithDescription("seconds after now when to show"))
                .WithOption(new ConsoleCommandOption("c|channel")
                    .WithDescription("identifier of the notifications channel"))
                .WithDelegate(OnNotifyCommand);
            adminPanel.RegisterCommand("notify", cmd);
        }

        void OnNotifyCommand(ConsoleCommand cmd)
        {
            var notify = new Notification(cmd["delay"].IntValue, Notification.OffsetType.None);
            notify.Title = cmd["title"].Value;
            notify.Message = cmd["message"].Value;
            notify.ChannelID = cmd["channel"].Value;
        }

        void ConsolePrint(string msg)
        {
            if(_console != null)
            {
                _console.Print(msg);
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Notification Services");

            layout.CreateLabel("User allows notifications: " + _services.UserAllowsNofitication);

            layout.CreateButton("Request Permissions", _services.RequestPermissions);

            layout.CreateButton("Clear Received", _services.ClearReceived);

            layout.CreateButton("Cancel Pending", _services.CancelPending);

            layout.CreateButton("Get Push Token", () => _services.RegisterForRemoteToken((isTokenValid, token) => {
                string msg;
                if(isTokenValid)
                {
                    msg = string.Format("Retrieved push token: {0}", token);
                }
                else
                {
                    msg = "Cannot retrieve push token (permission denied).";
                }
                ConsolePrint(msg);
                layout.Refresh();
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
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Channel ID");
            _channelInput = flayout.CreateTextInput();

            layout.CreateButton("Set Notification", () => {
                int secs = 0;
                int.TryParse(_secondsInput.text, out secs);
                var notif = new Notification(secs, Notification.OffsetType.None);
                notif.Message = _messageInput.text;
                notif.Title = _actionInput.text;
                notif.ChannelID = _channelInput.text;
                _services.Schedule(notif);
                ConsolePrint(string.Format("A test notification should appear in {0} seconds.", secs));
            });
        }
    }
}

#endif
