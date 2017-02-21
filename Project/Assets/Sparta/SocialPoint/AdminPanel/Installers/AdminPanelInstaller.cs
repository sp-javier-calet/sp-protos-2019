using SocialPoint.Base;
using SocialPoint.Dependency;
using UnityEngine.EventSystems;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Profiling;
using SocialPoint.Utils;
#endif

public class AdminPanelInstaller : ServiceInstaller, IInitializable
{
    public override void InstallBindings()
    {
        #if ADMIN_PANEL
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
        #endif
    }

    #if ADMIN_PANEL
    AdminPanel CreateAdminPanel()
    {
        return new AdminPanel(
            Container.ResolveList<IAdminPanelConfigurer>());
    }
    #endif

    public void Initialize()
    {
        // Force resolution of EventSystem as a requirement
        var eventSystem = Container.Resolve<EventSystem>();
        DebugUtils.Assert(eventSystem != null, "An EventSystem is required for AdminPanel UI");
    }
}
