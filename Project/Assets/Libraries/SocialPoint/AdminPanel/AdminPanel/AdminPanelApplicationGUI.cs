using UnityEngine;
using SocialPoint.AdminPanel;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelApplicationGUI : IAdminPanelGUI, IAdminPanelConfigurer {

        public void OnConfigure(AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Application", this));
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Application");
            layout.CreateConfirmButton("Reload current scene", () => {
                Application.LoadLevel(Application.loadedLevel);
            });

            layout.CreateConfirmButton("Quit Application", () => {
                Application.Quit();
            });
        }
    }
}
