using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Network
{
    class TestNetworkServer : INetworkServer
    {
        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        INetworkMessageReceiver _receiver;

        public void Start()
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnServerStarted();
            }
        }

        public void Stop()
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnServerStopped();
            }
        }

        public void Fail(Error err)
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnNetworkError(err);
            }
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetTimestamp()
        {
            return 0;
        }

        public bool Running
        {
            get
            {
                return true;
            }
        }

        public string Id
        {
            get
            {
                return string.Empty;
            }
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        public NetworkMessageData MessageData;

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            MessageData = data;
            return null;
        }

        public void ClientConnect(byte clientId)
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnClientConnected(clientId);
            }
        }

        public void ClientDisconnect(byte clientId)
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnClientDisconnected(clientId);
            }
        }

        public void ReceivedMessage(NetworkMessageData data, IReader reader)
        {
            _receiver.OnMessageReceived(data, reader);
        }

        public void ReceivedMessage(NetworkMessageData data)
        {
            foreach(var dlg in _delegates)
            {
                dlg.OnMessageReceived(data);
            }
        }

        void System.IDisposable.Dispose()
        {
            _delegates.Clear();
        }
    }

    class TestNetworkServerFactory : INetworkServerFactory
    {
        #region INetworkServerFactory implementation

        INetworkServer INetworkServerFactory.Create()
        {
            return new TestNetworkServer();
        }

        #endregion
    }

    public interface INetworkMatchDelegate : INetworkServerDelegate, INetworkMessageReceiver
    {
    }

    class TestMatchDelegateFactory : INetworkMatchDelegateFactory
    {
        public Dictionary<string,INetworkMatchDelegate> Delegates = new Dictionary<string,INetworkMatchDelegate>();
        public Dictionary<string,INetworkMessageSender> Senders = new Dictionary<string,INetworkMessageSender>();

        public object Create(string matchId, INetworkMessageSender sender)
        {
            var dlg = Substitute.For<INetworkMatchDelegate>();
            Delegates[matchId] = dlg;

            Senders[matchId] = sender; 
            return dlg;
        }
    }

    [TestFixture]
    [Category("SocialPoint.Network")]
    class MultiMatchControllerTests
    {
        TestNetworkServer _server;
        MultiMatchController _multiMatch;
        TestMatchDelegateFactory _factory;

        const byte ConnectMessageType = 5;


        [SetUp]
        public void SetUp()
        {
            _server = new TestNetworkServer();
            _factory = new TestMatchDelegateFactory();
            _multiMatch = new MultiMatchController(_server, _factory, ConnectMessageType);

            // HACK: avoid not used warning
            _server.RemoveDelegate(_multiMatch);
            _server.AddDelegate(_multiMatch);
        }

        [Test]
        public void ReceivedMatchConnectMessageTests()
        {
            //CHECK OnClientConnected
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);
            _factory.Delegates["matchId"].Received(1).OnClientConnected(1);
        }

        [Test]
        public void ReceivedMatchConnectAndMatchMessageTests()
        {
            //CHECK OnClientConnected
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);


            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader);

            _factory.Delegates["matchId"].Received(1).OnClientConnected(1);
            _factory.Delegates["matchId"].Received(1).OnClientConnected(2);

            //CHECK OnMessageReceived
            reader = CreateReader();
            NetworkMessageData data = new NetworkMessageData {
                ClientIds = new List<byte>{ 2 }
            };
            _server.ReceivedMessage(data, reader);

            _factory.Delegates["matchId"].Received(1).OnMessageReceived(data);
        }


        [Test]
        public void ReceivedNetworkMessagesMultipleRooms()
        {
            //CHECK OnClientConnected
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader);

            var reader2 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 3 }
            }, reader2);

            reader2 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 4 }
            }, reader2);

            _factory.Delegates["matchId"].Received(1).OnClientConnected(1);
            _factory.Delegates["matchId"].Received(1).OnClientConnected(2);
            _factory.Delegates["matchId1"].Received(1).OnClientConnected(3);
            _factory.Delegates["matchId1"].Received(1).OnClientConnected(4);

            //CHECK OnMessageReceived
            NetworkMessageData data = new NetworkMessageData {
                ClientIds = new List<byte>{ 2 }
            };
            reader = CreateReader();
            _server.ReceivedMessage(data, reader);
            _factory.Delegates["matchId"].Received(1).OnMessageReceived(data, reader);
        }

        [Test]
        public void NetworkErrorRoomsTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            var reader1 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader1);

            Error error = new Error(404, "ERROR MESSAGE TEST");
            _server.Fail(error);

            _factory.Delegates["matchId"].Received(1).OnNetworkError(error);
            _factory.Delegates["matchId1"].Received(1).OnNetworkError(error);

        }

       
        [Test]
        public void NetworkClientsInRoomsDisconnectTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 100 }
            }, reader);

            var reader1 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 200 }
            }, reader1);


            _server.ClientDisconnect(100);
            _server.ClientDisconnect(200);

            _factory.Delegates["matchId"].Received(1).OnClientDisconnected(100);
            _factory.Delegates["matchId1"].Received(1).OnClientDisconnected(200);

            _factory.Delegates["matchId"].DidNotReceive().OnClientDisconnected(200);
            _factory.Delegates["matchId1"].DidNotReceive().OnClientDisconnected(100);

        }

        [Test]
        public void NetworkServerStartedFirstClientTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            var reader1 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader1);

            _factory.Delegates["matchId"].Received(1).OnServerStarted();
            _factory.Delegates["matchId1"].Received(1).OnServerStarted();
        }

        [Test]
        public void NetworkServerDisconnectTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            var reader1 = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader1);

            _server.Stop();

            _factory.Delegates["matchId"].Received(1).OnServerStopped();
            _factory.Delegates["matchId1"].Received(1).OnServerStopped();

        }

        [Test]
        public void NetworkServerStopedLastClientDisconnectsTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            _server.ClientDisconnect(1);

            _factory.Delegates["matchId"].Received(1).OnServerStopped();
        }

        [Test]
        public void NetworkServerReconnectionTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            _factory.Delegates["matchId"].Received(1).OnServerStarted();

            _server.ClientDisconnect(1);

            _factory.Delegates["matchId"].Received(1).OnServerStopped();

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            _factory.Delegates["matchId"].Received(1).OnServerStarted();
        }

        [Test]
        public void NetworkServerSendMessageToAllClientsTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId1"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 3 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 4 }
            }, reader);


            _factory.Senders["matchId"].CreateMessage(new NetworkMessageData{ });
            Assert.AreEqual(_server.MessageData.ClientIds.Count, 3);
            Assert.AreEqual(_server.MessageData.ClientIds[0], 1);
            Assert.AreEqual(_server.MessageData.ClientIds[1], 3);
            Assert.AreEqual(_server.MessageData.ClientIds[2], 4);
        }

        [Test]
        public void NetworkServerSendMessageToSpecificClientTests()
        {
            var reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 1 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 2 }
            }, reader);

            reader = CreateReader(new MatchConnectMessage("matchId"));
            _server.ReceivedMessage(new NetworkMessageData {
                MessageType = ConnectMessageType,
                ClientIds = new List<byte>{ 3 }
            }, reader);


            _factory.Senders["matchId"].CreateMessage(new NetworkMessageData {
                ClientIds = new List<byte>{ 3 }
            });
            Assert.AreEqual(_server.MessageData.ClientIds.Count, 1);
            Assert.AreEqual(_server.MessageData.ClientIds[0], 3);
        }

        IReader CreateReader(INetworkShareable msg)
        {
            var mem = new MemoryStream();
            var writer = new SystemBinaryWriter(mem);
            msg.Serialize(writer);
            mem.Seek(0, SeekOrigin.Begin);
            return new SystemBinaryReader(mem);
        }

        IReader CreateReader()
        {
            var mem = new MemoryStream();
            return new SystemBinaryReader(mem);
        }

    }
}
