using SocialPoint.AdminPanel;

namespace SocialPoint.WAMP
{

    public class AdminPanelWAMP : IAdminPanelConfigurer, IAdminPanelGUI
    {
        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("WAMP", this);
        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            throw new System.NotImplementedException();
        }

        #endregion
	    
    }
}