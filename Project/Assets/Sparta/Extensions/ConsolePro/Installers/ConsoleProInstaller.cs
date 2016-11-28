using FlyingWormConsole3;
using SocialPoint.Dependency;

public class ConsoleProInstaller : ServiceInstaller, IInitializable
{
    public override void InstallBindings()
    {
        #if (ADMIN_PANEL && !NO_ADMIN_PANEL) || UNITY_EDITOR
        Container.Bind<IInitializable>().ToInstance(this);
        Container.BindUnityComponent<ConsoleProRemoteServer>();
        #endif
    }

    public void Initialize()
    {   
        Container.Resolve<ConsoleProRemoteServer>();
    }
}