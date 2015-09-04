using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole : IAdminPanelConfigurer, IAdminPanelGUI 
    {
        public string Content { get; private set; }

        public event Action OnContentChanged;

        public bool FixedFocus { get; protected set; }

        public ConsoleApplication Application { get; private set; }

        public AdminPanelConsole()
        {
            Application = new ConsoleApplication();
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

        public void OnConfigure(AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("Console", this);
            adminPanel.RegisterCommand("clear", "Clear console", (command) => {
                Clear();
            });
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateTextInput("Enter command", OnSubmitCommand, OnValueChange);
            
            layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(this));
            
            layout.CreateMargin(2);
            
            layout.CreateToggleButton("Lock console", FixedFocus, (value) => {
                FixedFocus = value; });
            
            layout.CreateButton("Clear console", () => { Clear(); });
        }
        
        private void OnSubmitCommand(string command)
        {
            Print("$" + command);
            
            ConsoleCommand consoleCommand = Application.FindCommand(command);
            if(consoleCommand != null)
            {
                consoleCommand.Execute();
            }
            else
            {
                Print("Command " + command + " not found");
            }
        }
        
        private void OnValueChange(AdminPanelLayout.InputStatus status)
        {
            status.Suggestion = status.Content;
            
            ConsoleCommand currentCommand = Application.FindCommand(status.Content);
            if(currentCommand != null)
            {
                status.Suggestion += " - " + currentCommand.Description;
            }
        }

        private class AdminPanelAvailableCommands : IAdminPanelGUI 
        {
            private AdminPanelConsole _console;
            
            public AdminPanelAvailableCommands(AdminPanelConsole console)
            {
                _console = console;
            }

            public void OnConfigure(AdminPanel adminPanel)
            {
                adminPanel.RegisterCommand("clear", "Clear console", (command) => {
                    _console.Clear();
                });
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Available commands");

                StringBuilder content = new StringBuilder();
                foreach(KeyValuePair<string, ConsoleCommand> entry in _console.Application)
                {
                    content.Append(entry.Key).Append(" : ").AppendLine(entry.Value.Description);
                }

                layout.CreateTextArea(content.ToString());
            }
        }
    }
}