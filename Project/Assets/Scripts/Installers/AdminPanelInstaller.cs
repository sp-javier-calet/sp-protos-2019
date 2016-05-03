using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;
using SocialPoint.Attributes;
using FlyingWormConsole3;
using UnityEngine;

public class AdminPanelInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
#if (ADMIN_PANEL && !NO_ADMIN_PANEL) || UNITY_EDITOR
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.Rebind<AdminPanel>().ToSingleMethod<AdminPanel>(CreateAdminPanel);
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfiler>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAttributes>();
        Container.BindUnityComponent<ConsoleProRemoteServer>();
#endif
    }

    AdminPanel CreateAdminPanel()
    {
        return new AdminPanel(
            Container.ResolveList<IAdminPanelConfigurer>());
    }

    public void Initialize()
    {
        Container.Resolve<ConsoleProRemoteServer>();
    }
}