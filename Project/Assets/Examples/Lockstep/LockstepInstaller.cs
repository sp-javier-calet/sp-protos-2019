
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Utils;
using System;

public class LockstepInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public int Clients = 2;
        public long CommandStep = 300;
    }

    public SettingsData Settings = new SettingsData();


    public override void InstallBindings()
    {
        Container.Rebind<LockstepModel>().ToMethod<LockstepModel>(CreateModel);
        Container.Rebind<ClientLockstepController>().ToMethod<ClientLockstepController>(CreateClientController);
        Container.Bind<IDisposable>().ToLookup<ClientLockstepController>();
        Container.Rebind<ServerLockstepController>().ToMethod<ServerLockstepController>(CreateServerController);
        Container.Bind<IDisposable>().ToLookup<ServerLockstepController>();
    }

    LockstepModel CreateModel()
    {
        return new LockstepModel();
    }

    ClientLockstepController CreateClientController()
    {
        var ctrl = new ClientLockstepController(
            Container.Resolve<LockstepModel>(),
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
            Settings.Clients, Settings.CommandStep);
        return ctrl;
    }

}
