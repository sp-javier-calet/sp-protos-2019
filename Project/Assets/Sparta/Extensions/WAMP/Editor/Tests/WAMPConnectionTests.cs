using NUnit.Framework;
using NSubstitute;

using System;
using SocialPoint.Network;
using SocialPoint.WAMP;

namespace SocialPoint.WAMP
{
    [TestFixture]
    [Category("SocialPoint.WAMP")]
    internal class WAMPConnectionTests
    {
        WAMPConnection _connection;
        INetworkClient _client;
        INetworkClientDelegate _delegate;
        INetworkMessageReceiver _receiver;

        [SetUp]
        public void SetUp()
        {
            _client = Substitute.For<INetworkClient>();
            _client.When(x => x.AddDelegate(Arg.Any<INetworkClientDelegate>()))
                .Do(x => {
                _delegate = x.Arg<INetworkClientDelegate>();
            });

            _client.When(x => x.RegisterReceiver(Arg.Any<INetworkMessageReceiver>()))
                .Do(x => {
                _receiver = x.Arg<INetworkMessageReceiver>();
            });

            _connection = new WAMPConnection(_client);
        }

        [Test]
        public void Connect()
        {
            _client.When(x => x.Connect()).Do(x => _delegate.OnClientConnected());

            bool connected = false;
            _connection.Start(() => {
                connected = true;
            });

            Assert.IsTrue(connected);
        }

        [Test]
        public void Connect_Cancel()
        {
            var t = new System.Threading.Thread((obj) => {
                System.Threading.Thread.Sleep(10);
                var del = obj as INetworkClientDelegate;
                del.OnClientConnected(); 
            });
            _client.When(x => x.Connect()).Do(x => t.Start(_delegate));

            bool connected = false;
            var req = _connection.Start(() => connected = true);

            req.Dispose();
            t.Join(15);

            Assert.IsFalse(connected);
        }

        [Test]
        public void Disconnect()
        {
            Connect();
            bool connected = true;

            _client.When(x => x.Disconnect()).Do(x => _delegate.OnClientDisconnected());
            _connection.Stop(() => connected = false);
            Assert.IsFalse(connected);
        }

        [Test]
        public void Disconnect_Cancel()
        {
            Connect();
            bool connected = true;

            var t = new System.Threading.Thread((obj) => {
                System.Threading.Thread.Sleep(10);
                var del = obj as INetworkClientDelegate;
                del.OnClientDisconnected(); 
            });

            _client.When(x => x.Disconnect()).Do(x => t.Start(_delegate));
            var req = _connection.Stop(() => connected = false);

            req.Dispose();
            t.Join(15);

            Assert.IsTrue(connected);
        }

        [Test]
        public void Join()
        {
            Connect();
            bool joined = false;
            const long fakedSessionId = 10;

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                    var message = string.Format("[2, {0}, {{}}]", fakedSessionId);
                    var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                    _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                });
            
            _connection.Join("wamp.test_realm", null, (error, sessionId, dict) => {
                Assert.AreEqual(fakedSessionId, sessionId);
                Assert.IsNull(error);
                Assert.AreEqual(dict.Count, 0);
                joined = true;
            });

            Assert.IsTrue(joined);
        }

        [Test]
        public void Join_Cancel()
        {
            Connect();
            bool joined = false;
            const long fakedSessionId = 10;

            var t = new System.Threading.Thread((obj) => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {{}}]",MsgCode.WELCOME, fakedSessionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                    t.Start(_receiver);
                });

            var req = _connection.Join("wamp.test_realm", null, (error, sessionId, dict) => {
                Assert.AreEqual(fakedSessionId, sessionId);
                Assert.IsNull(error);
                Assert.AreEqual(dict.Count, 0);
                joined = true;
            });

            req.Dispose();
            t.Join(15);

            Assert.IsFalse(joined);
        }

        [Test]
        public void Leave()
        {
            Join();
            bool joined = true;
            const string _fakeReason = "wamp.test_leave";

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                    var message = string.Format("[{0}, {{}}, \"{1}\"]", MsgCode.GOODBYE, _fakeReason);
                    var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                    _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                });

            _connection.Leave((error, reason) => {
                Assert.IsNull(error);
                Assert.AreEqual(reason, _fakeReason);
                joined = true;
            }, "wamp.test_leaving");

            Assert.IsTrue(joined);
        }

        [Test]
        public void Leave_Cancel()
        {
            Join();
            bool joined = true;
            const string _fakeReason = "wamp.test_leave";

            var t = new System.Threading.Thread((obj) => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {{}}, \"{1}\"]", MsgCode.GOODBYE, _fakeReason);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                    t.Start(_receiver);
                });

            var req = _connection.Leave((error, reason) => {
                Assert.IsNull(error);
                Assert.AreEqual(reason, _fakeReason);
                joined = true;
            }, "wamp.test_leaving");

            req.Dispose();
            t.Join(15);

            Assert.IsTrue(joined);
        }
    }
}