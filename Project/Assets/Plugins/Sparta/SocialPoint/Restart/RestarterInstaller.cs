using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.Restart;

public class RestarterInstaller : ServiceInstaller
{
    public override void InstallBindings()
    {
        Container.BindDefault<IRestarter>().ToMethod<IRestarter>(CreateRestarter);
    }

    IRestarter CreateRestarter()
    {
        return new DefaultRestarter(Container.Resolve<IAppEvents>());
    }
}
