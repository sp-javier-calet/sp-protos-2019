#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Hardware;

namespace SocialPoint.GUIControl
{
    public sealed class AdminPanelUI : IAdminPanelGUI, IAdminPanelConfigurer
    {  
        readonly IAttrStorage _storage;
        readonly IDeviceInfo _deviceInfo;

        public AdminPanelUI(IDeviceInfo deviceInfo, IAttrStorage persistentStorage)
        {
            _deviceInfo = deviceInfo;
            _storage = persistentStorage;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("UI", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        AdminPanelLayout _layout;
        AdminPanelUISafeArea _adminPanelUISafeArea;

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _adminPanelUISafeArea = new AdminPanelUISafeArea(_deviceInfo, _storage);

            _layout = layout;
            _layout.CreateLabel("UI");

            layout.CreateOpenPanelButton("Safe Area", _adminPanelUISafeArea);
        }

        #endregion
    }
}

#endif
