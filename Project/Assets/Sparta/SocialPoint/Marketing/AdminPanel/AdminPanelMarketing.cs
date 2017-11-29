#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

namespace SocialPoint.Marketing
{
    public sealed class AdminPanelMarketing : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly IMarketingAttributionManager _manager;
        readonly IAttrStorage _storage;
        readonly IDeviceInfo _deviceInfo;
        readonly IAppEvents _appEvents;
        readonly IAlertView _alertPrototype;

        public AdminPanelMarketing(IMarketingAttributionManager manager, IAttrStorage persistentStorage, IDeviceInfo deviceInfo, IAppEvents appEvents, IAlertView alertPrototype)
        {
            _manager = manager;
            _storage = persistentStorage;
            _deviceInfo = deviceInfo;
            _appEvents = appEvents;
            _alertPrototype = alertPrototype;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Marketing", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Marketing");
            layout.CreateMargin();

            CreateAppsFlyerEntry(layout);
            layout.CreateMargin();

            layout.CreateToggleButton("Debug Mode", _manager.DebugMode, debug => {
                _manager.DebugMode = debug;
            });
            layout.CreateButton("Delete AppPreviouslyInstalledForMarketing", () => {
                if(_storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing))
                {
                    _storage.Remove(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing);
                    layout.Refresh();
                }
            }, _storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing));
        }

        #endregion

        void CreateAppsFlyerEntry(AdminPanelLayout layout)
        {
            var appsFlyerTracker = _manager.GetTracker(SocialPointAppsFlyer.TrackerName) as SocialPointAppsFlyer;
            layout.CreateOpenPanelButton("Apps Flyer", 
                new AdminPanelMarketingAppsFlyer(appsFlyerTracker, _deviceInfo, _appEvents, _alertPrototype), 
                /*enabled*/(appsFlyerTracker != null));
        }
    }
}

#endif
