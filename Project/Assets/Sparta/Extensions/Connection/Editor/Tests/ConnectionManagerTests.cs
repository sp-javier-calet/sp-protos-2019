using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Locale;

namespace SocialPoint.Connection
{
    [TestFixture]
    [Category("SocialPoint.Connection")]
    public class ConnectionManagerTests
    {
        IWebSocketClient _websocketClient;
        ConnectionManager _connectionManager;
        List<INetworkClientDelegate> _delegates;
        INetworkMessageReceiver _receiver;

        [SetUp]
        public void SetUp()
        {
            _delegates = new List<INetworkClientDelegate>();
            _websocketClient = Substitute.For<IWebSocketClient>();
            _websocketClient.When(x => x.AddDelegate(Arg.Any<INetworkClientDelegate>())).Do(info => _delegates.Add(info.Arg<INetworkClientDelegate>()));
            _websocketClient.When(x => x.RemoveDelegate(Arg.Any<INetworkClientDelegate>())).Do(info => _delegates.Remove(info.Arg<INetworkClientDelegate>()));
            _websocketClient.When(x => x.RegisterReceiver(Arg.Any<INetworkMessageReceiver>())).Do(x => _receiver = x.Arg<INetworkMessageReceiver>());
            
            _connectionManager = new ConnectionManager(_websocketClient, null);
            _connectionManager.LoginData = Substitute.For<ILoginData>();
            _connectionManager.DeviceInfo = Substitute.For<IDeviceInfo>();
            _connectionManager.Localization = new Localization();
        }

        void CallOnDelegates(Action<INetworkClientDelegate> callback)
        {
            foreach(var del in _delegates)
            {
                callback(del);
            }
        }

        void SendJoinRequest()
        {
            _websocketClient.When(x => x.Connect()).Do(x => CallOnDelegates(del => del.OnClientConnected()));

            const long fakedSessionId = 0;

            _websocketClient.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                var message = string.Format("[2, {0}, {{}}]", fakedSessionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _connectionManager.Reconnect();

            _websocketClient.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send()).Do(x => {
            });
        }

        [Test]
        public void PublishWithoutSession()
        {
            SendJoinRequest();

            _websocketClient.Connected.Returns(true);
            _websocketClient.InStandby.Returns(false);

            _connectionManager.Publish("fake.topic", null, null, (err, publication) => {

            });

            _connectionManager.Update();

            CallOnDelegates(del => del.OnNetworkError(new Error()));
        }

        [Test]
        public void CallWithoutSession()
        {
            SendJoinRequest();

            _websocketClient.Connected.Returns(true);
            _websocketClient.InStandby.Returns(false);

            _connectionManager.Call("fake.rpc", null, null, (err, list, dic) => {

            });

            _connectionManager.Update();

            CallOnDelegates(del => del.OnNetworkError(new Error()));
        }
    }
}
