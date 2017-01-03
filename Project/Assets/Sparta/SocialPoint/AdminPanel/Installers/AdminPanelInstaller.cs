using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;
using SocialPoint.Attributes;

public class AdminPanelInstaller : ServiceInstaller
{
    public override void InstallBindings()
    {
        if(AdminPanel.IsActive)
        {
            Container.Rebind<AdminPanel>().ToMethod<AdminPanel>(CreateAdminPanel);
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelSceneSelector>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();

            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfiler>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAttributes>();
        }
    }

    AdminPanel CreateAdminPanel()
    {
        return new AdminPanel(
            Container.ResolveList<IAdminPanelConfigurer>());
    }
}