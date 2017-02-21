
namespace SocialPoint.Dependency
{
    public class TestInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<ITestService>().ToInstance(new TestService());
        }
    }
}