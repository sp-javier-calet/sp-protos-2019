﻿using System;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.ServerMessaging;
using SocialPoint.ServerSync;

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
            Container.Resolve<ILogin>());
    }

    MessageCenter CreateMessageCenter()
    {
        return new MessageCenter(
            Container.Resolve<ICommandQueue>(),
            Container.Resolve<CommandReceiver>(),
            Container.Resolve<IAppEvents>());
    }
}