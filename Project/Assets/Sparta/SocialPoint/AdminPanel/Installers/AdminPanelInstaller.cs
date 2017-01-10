using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;
using SocialPoint.Attributes;

public class AdminPanelInstaller : ServiceInstaller
{
    public override void InstallBindings()
    {
        #pragma warning disable 0162
        if(AdminPanel.IsAvailable)
        {
            Container.Rebind<AdminPanel>().ToMethod<AdminPanel>(CreateAdminPanel);
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelSceneSelector>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();

            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfiler>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAttributes>();
        }
        #pragma warning restore 0162
    }

    AdminPanel CreateAdminPanel()
    {
        return new AdminPanel(
            Container.ResolveList<IAdminPanelConfigurer>());
    }
}