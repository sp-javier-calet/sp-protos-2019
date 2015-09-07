using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole : IAdminPanelConfigurer, IAdminPanelGUI 
    {
        public StringBuilder _contentBuilder;
        public string Content { get { return _contentBuilder.ToString(); } }

        public event Action OnContentChanged;

        public bool FixedFocus { get; protected set; }

        public ConsoleApplication Application { get; private set; }

        public AdminPanelConsole()
        {
            Application = new ConsoleApplication();
            _contentBuilder = new StringBuilder();
            FixedFocus = true;
            Clear();
        }

        public void Print(string text)
        {
            _contentBuilder.AppendLine(text);
            ContentChanged();
        }

        public void Clear()
        {
            _contentBuilder = new StringBuilder();
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

            using(var hLayout = layout.CreateHorizontalLayout())
            {
                hLayout.CreateToggleButton("Lock", FixedFocus, (value) => {
                    FixedFocus = value; });
            
                hLayout.CreateButton("Clear", () => {
                    Clear(); });
            }
        }
        
        private void OnSubmitCommand(string command)
        {
            Print(String.Format("${0}", command));
            
            ConsoleCommand consoleCommand = Application.FindCommand(command);
            if(consoleCommand != null)
            {
                consoleCommand.Execute();
            }
            else
            {
                Print(String.Format("Command {0} not found", command));
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