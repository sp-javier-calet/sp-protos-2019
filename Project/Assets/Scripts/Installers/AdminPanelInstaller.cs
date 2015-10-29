using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Login;
using SocialPoint.Crash;
using SocialPoint.Rating;
using SocialPoint.AppEvents;
using SocialPoint.Social;
using SocialPoint.Hardware;
using SocialPoint.Notifications;

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
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppRater>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelFacebook>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelHardware>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
    }
}
