using SocialPoint.AdminPanel;
using SocialPoint.ServerMessaging;
using SocialPoint.Dependency;
using System;

public class MessageCenterInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<IMessageCenter>().ToSingle<MessageCenter>();
        Container.Bind<IDisposable>().ToLookup<IMessageCenter>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelMessageCenter>();
    }
}