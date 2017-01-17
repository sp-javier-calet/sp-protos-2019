using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;
using SocialPoint.Attributes;
using SocialPoint.Base;
using UnityEngine.EventSystems;

public class AdminPanelInstaller : ServiceInstaller, IInitializable
{
    public override void InstallBindings()
    {
        #pragma warning disable 0162
        if(AdminPanel.IsAvailable)
        {
            Container.Bind<IInitializable>().ToInstance(this);

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

    public void Initialize()
    {
        // Force resolution of EventSystem as a requirement
        var eventSystem = Container.Resolve<EventSystem>();
        DebugUtils.Assert(eventSystem != null, "An EventSystem is required for AdminPanel UI");
    }
}
