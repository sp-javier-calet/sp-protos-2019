//-----------------------------------------------------------------------
// GameInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Social;
#if ADMIN_PANEL
using SocialPoint.AdminPanel;

#endif

[InstallerGameCategory]
public class GameInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        // Settings will be exposed to the Installer asset
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IGameErrorHandler>().ToSingle<CustomErrorHandler>();
        container.Bind<IPlayerData>().ToSingle<PlayerDataProvider>();

#if ADMIN_PANEL
        container.BindAdminPanelConfigurer<AdminPanelGame>();
#endif
    }
}
