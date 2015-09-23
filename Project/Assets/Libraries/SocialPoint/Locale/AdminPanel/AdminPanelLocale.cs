using SocialPoint.AdminPanel;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

namespace SocialPoint.Locale
{
    public class AdminPanelLocale : IAdminPanelGUI, IAdminPanelConfigurer
    {
        ILocalizationManager _manager;

        AdminPanelLocale(ILocalizationManager manager)
        {
            _manager = manager;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Locale", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        Dictionary<string,Toggle> _langButtons;

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Change Language");
            _langButtons = new Dictionary<string,Toggle>();
            foreach(var lang in _manager.SupportedLanguages)
            {
                var blang = lang;
                _langButtons[lang] = layout.CreateToggleButton(blang, false, (enabled) => {
                    _manager.CurrentLanguage = blang;
                    UpdateLangButtons();
                });
            }
            UpdateLangButtons();
        }

        void UpdateLangButtons()
        {
            foreach(var lang in _manager.SupportedLanguages)
            {
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

    }

}
