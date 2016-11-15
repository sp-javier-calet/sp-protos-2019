
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.AdminPanel;
using SocialPoint.Matchmaking;
using System;

public class LockstepInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public LockstepConfig Config;
        public LockstepServerConfig ServerConfig;
        public LockstepClientConfig ClientConfig;
        public string MatchmakingBaseUrl = "http://int-lod.socialpointgames.es";
        public string MatchmakingWebsocketUrl = "ws://int-lod.socialpointgames.com:8001/find_opponent";
        public bool RunLocalServerClient = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<LockstepConfig>().ToMethod<LockstepConfig>(CreateConfig);
        Container.Rebind<LockstepServerConfig>().ToMethod<LockstepServerConfig>(CreateServerConfig);
        Container.Rebind<LockstepClient>().ToMethod<LockstepClient>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<LockstepClient>();
        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
        Container.Rebind<LockstepNetworkClient>().ToMethod<LockstepNetworkClient>
            (CreateClientNetworkController);
        Container.Rebind<LockstepNetworkServer>().ToMethod<LockstepNetworkServer>
            (CreateServerNetworkController);

        Container.Bind<AdminPanelLockstep>().ToMethod<AdminPanelLockstep>(CreateAdminPanel);
        Container.Bind<IAdminPanelConfigurer>().ToLookup<AdminPanelLockstep>();
    }

    AdminPanelLockstep _adminPanel;
    AdminPanelLockstep CreateAdminPanel()
    {
        _adminPanel = new AdminPanelLockstep(
            Container.Resolve<LockstepClient>());
        return _adminPanel;
    }

    LockstepConfig CreateConfig()
    {
        return Settings.Config ?? new LockstepConfig();
    }

    LockstepServerConfig CreateServerConfig()
    {
        return Settings.ServerConfig ?? new LockstepServerConfig();
    }

    LockstepClientConfig CreateClientConfig()
    {
        return Settings.ClientConfig ?? new LockstepClientConfig();
    }

    LockstepCommandFactory CreateCommandFactory()
    {
        return new LockstepCommandFactory();
    }

    LockstepReplay CreateReplay()
    {
        return new LockstepReplay(
            Container.Resolve<LockstepClient>(),
            Container.Resolve<LockstepCommandFactory>()
        );
    }

    LockstepClient CreateClientController()
    {
        var ctrl = new LockstepClient(
            Container.Resolve<IUpdateScheduler>()
        );
        ctrl.Config = Container.Resolve<LockstepConfig>();
        return ctrl;
    }

    LockstepServer CreateServerController()
    {
        var ctrl = new LockstepServer(
            Container.Resolve<IUpdateScheduler>());
        ctrl.Config = Container.Resolve<LockstepConfig>();
        return ctrl;
    }

    LockstepNetworkClient CreateClientNetworkController()
    {
        return new LockstepNetworkClient(
            Container.Resolve<INetworkClient>(),
            Container.Resolve<LockstepClient>(),
            Container.Resolve<LockstepCommandFactory>());
    }

    LockstepNetworkServer CreateServerNetworkController()
    {
        var ctrl = new LockstepNetworkServer(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<IMatchmakingServer>(),
            Container.Resolve<IUpdateScheduler>());
        ctrl.Config = Container.Resolve<LockstepConfig>();
        ctrl.ServerConfig = Container.Resolve<LockstepServerConfig>();

        if(Settings.RunLocalServerClient)
        {
            ctrl.RegisterLocalClient(
                Container.Resolve<LockstepClient>(),
                Container.Resolve<LockstepCommandFactory>());
        }

        if(_adminPanel != null)
        {
            _adminPanel.RegisterServer(ctrl);
        }

        return ctrl;
    }
}
