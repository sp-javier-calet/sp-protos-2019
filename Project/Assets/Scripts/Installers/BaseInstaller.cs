using SocialPoint.Dependency;
using SocialPoint.Utils;

public class BaseInstaller : Installer, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
        Container.BindUnityComponent<UnityUpdateRunner>();
        Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();
        Container.Rebind<IFixedUpdateScheduler>().ToLookup<UnityUpdateRunner>();
    }

    public void Initialize()
    {
        var scheduler = Container.Resolve<IUpdateScheduler>();
        var updateables = Container.ResolveList<IUpdateable>().ToArray();
        if(updateables != null)
        {
            scheduler.Add(updateables);
        }
    }
}
