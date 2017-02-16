﻿#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Marketing
{
    public sealed class AdminPanelMarketing : IAdminPanelGUI, IAdminPanelConfigurer
    {
        IMarketingAttributionManager _manager;
        IAttrStorage _storage;

        public AdminPanelMarketing(IMarketingAttributionManager manager, IAttrStorage persistentStorage)
        {
            _manager = manager;
            _storage = persistentStorage;
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
            layout.CreateToggleButton("Debug Mode", _manager.DebugMode, (debug) => {
                _manager.DebugMode = debug;
            });
            layout.CreateButton("Delete AppPreviouslyInstalledForMarketing", () => {
                if(_storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing))
                {
                    _storage.Remove(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing);
                    layout.Refresh();
                }
            },_storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing));
        }

        #endregion
    }
}

#endif
