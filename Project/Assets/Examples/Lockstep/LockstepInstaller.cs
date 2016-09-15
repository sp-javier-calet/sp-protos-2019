
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Utils;
using System;

public static class GameLockstepMsgType
{
    public static byte Click = 1;
}

public class LockstepInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public long CommandStep = 300;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<ClientLockstepController>().ToMethod<ClientLockstepController>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<ClientLockstepController>();
        Container.Rebind<ServerLockstepController>().ToMethod<ServerLockstepController>(CreateServerController);
        Container.Bind<IDisposable>().ToLookup<ServerLockstepController>();
        Container.Rebind<LockstepCommandFactory>().ToMethod<LockstepCommandFactory>(CreateCommandFactory);
        Container.Rebind<LockstepReplay>().ToMethod<LockstepReplay>(CreateReplay);
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
        ctrl.Init(new LockstepConfig {
        });
        return ctrl;
    }

    ServerLockstepController CreateServerController()
    {
        var ctrl = new ServerLockstepController(
                       Container.Resolve<IUpdateScheduler>(),
                       Settings.CommandStep);
        return ctrl;
    }

}
