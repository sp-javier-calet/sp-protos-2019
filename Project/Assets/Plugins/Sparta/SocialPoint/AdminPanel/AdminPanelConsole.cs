#if ADMIN_PANEL

using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Console;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanelConsole : IAdminPanelConfigurer, IAdminPanelGUI
    {
        public StringBuilder _contentBuilder;

        public string Content { get { return _contentBuilder.ToString(); } }

        public event Action OnContentChanged;

        public bool FixedFocus { get; private set; }

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

        void ContentChanged()
        {
            if(OnContentChanged != null)
            {
                OnContentChanged();
            }
        }

        public void OnConfigure(AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("Console", this);
            adminPanel.RegisterCommand("clear", "Clear console", command => Clear());
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateTextInput("Enter command", OnSubmitCommand, OnValueChange);
            
            layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(this));
            
            layout.CreateMargin(2);

            var hLayout = layout.CreateHorizontalLayout();
            hLayout.CreateToggleButton("Lock", FixedFocus, value => {
                FixedFocus = value;
            });

            hLayout.CreateButton("Clear", Clear);
        }

        void OnSubmitCommand(string command)
        {
            Print(String.Format("${0}", command));
            try
            {
                Application.Run(command.Split(new []{ ' ' }));
            }
            catch(ConsoleException)
            {
                Print(String.Format("Command {0} not found", command));
            }
        }

        void OnValueChange(AdminPanelLayout.InputStatus status)
        {
            status.Suggestion = status.Content;
            
            ConsoleCommand currentCommand = Application.FindCommand(status.Content);
            if(currentCommand != null)
            {
                status.Suggestion += " - " + currentCommand.Description;
            }
        }

        class AdminPanelAvailableCommands : IAdminPanelGUI
        {
            readonly AdminPanelConsole _console;

            public AdminPanelAvailableCommands(AdminPanelConsole console)
            {
                _console = console;
            }

            public void OnConfigure(AdminPanel adminPanel)
            {
                adminPanel.RegisterCommand("clear", "Clear console", command => _console.Clear());
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Available commands");

                var content = new StringBuilder();
                var itr = _console.Application.GetEnumerator();
                while(itr.MoveNext())
                {
                    var entry = itr.Current;
                    content.Append(entry.Key).Append(" : ").AppendLine(entry.Value.Description);
                }
                itr.Dispose();

                layout.CreateTextArea(content.ToString());
            }
        }
    }
}

#endif
