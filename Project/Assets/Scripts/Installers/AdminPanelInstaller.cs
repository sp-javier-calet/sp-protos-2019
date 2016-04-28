using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;
using SocialPoint.Attributes;
using FlyingWormConsole3;

public class AdminPanelInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
#if (ADMIN_PANEL && !NO_ADMIN_PANEL) || UNITY_EDITOR
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.Rebind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfiler>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAttributes>();
        Container.Bind<ConsoleProRemoteServer>().ToSingleGameObject();
#endif
    }

    public void Initialize()
    {
        Container.Resolve<ConsoleProRemoteServer>();
    }
}