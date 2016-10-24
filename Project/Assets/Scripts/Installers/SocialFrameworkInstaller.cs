using System;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Social;

public class SocialFrameworkInstaller : Installer
{
    const string SocialFrameworkTag = "social_framework";

    [Serializable]
    public class SettingsData
    {
    }

    public override void InstallBindings()
    {   
        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
        Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager);
        Container.Bind<IDisposable>().ToLookup<ChatManager>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanel);

        Container.Bind<INetworkClient>(SocialFrameworkTag).ToMethod<FakeNetworkClient>(CreateNetworkClient);
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager(Container.Resolve<INetworkClient>(SocialFrameworkTag));
    }

    void SetupConnectionManager(ConnectionManager manager)
    {
        manager.AppEvents = Container.Resolve<IAppEvents>();
        manager.Scheduler = Container.Resolve<IUpdateScheduler>();
        manager.LoginData = Container.Resolve<ILoginData>();
        manager.DeviceInfo = Container.Resolve<IDeviceInfo>();
        manager.Localization = Container.Resolve<Localization>();
    }

    ChatManager CreateChatManager()
    {
        return new ChatManager(
            Container.Resolve<ConnectionManager>());
    }

    AdminPanelSocialFramework CreateAdminPanel()
    {
        return new AdminPanelSocialFramework(
            Container.Resolve<INetworkClient>(SocialFrameworkTag),
            Container.Resolve<ConnectionManager>(),
            Container.Resolve<ChatManager>());
    }

    FakeNetworkClient CreateNetworkClient()
    {
        return new FakeNetworkClient();
    }

    // FIXME Dummy class
    #region INetworkClient implementation for testing

    class FakeNetworkClient : INetworkClient
    {
        bool _connected;
        #region INetworkClient implementation
        public void Connect()
        {
            _connected = true;
        }
        public void Disconnect()
        {
            _connected = false;
        }
        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new FakeNetworkMessage();
        }
        public void AddDelegate(INetworkClientDelegate dlg)
        {
            
        }
        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            
        }
        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            
        }
        public int GetDelay(int networkTimestamp)
        {
            return 0;
        }
        public byte ClientId
        {
            get
            {
                return 1;
            }
        }
        public bool Connected
        {
            get
            {
                return _connected;
            }
        }
        #endregion

        class FakeNetworkMessage : INetworkMessage
        {
            #region INetworkMessage implementation
            public void Send()
            {
                
            }
            public SocialPoint.IO.IWriter Writer
            {
                get
                {
                    return null;
                }
            }
            #endregion
        }
    }

    #endregion
}
