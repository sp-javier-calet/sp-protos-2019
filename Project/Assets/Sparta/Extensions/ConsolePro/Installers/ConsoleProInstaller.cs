using FlyingWormConsole3;
using SocialPoint.Dependency;

namespace SocialPoint.ConsolePro
{
    public class ConsoleProInstaller : ServiceInstaller, IInitializable
    {
        public override void InstallBindings()
        {
            #pragma warning disable 0162
            if(AdminPanel.AdminPanel.IsAvailable)
            {
                Container.Bind<IInitializable>().ToInstance(this);
                Container.BindUnityComponent<ConsoleProRemoteServer>();
            }
            #pragma warning restore 0162
        }

        public void Initialize()
        {   
            Container.Resolve<ConsoleProRemoteServer>();
        }
    }
}