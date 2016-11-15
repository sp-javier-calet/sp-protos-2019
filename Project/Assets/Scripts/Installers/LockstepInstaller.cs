﻿
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
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
        public ServerLockstepConfig ServerConfig;
        public ClientLockstepConfig ClientConfig;
        public string MatchmakingBaseUrl = "http://int-lod.socialpointgames.es";
        public string MatchmakingWebsocketUrl = "ws://int-lod.socialpointgames.com:8001/find_opponent";
        public bool RunLocalServerClient = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<LockstepConfig>().ToMethod<LockstepConfig>(CreateConfig);
        Container.Rebind<ServerLockstepConfig>().ToMethod<ServerLockstepConfig>(CreateServerConfig);
        Container.Rebind<ClientLockstepController>().ToMethod<ClientLockstepController>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<ClientLockstepController>();
        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
        Container.Rebind<ClientLockstepNetworkController>().ToMethod<ClientLockstepNetworkController>
            (CreateClientNetworkController);
        Container.Rebind<ServerLockstepNetworkController>().ToMethod<ServerLockstepNetworkController>
            (CreateServerNetworkController);

        Container.Bind<AdminPanelLockstep>().ToMethod<AdminPanelLockstep>(CreateAdminPanel);
        Container.Bind<IAdminPanelConfigurer>().ToLookup<AdminPanelLockstep>();
    }

    AdminPanelLockstep _adminPanel;
    AdminPanelLockstep CreateAdminPanel()
    {
        _adminPanel = new AdminPanelLockstep(
            Container.Resolve<ClientLockstepController>());
        return _adminPanel;
    }

    LockstepConfig CreateConfig()
    {
        return Settings.Config ?? new LockstepConfig();
    }

    ServerLockstepConfig CreateServerConfig()
    {
        return Settings.ServerConfig ?? new ServerLockstepConfig();
    }

    ClientLockstepConfig CreateClientConfig()
    {
        return Settings.ClientConfig ?? new ClientLockstepConfig();
    }

    LockstepCommandFactory CreateCommandFactory()
    {
        return new LockstepCommandFactory();
    }

    LockstepReplay CreateReplay()
    {
        return new LockstepReplay(
            Container.Resolve<ClientLockstepController>(),
            Container.Resolve<LockstepCommandFactory>()
        );
    }

    ClientLockstepController CreateClientController()
    {
        var ctrl = new ClientLockstepController(
            Container.Resolve<IUpdateScheduler>()
        );
        ctrl.Config = Container.Resolve<LockstepConfig>();
        return ctrl;
    }

    ServerLockstepController CreateServerController()
    {
        var ctrl = new ServerLockstepController(
            Container.Resolve<IUpdateScheduler>());
        ctrl.Config = Container.Resolve<LockstepConfig>();
        return ctrl;
    }

    ClientLockstepNetworkController CreateClientNetworkController()
    {
        return new ClientLockstepNetworkController(
            Container.Resolve<INetworkClient>(),
            Container.Resolve<ClientLockstepController>(),
            Container.Resolve<LockstepCommandFactory>());
    }

    ServerLockstepNetworkController CreateServerNetworkController()
    {
        var ctrl = new ServerLockstepNetworkController(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<IMatchmakingServer>(),
            Container.Resolve<IUpdateScheduler>());
        ctrl.Config = Container.Resolve<LockstepConfig>();
        ctrl.ServerConfig = Container.Resolve<ServerLockstepConfig>();

        if(Settings.RunLocalServerClient)
        {
            ctrl.RegisterLocalClient(
                Container.Resolve<ClientLockstepController>(),
                Container.Resolve<LockstepCommandFactory>());
        }

        if(_adminPanel != null)
        {
            _adminPanel.RegisterServer(ctrl);
        }

        return ctrl;
    }
}
