using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using UnityEngine.UI;

namespace SocialPoint.Locale
{
    public class AdminPanelLocale : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ILocalizationManager _manager;
        AdminPanel.AdminPanel _adminPanel;

        AdminPanelLocale(ILocalizationManager manager)
        {
            _manager = manager;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _adminPanel = adminPanel;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Locale", this));
            var cmd = new ConsoleCommand()
                .WithDescription("get a localized string")
                .WithDelegate(OnTranslateCommand);
            adminPanel.RegisterCommand("translate", cmd);
        }

        #endregion

        #region IAdminPanelGUI implementation

        Dictionary<string,Toggle> _langButtons;

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Change Language");
            _langButtons = new Dictionary<string,Toggle>();
            for(int i = 0, _managerSupportedLanguagesLength = _manager.SupportedLanguages.Length; i < _managerSupportedLanguagesLength; i++)
            {
                var lang = _manager.SupportedLanguages[i];
                var blang = lang;
                _langButtons[lang] = layout.CreateToggleButton(blang, false, enabled =>  {
                    _manager.CurrentLanguage = blang;
                    UpdateLangButtons();
                });
            }
            UpdateLangButtons();
        }

        void UpdateLangButtons()
        {
            for(int i = 0, _managerSupportedLanguagesLength = _manager.SupportedLanguages.Length; i < _managerSupportedLanguagesLength; i++)
            {
                var lang = _manager.SupportedLanguages[i];
                Toggle button;
                if(_langButtons.TryGetValue(lang, out button))
                {
                    var action = button.onValueChanged;
                    button.onValueChanged = new Toggle.ToggleEvent();
                    button.isOn = lang == _manager.CurrentLanguage;
                    button.onValueChanged = action;
                }
            }
        }

        #endregion

        void OnTranslateCommand(ConsoleCommand cmd)
        {
            var list = new List<string>(cmd.Arguments);
            if(list.Count == 0)
            {
                throw new ConsoleException("Need at least a key argument");
            }
            var trans = _manager.Localization.Get(list[0]);
            list.RemoveAt(0);
            if(list.Count > 0)
            {
                trans = string.Format(trans, list.ToArray());
            }
            _adminPanel.Console.Print(trans);
        }
    }

}
