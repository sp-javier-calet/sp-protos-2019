using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

namespace SocialPoint.Login
{
    public class AdminPanelLoginGUI : AdminPanelGUI, AdminPanelConfigurer {

        public ILogin Login;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("Login", this);
            adminPanel.RegisterCommand("test", "Test command", (command) => { adminPanel.Console.Print("Hola"); });
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Login");
        }
    }
}
