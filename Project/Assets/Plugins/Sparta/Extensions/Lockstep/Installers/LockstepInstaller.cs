using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Matchmaking;
using System;
using SocialPoint.ServerEvents;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif


public class LockstepInstaller : ServiceInstaller
{
    [Serializable]
    public class SettingsData
    {
        public LockstepConfig Config;
        public LockstepServerConfig ServerConfig;
        public LockstepClientConfig ClientConfig;
        public bool RunLocalServerClient = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<LockstepConfig>().ToMethod<LockstepConfig>(CreateConfig);
        Container.Rebind<LockstepServerConfig>().ToMethod<LockstepServerConfig>(CreateServerConfig);
        Container.Rebind<LockstepClientConfig>().ToMethod<LockstepClientConfig>(CreateClientConfig);
        Container.Rebind<LockstepClient>().ToMethod<LockstepClient>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<LockstepClient>();

        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
        Container.Rebind<LockstepNetworkClient>().ToMethod<LockstepNetworkClient>
            (CreateClientNetworkController);
        Container.Rebind<LockstepNetworkServer>().ToMethod<LockstepNetworkServer>
            (CreateServerNetworkController);

        #if ADMIN_PANEL
        Container.Bind<AdminPanelLockstep>().ToMethod<AdminPanelLockstep>(CreateAdminPanel);
        Container.Bind<IAdminPanelConfigurer>().ToLookup<AdminPanelLockstep>();
        #endif
    }

    #if ADMIN_PANEL
    AdminPanelLockstep _adminPanel;
    AdminPanelLockstep CreateAdminPanel()
    {
        _adminPanel = new AdminPanelLockstep(
            Container.Resolve<LockstepClient>());
        return _adminPanel;
    }
    #endif

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
        var clientFactory = Container.Resolve<INetworkClientFactory>();

        var client = new LockstepNetworkClient(
            clientFactory.Create(),
            Container.Resolve<LockstepClient>(),
            Container.Resolve<LockstepCommandFactory>());
        client.SendTrack = Container.Resolve<IEventTracker>().TrackSystemEvent;
        return client;
    }

    LockstepNetworkServer CreateServerNetworkController()
    {
        var serverFactory = Container.Resolve<INetworkServerFactory>();

        var ctrl = new LockstepNetworkServer(
            serverFactory.Create(),
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

        #if ADMIN_PANEL
        if(_adminPanel != null)
        {
            _adminPanel.RegisterServer(ctrl);
        }
        #endif

        return ctrl;
    }
}
