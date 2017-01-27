using System;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.ServerSync;

namespace SocialPoint.ServerMessaging
{
    public class MessageCenterInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(!Settings.UseEmpty)
            {
                Container.Bind<IMessageCenter>().ToMethod<MessageCenter>(CreateMessageCenter);
            }
            else
            {
                Container.Bind<IMessageCenter>().ToSingle<EmptyMessageCenter>();
            }

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
}