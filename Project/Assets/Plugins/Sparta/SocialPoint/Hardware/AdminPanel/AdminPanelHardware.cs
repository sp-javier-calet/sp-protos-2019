#if ADMIN_PANEL 

using SocialPoint.AdminPanel;

namespace SocialPoint.Hardware
{
    public sealed class AdminPanelHardware : IAdminPanelConfigurer, IAdminPanelGUI
    {   
        IDeviceInfo _deviceInfo;

        public AdminPanelHardware(IDeviceInfo deviceInfo)
        {
            _deviceInfo = deviceInfo;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_deviceInfo != null)
            {
                adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Hardware", this));
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Device Info");
            layout.CreateTextArea(_deviceInfo.ToString());
        }
    }
}

#endif
