#if ADMIN_PANEL

using UnityEngine.UI;
using SocialPoint.AdminPanel;

namespace SocialPoint.Rating
{
    public sealed class AdminPanelAppRater : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly IAppRater _appRater;
        Text _infoTextComponent;

        public AdminPanelAppRater(IAppRater appRater)
        {
            _appRater = appRater;
            _appRater.OnRequestResultAction += UpdateInfo;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("App Rater", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Rater");
            layout.CreateMargin();
            layout.CreateLabel("App Rater Info");
            _infoTextComponent = layout.CreateTextArea(_appRater.ToString());
            layout.CreateMargin();
            layout.CreateButton("Show rate view", () => {
                _appRater.ShowRateView();
                UpdateInfo();
            });
            layout.CreateButton("Restart", () => {
                _appRater.ResetStatistics();
                UpdateInfo();
            });
        }

        #endregion

        void UpdateInfo()
        {
            if(_infoTextComponent != null)
            {
                _infoTextComponent.text = _appRater.ToString();
            }
        }
    }
}

#endif
