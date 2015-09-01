using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole {

        public string Content { get; private set; }

        public event Action OnContentChanged;

        public bool FixedFocus { get; protected set; }

        public ConsoleApplication Application { get; private set; }

        public AdminPanelConsole()
        {
            Application = new ConsoleApplication();

            AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
            {
                handler.AddPanelGUI("Console", new AdminPanelConsoleConfiguration(this));
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
            Content = string.Empty;
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
                layout.CreateTextInput("Enter command", OnSubmitCommand, OnValueChange);

                layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(_console));

                layout.CreateMargin(2);

                layout.CreateToggleButton("Lock console", _console.FixedFocus, (value) => {
                    _console.FixedFocus = value; });
                
                layout.CreateButton("Clear console", () => { _console.Clear(); });
            }

            private void OnSubmitCommand(string command)
            {
                Console.Print("$" + command);
                
                ConsoleCommand consoleCommand = _console.Application.FindCommand(command);
                if(consoleCommand != null)
                {
                    consoleCommand.Execute();
                }
                else
                {
                    Console.Print("Command " + command + " not found");
                }
            }

            private void OnValueChange(AdminPanelLayout.InputStatus status)
            {
                status.Suggestion = status.Content;

                ConsoleCommand currentCommand = _console.Application.FindCommand(status.Content);
                if(currentCommand != null)
                {
                    status.Suggestion += " - " + currentCommand.Description;
                }
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

                string content = "";
                foreach(KeyValuePair<string, ConsoleCommand> entry in _console.Application)
                {
                    content += entry.Key + " : " + entry.Value.Description + "\n";
                }

                layout.CreateTextArea(content);
            }
        }
    }
}