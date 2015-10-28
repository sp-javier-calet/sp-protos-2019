using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Login;
using SocialPoint.Crash;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        if(Container.HasBinding<AdminPanel>())
        {
            return;
        }
        Container.Bind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLogin>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
    }
}
