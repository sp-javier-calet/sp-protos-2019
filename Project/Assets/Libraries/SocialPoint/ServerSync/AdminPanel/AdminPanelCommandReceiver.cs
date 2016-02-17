using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    public class AdminPanelCommandReceiver : IAdminPanelGUI, IAdminPanelConfigurer
    {
        struct CommandLog
        {
            public string Name;
            public string Id;
            public DateTime Time;
            public string Arguments;

            public CommandLog(string id, string name, long ts, string args)
            {
                Id = id;
                Name = name;
                Time = TimeUtils.GetTime(ts);
                Arguments = args;
            }

            public override string ToString()
            {
                return string.Format("{0} - {1} Id: {2} Arguments: {3}", Time, Name, Id, Arguments);
            }
        }

        readonly CommandReceiver _commandReceiver;

        readonly List<CommandLog> _history;

        AdminPanelLayout _layout;

        AdminPanelCommandReceiver(CommandReceiver receiver)
        {
            _commandReceiver = receiver;
            _history = new List<CommandLog>();

            Reflection.CallPrivateVoidMethod(_commandReceiver, "SetListener", new Action<STCCommand>(OnReceive));
        }

        void OnReceive(STCCommand cmd)
        {
            _history.Add(new CommandLog(cmd.Id, cmd.Name, cmd.Timestamp, cmd.Args.ToString()));
            RefreshLayout();
        }

        void RefreshLayout()
        {
            if(_layout != null && _layout.IsActiveInHierarchy)
            {
                _layout.Refresh();
            }
            else
            {
                _layout = null;
            }
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Command Receiver", this));
        }

        static AttrDic CreateCommand(string name)
        {
            var attr = new AttrDic();
            attr.Set("cid", new AttrString(RandomUtils.GetUuid()));
            attr.Set("cmd", new AttrString(name));
            attr.Set("ts", new AttrLong(TimeUtils.Timestamp));
            attr.Set("args", new AttrDic());
            return attr;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;

            layout.CreateLabel("Command Receiver");

            layout.CreateTextInput("Enter command", name => {
                string commandId;
                var result = _commandReceiver.Receive(CreateCommand(name), out commandId);
                layout.AdminPanel.Console.Print(string.Format("Emulated STC Command '{0}'with cid '{1}'. Received: {2}", name, commandId, result));
            });

            layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(_commandReceiver));

            layout.CreateMargin();

            layout.CreateLabel("STC Command History");

            StringBuilder content = new StringBuilder();
            foreach(var log in _history)
            {
                content.AppendLine(log.ToString());
            }

            layout.CreateVerticalScrollLayout()
                  .CreateTextArea(content.ToString());
            layout.CreateButton("Clear History", _history.Clear);
        }

        class AdminPanelAvailableCommands : IAdminPanelGUI
        {
            readonly CommandReceiver _commandReceiver;

            public AdminPanelAvailableCommands(CommandReceiver receiver)
            {
                _commandReceiver = receiver;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Available STC Commands");

                var registeredCommands = Reflection.GetPrivateField<Dictionary<string, ISTCCommandFactory>>(_commandReceiver, "_registeredCommands");
                if(registeredCommands != null)
                {
                    StringBuilder content = new StringBuilder();
                    foreach(var kpv in registeredCommands)
                    {
                        content.AppendLine(kpv.Key);
                    }

                    layout.CreateTextArea(content.ToString());
                }
            }
        }
    }
}