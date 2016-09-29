
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;

public class LockstepInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public LockstepConfig Config;
        public ServerLockstepConfig ServerConfig;
        public bool RunServerClient = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<LockstepConfig>().ToMethod<LockstepConfig>(CreateConfig);
        Container.Rebind<ServerLockstepConfig>().ToMethod<ServerLockstepConfig>(CreateServerConfig);
        Container.Rebind<ClientLockstepController>().ToMethod<ClientLockstepController>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<ClientLockstepController>();
        Container.Rebind<ServerLockstepController>().ToMethod<ServerLockstepController>(CreateServerController);
        Container.Bind<IDisposable>().ToLookup<ServerLockstepController>();
        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
        Container.Rebind<ClientLockstepNetworkController>().ToMethod<ClientLockstepNetworkController>
            (CreateClientNetworkController, SetupClientNetworkController);
        Container.Rebind<ServerLockstepNetworkController>().ToMethod<ServerLockstepNetworkController>
            (CreateServerNetworkController, SetupServerNetworkController);
    }

    LockstepConfig CreateConfig()
    {
        return Settings.Config ?? new LockstepConfig();
    }

    ServerLockstepConfig CreateServerConfig()
    {
        return Settings.ServerConfig ?? new ServerLockstepConfig();
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
        ctrl.Init(Container.Resolve<LockstepConfig>());
        return ctrl;
    }

    ServerLockstepController CreateServerController()
    {
        var ctrl = new ServerLockstepController(
            Container.Resolve<IUpdateScheduler>(),
            Container.Resolve<LockstepConfig>().CommandStep);
        return ctrl;
    }


    ClientLockstepNetworkController CreateClientNetworkController()
    {
        return new ClientLockstepNetworkController(
            Container.Resolve<INetworkClient>());
    }

    void SetupClientNetworkController(ClientLockstepNetworkController ctrl)
    {
        ctrl.Init(
            Container.Resolve<ClientLockstepController>(),
            Container.Resolve<LockstepCommandFactory>());
    }

    ServerLockstepNetworkController CreateServerNetworkController()
    {
        return new ServerLockstepNetworkController(
            Container.Resolve<INetworkServer>(),
            Container.Resolve<LockstepConfig>(),
            Container.Resolve<ServerLockstepConfig>());
    }

    void SetupServerNetworkController(ServerLockstepNetworkController ctrl)
    {
        ctrl.Init(
            Container.Resolve<ServerLockstepController>());
        if(Settings.RunServerClient)
        {
            ctrl.RegisterLocalClient(
                Container.Resolve<ClientLockstepController>(),
                Container.Resolve<LockstepCommandFactory>());
        }
    }
}
