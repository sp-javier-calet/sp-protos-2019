using SocialPoint.AdminPanel;
using UnityEngine;
using Zenject;

namespace SocialPoint.Locale
{
    public class AdminPanelLocale : IAdminPanelGUI, IAdminPanelConfigurer
    {
        LocalizationManager _manager;

        AdminPanelLocale(LocalizationManager manager)
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

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Change Language");

            foreach(var lang in _manager.SupportedLanguages)
            {
                layout.CreateConfirmButton(lang, ButtonColor.Gray, () => {
                    _manager.CurrentLanguage = lang;
                });
            }
        }

        #endregion

    }

}
