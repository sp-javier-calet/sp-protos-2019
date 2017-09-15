#if ADMIN_PANEL

using SocialPoint.AdminPanel;

namespace SocialPoint.Alert
{
    public sealed class AdminPanelAlert : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly IAlertView _alertView;

        public AdminPanelAlert(IAlertView alertView)
        {
            _alertView = alertView;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Alert", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel(_alertView.GetType().Name);
            layout.CreateButton("Show alert", ShowAlert);
        }

        void ShowAlert()
        {
            var alert = _alertView.Clone() as IAlertView;
            alert.Message = "Message";
            alert.Signature = "Signature";
            alert.Title = "Title";
            alert.Buttons = new [] {
                "Button 1",
                "Button 2"
            };
            alert.Show(null);
        }
    }
}

#endif
