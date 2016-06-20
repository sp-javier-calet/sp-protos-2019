using SocialPoint.AdminPanel;
using SocialPoint.ServerMessaging;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;
using SocialPoint.AppEvents;
using SocialPoint.Login;
using System;

public class MessageCenterInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IMessageCenter>().ToMethod<MessageCenter>(CreateMessageCenter);
        Container.Bind<IDisposable>().ToLookup<IMessageCenter>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMessageCenter>(CreateAdminPanel);
    }

    AdminPanelMessageCenter CreateAdminPanel()
    {
        return new AdminPanelMessageCenter(
            Container.Resolve<IMessageCenter>(),
            Container.Resolve<ILoginData>());
    }

    MessageCenter CreateMessageCenter()
    {
        return new MessageCenter(
            Container.Resolve<ICommandQueue>(),
            Container.Resolve<CommandReceiver>(),
            Container.Resolve<IAppEvents>());
    }
}