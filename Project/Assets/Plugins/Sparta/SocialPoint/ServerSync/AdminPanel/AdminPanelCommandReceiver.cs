#if ADMIN_PANEL 

using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    public sealed class AdminPanelCommandReceiver : IAdminPanelGUI, IAdminPanelConfigurer
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

        AdminPanelConsole _console;

        public AdminPanelCommandReceiver(CommandReceiver receiver)
        {
            _commandReceiver = receiver;
            _history = new List<CommandLog>();

            Reflection.CallPrivateVoidMethod<CommandReceiver>(_commandReceiver, "SetListener", new Action<STCCommand>(OnReceive));
        }

        void OnReceive(STCCommand cmd)
        {
            _history.Add(new CommandLog(cmd.Id, cmd.Name, cmd.Timestamp, cmd.Args.ToString()));
            if(_layout != null)
            {
                _layout.Refresh();
            }
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
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
                if(_console != null)
                {
                    _console.Print(string.Format("Emulated STC Command '{0}'with cid '{1}'. Received: {2}", name, commandId, result));
                }
            });

            layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(_commandReceiver));

            layout.CreateMargin(2);

            layout.CreateLabel("STC Command History");

            var content = new StringBuilder();
            if(_history.Count > 0)
            {
                for(int i = 0, _historyCount = _history.Count; i < _historyCount; i++)
                {
                    var log = _history[i];
                    content.AppendLine(log.ToString());
                }
            }
            else
            {
                content.AppendLine("Empty history");
            }

            layout.CreateVerticalScrollLayout()
                  .CreateTextArea(content.ToString());
            layout.CreateButton("Clear History", () => {
                _history.Clear();
                layout.Refresh();
            });
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

                var registeredCommands = Reflection.GetPrivateField<CommandReceiver, Dictionary<string, ISTCCommandFactory>>(_commandReceiver, "_registeredCommands");
                if(registeredCommands != null)
                {
                    var content = new StringBuilder();
                    var itr = registeredCommands.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var kpv = itr.Current;
                        content.AppendLine(kpv.Key);
                    }
                    itr.Dispose();

                    layout.CreateTextArea(content.ToString());
                }
            }
        }
    }
}

#endif
