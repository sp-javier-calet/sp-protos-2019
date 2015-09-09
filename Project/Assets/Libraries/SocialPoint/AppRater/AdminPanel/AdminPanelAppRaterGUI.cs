using UnityEngine.UI;
using SocialPoint.AdminPanel;

namespace SocialPoint.AppRater
{
    public class AdminPanelAppRaterGUI : IAdminPanelGUI, IAdminPanelConfigurer
    {

        public AppRater AppRater;
        private Text _infoTextComponent;

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("App Rater", this);
        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Rater");
            layout.CreateMargin();
            layout.CreateLabel("App Rater Info");
            layout.CreateTextArea(AppRater.ToString(), out _infoTextComponent);
            layout.CreateMargin();
            layout.CreateButton("Show rate view", () => {
                AppRater.ShowRateView();
                UpdateInfo();
            });
            layout.CreateButton("Restart", () => {
                AppRater.ResetStatistics();
                UpdateInfo();
            });
        }

        #endregion

        private void UpdateInfo()
        {
            _infoTextComponent.text = AppRater.ToString();
        }
    }
}

