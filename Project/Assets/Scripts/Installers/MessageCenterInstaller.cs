using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.ServerMessaging;

public class MessageCenterInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<IMessageCenter>().ToSingle<MessageCenter>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelMessageCenter>();
    }
}
