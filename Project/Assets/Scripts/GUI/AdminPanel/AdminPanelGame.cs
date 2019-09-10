//-----------------------------------------------------------------------
// AdminPanelGame.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

#if ADMIN_PANEL
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;

public class AdminPanelGame : IAdminPanelConfigurer
{
    [RequiredDependency]
    IAppEvents _appEvents;

    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Game", new AdminPanelGameControl(_appEvents));
        adminPanel.DefaultCategory = "System";
    }

    class AdminPanelGameControl : IAdminPanelGUI
    {
        readonly IAppEvents _appEvents;

        public AdminPanelGameControl(IAppEvents appEvents)
        {
            _appEvents = appEvents;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Control");
            layout.CreateConfirmButton("Restart", _appEvents.RestartGame);
            layout.CreateMargin(2);
        }
    }
}

#endif
