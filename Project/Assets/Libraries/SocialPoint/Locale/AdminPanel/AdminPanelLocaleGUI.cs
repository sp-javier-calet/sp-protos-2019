using SocialPoint.AdminPanel;
using UnityEngine;
using Zenject;

namespace SocialPoint.Locale
{
    public class AdminPanelLocaleGUI : IAdminPanelGUI, IAdminPanelConfigurer
    {
        public LocalizationManager LocalizationManager;
        public Localization Localization;

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

            foreach(var lang in LocalizationManager.SupportedLanguages)
            {
                layout.CreateConfirmButton(lang, () => {
                    LocalizationManager.CurrentLanguage = lang;
                }, ButtonColor.Gray);
            }
        }

        #endregion

    }

}
