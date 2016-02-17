using System.Text;
using System.Reflection;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    public class AdminPanelCommandReceiver : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly CommandReceiver _commandReceiver;

        AdminPanelCommandReceiver(CommandReceiver receiver)
        {
            _commandReceiver = receiver;
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
            layout.CreateLabel("Command Receiver");

            layout.CreateTextInput("Enter command", name => {
                string commandId;
                var result = _commandReceiver.Receive(CreateCommand(name), out commandId);
                layout.AdminPanel.Console.Print(string.Format("Emulated STC Command '{0}'with cid '{1}'. Received: {2}", name, commandId, result));
            });

            layout.CreateOpenPanelButton("Available commands", new AdminPanelAvailableCommands(_commandReceiver));
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