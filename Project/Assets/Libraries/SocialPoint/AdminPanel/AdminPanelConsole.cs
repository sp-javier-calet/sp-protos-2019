using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole {

        public string Content { get; private set; }

        public event Action OnContentChanged;

        public bool FixedFocus { get; protected set; }

        public AdminPanelConsole()
        {
            FixedFocus = true;

            AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
            {
                handler.AddPanelGUI("Console", new AdminPanelConsoleConfiguration(this));
            };

            Clear();
        }

        public void Print(string text)
        {
            Content += text + "\n";
            ContentChanged();
        }

        public void Clear()
        {
            Content = "";
            ContentChanged();
        }

        private void ContentChanged()
        {
            if(OnContentChanged != null)
            {
                OnContentChanged();
            }
        }

        private class AdminPanelConsoleConfiguration : AdminPanelGUI 
        {
            private AdminPanelConsole _console;

            public AdminPanelConsoleConfiguration(AdminPanelConsole console)
            {
                _console = console;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateTextInput("Enter command", (value) => {
                    Console.Print("Entered command  " + value);
                });

                layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(_console));

                layout.CreateMargin(2);

                layout.CreateToggleButton("Lock console", _console.FixedFocus, (value) => {
                    _console.FixedFocus = value; });
                
                layout.CreateButton("Clear console", () => { _console.Clear(); });
            }
        }

        private class AdminPanelAvailableCommands : AdminPanelGUI 
        {
            private AdminPanelConsole _console;
            
            public AdminPanelAvailableCommands(AdminPanelConsole console)
            {
                _console = console;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Available commands");
            }
        }
    }
}