using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Console;

namespace SocialPoint.Utils
{
    public class AdminPanelUtilCommands : IAdminPanelConfigurer
    {
        AdminPanel.AdminPanel _adminPanel;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _adminPanel = adminPanel;
            adminPanel.RegisterCommand("echo", "print arguments", OnEcho);
        }

        void OnEcho(ConsoleCommand cmd)
        {
            var args = string.Join(" ", new List<string>(cmd.Arguments).ToArray());
            _adminPanel.Console.Print(args);
        }
    }
}
