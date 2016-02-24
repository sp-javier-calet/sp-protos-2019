using System;
using SocialPoint.AdminPanel;

namespace SocialPoint.ServerMessaging
{
    public class AdminPanelMessageCenter : IAdminPanelGUI, IAdminPanelConfigurer
    {
        IMessageCenter _mesageCenter;

        public AdminPanelMessageCenter(IMessageCenter messageCenter)
        {
            _mesageCenter = messageCenter;
        }

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Messages");
            layout.CreateButton("Load", _mesageCenter.Load);
        }

        #endregion

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Message Center", this));
        }

        #endregion
    }
}

