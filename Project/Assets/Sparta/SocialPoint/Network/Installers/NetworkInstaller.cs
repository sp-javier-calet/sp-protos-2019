using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.AdminPanel;

namespace SocialPoint.Network
{
    public class NetworkInstaller : SubInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNetwork>(CreateAdminPanel);
        }

        AdminPanelNetwork CreateAdminPanel()
        {
            return new AdminPanelNetwork(
                Container.Resolve<IUpdateScheduler>(), Container);
        }
    }
}