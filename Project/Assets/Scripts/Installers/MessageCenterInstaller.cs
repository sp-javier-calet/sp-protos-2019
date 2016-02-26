using SocialPoint.AdminPanel;
using SocialPoint.ServerMessaging;
using System;
using Zenject;

public class MessageCenterInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<IMessageCenter>().ToSingle<MessageCenter>();
        Container.Bind<IDisposable>().ToLookup<IMessageCenter>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelMessageCenter>();
    }
}
