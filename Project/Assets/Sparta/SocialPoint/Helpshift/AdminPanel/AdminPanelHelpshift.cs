#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using UnityEngine.UI;
using System.Text;

namespace SocialPoint.Helpshift
{
    public sealed class AdminPanelHelpshift : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IHelpshift _helpshift;
        AdminPanelConsole _console;
        string _faqSectionId;

        Toggle _toggleHelpshift;

        public AdminPanelHelpshift(IHelpshift helpshift)
        {
            _helpshift = helpshift;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Helpshift", this));
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

            var config = _helpshift.Configuration;
            var builder = new StringBuilder();
            layout.CreateLabel("Configuration");
            builder.Append("Contact Mode: ").AppendLine(config.Mode.ToString());
            builder.Append("Conversation resolution question: ").AppendLine(config.ConversationResolutionQuestionEnabled.ToString());
            builder.Append("Search on new conversation: ").AppendLine(config.SearchOnNewConversationEnabled.ToString());
            layout.CreateTextArea(builder.ToString());
            layout.CreateMargin();

            var userData = _helpshift.UserData;
            builder = new StringBuilder();
            layout.CreateLabel("User Data");
            builder.Append("User Id: ").AppendLine(userData.UserId);
            builder.Append("Metadata: ").AppendLine(StringUtils.DictToString(userData.CustomMetaData));
            builder.Append("Tags: ").AppendLine(string.Join(",", userData.CustomerTags));
            layout.CreateMargin();

            layout.CreateTextInput("FAQ Id", value => _faqSectionId = value, enabled);
            layout.CreateButton("Show FAQ", ShowFAQ, enabled);
            layout.CreateButton("Conversation", _helpshift.ShowConversation, enabled);
        }

        void ShowFAQ()
        {
            if(string.IsNullOrEmpty(_faqSectionId))
            {
                _helpshift.ShowFAQ();
            }
            else
            {
                _helpshift.ShowFAQ(_faqSectionId);
            }
        }
    }
}

#endif
