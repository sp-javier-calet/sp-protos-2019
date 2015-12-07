using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Profiling;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
#if ADMIN_PANEL || UNITY_EDITOR
        Container.Rebind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelProfiler>();
#endif
    }
}