
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Utils;
using System;

public class LockstepInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public LockstepConfig Config;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<LockstepConfig>().ToMethod<LockstepConfig>(CreateConfig);
        Container.Rebind<ClientLockstepController>().ToMethod<ClientLockstepController>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<ClientLockstepController>();
        Container.Rebind<ServerLockstepController>().ToMethod<ServerLockstepController>(CreateServerController);
        Container.Bind<IDisposable>().ToLookup<ServerLockstepController>();
        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
    }

    LockstepConfig CreateConfig()
    {
        return Settings.Config ?? new LockstepConfig();
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

}
