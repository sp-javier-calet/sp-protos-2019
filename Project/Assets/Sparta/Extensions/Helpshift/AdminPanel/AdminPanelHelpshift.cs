using SocialPoint.AdminPanel;

namespace SocialPoint.Extension.Helpshift
{
    public class AdminPanelHelpshift : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly SocialPointHelpshift _helpshift;

        public AdminPanelHelpshift(SocialPointHelpshift helpshift)
        {
            _helpshift = helpshift;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("HelpShift", this));           
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            bool enabled = _helpshift.IsEnabled;
            layout.CreateLabel("Helpshift");
            layout.CreateToggleButton("Enable Helpshift", enabled, value => {
                if(value)
                {
                    _helpshift.Enable();
                    layout.Refresh();
                }
            }, !enabled);

            layout.CreateMargin();
            layout.CreateButton("FAQ", ShowFAQ, enabled);
            layout.CreateButton("Conversation", _helpshift.ShowConversation, enabled);
        }

        void ShowFAQ()
        {
            _helpshift.ShowFAQ();
        }
    }
}
