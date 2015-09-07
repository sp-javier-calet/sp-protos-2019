using SocialPoint.AdminPanel;

namespace SocialPoint.Hardware
{
    public class AdminPanelHardwareGUI : IAdminPanelConfigurer, IAdminPanelGUI
    {   
        public IDeviceInfo DeviceInfo;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(DeviceInfo != null)
            {
                adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Hardware", this));
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Device Info");
            layout.CreateTextArea(DeviceInfo.ToString());
        }
    }
}