#if ADMIN_PANEL 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Console;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelUtilCommands : IAdminPanelConfigurer
    {
        AdminPanelConsole _console;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterCommand("echo", "print arguments", OnEcho);
        }

        void OnEcho(ConsoleCommand cmd)
        {
            if(_console != null)
            {
                var args = string.Join(" ", new List<string>(cmd.Arguments).ToArray());
                _console.Print(args);
            }
        }
    }
}

#endif
