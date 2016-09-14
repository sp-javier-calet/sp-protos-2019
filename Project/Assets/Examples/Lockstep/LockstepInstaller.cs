
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
        var model = Container.Resolve<LockstepModel>();
        var ctrl = new ClientLockstepController(model,
               Container.Resolve<IUpdateScheduler>()
       );
        ctrl.Init(new LockstepConfig {
        });
        ctrl.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(model));
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
