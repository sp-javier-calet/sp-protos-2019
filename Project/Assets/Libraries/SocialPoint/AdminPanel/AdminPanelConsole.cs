using System;
using System.Collections;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole {

        public string Content { get; private set; }

        public event Action OnContentChanged;

        public bool FixedFocus { get; protected set; }

        protected ConsoleApplication _consoleApplication;

        public AdminPanelConsole(ConsoleApplication consoleApplication)
        {
            _consoleApplication = consoleApplication;

            AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
            {
                handler.AddPanelGUI("Console", new AdminPanelConsoleConfiguration(this, _consoleApplication));
                handler.RegisterCommand("clear", "Clear console", (command) => {
                    Clear();
                });
            };

            FixedFocus = true;
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
            protected ConsoleApplication _consoleApplication;

            public AdminPanelConsoleConfiguration(AdminPanelConsole console, ConsoleApplication consoleApplication)
            {
                _console = console;
                _consoleApplication = consoleApplication;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateTextInput("Enter command", (value) => {

                    Console.Print("$" + value);

                    ConsoleCommand command = _consoleApplication.FindCommand(value);
                    if(command != null)
                    {
                        command.Execute();
                    }
                    else
                    {
                        Console.Print("Command " + value + " not found");
                    }
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
                // TODO
            }
        }
    }
}