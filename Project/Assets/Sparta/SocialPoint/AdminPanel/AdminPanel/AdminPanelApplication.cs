#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanelApplication : IAdminPanelGUI, IAdminPanelConfigurer
    {
        public void OnConfigure(AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Application", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Application");
            layout.CreateConfirmButton("Reload current scene", () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
            layout.CreateConfirmButton("Quit Application", Application.Quit);
        }
    }
}

#endif
