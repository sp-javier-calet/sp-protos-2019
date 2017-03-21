#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using UnityEngine.UI;

namespace SocialPoint.Extension.Helpshift
{
    public sealed class AdminPanelHelpshift : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IHelpshift _helpshift;
        AdminPanelConsole _console;

        Toggle _toggleHelpshift;

        public AdminPanelHelpshift(IHelpshift helpshift)
        {
            _helpshift = helpshift;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("HelpShift", this));
        }

        void ConsolePrint(string msg)
        {
            if(_console != null)
            {
                _console.Print(msg);
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            bool enabled = _helpshift.IsEnabled;
            layout.CreateLabel("Helpshift");
            _toggleHelpshift = layout.CreateToggleButton("Enable Helpshift", enabled, value => {
                if(value)
                {
                    _helpshift.Enable();
                    _toggleHelpshift.isOn = _helpshift.IsEnabled;
                    ConsolePrint(_toggleHelpshift.isOn ? "Helpshift Enabled" : "Helpshift can not be enabled");
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

#endif
