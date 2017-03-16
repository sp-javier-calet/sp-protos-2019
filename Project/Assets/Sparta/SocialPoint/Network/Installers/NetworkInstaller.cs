using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Network
{
    public class NetworkInstaller : SubInstaller
    {
        public override void InstallBindings()
        {
            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNetwork>(CreateAdminPanel);
            #endif
        }

        #if ADMIN_PANEL
        AdminPanelNetwork CreateAdminPanel()
        {
            return new AdminPanelNetwork(
                Container.Resolve<IUpdateScheduler>(), Container);
        }
        #endif
    }
}