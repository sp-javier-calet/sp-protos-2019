using SocialPoint.AdminPanel;
using SocialPoint.Console;

namespace SocialPoint.Social
{
    public class AdminPanelGoogle : IAdminPanelConfigurer, IAdminPanelGUI
    {
        IGoogle _google;
        AdminPanel.AdminPanel _adminPanel;

        public AdminPanelGoogle(IGoogle google)
        {
            _google = google;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_google == null)
            {
                return;
            }
            _adminPanel = adminPanel;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Google Play", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Google Play");
            layout.CreateMargin();
            layout.CreateLabel("Google Play User");
           
            layout.CreateToggleButton("Logged In", _google.IsConnected, (status) => {
                if(status)
                {
                    _google.Login((err) => {
                    });
                }
                else
                {
                    _google.Logout((err) => {
                    });
                }
            });
        }
    }
}