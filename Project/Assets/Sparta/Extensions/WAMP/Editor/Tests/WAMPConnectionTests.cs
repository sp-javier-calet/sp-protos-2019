using NUnit.Framework;
using NSubstitute;

using System;
using SocialPoint.Network;
using SocialPoint.WAMP;
using SocialPoint.Attributes;

namespace SocialPoint.WAMP
{
    [TestFixture]
    [Category("SocialPoint.WAMP")]
    class WAMPConnectionTests
    {
        const string TestTopic = "sparta.test.topic";
        const long TestSubscriptionId = 123456;

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
            var t = new System.Threading.Thread(obj => {
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

            var t = new System.Threading.Thread(obj => {
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

            var t = new System.Threading.Thread(obj => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {{}}]", MsgCode.WELCOME, fakedSessionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

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

            var t = new System.Threading.Thread(obj => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {{}}, \"{1}\"]", MsgCode.GOODBYE, _fakeReason);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            var req = _connection.Leave((error, reason) => {
                Assert.IsNull(error);
                Assert.AreEqual(reason, _fakeReason);
                joined = true;
            }, "wamp.test_leaving");

            req.Dispose();
            t.Join(15);

            Assert.IsTrue(joined);
        }

        [Test]
        public void Subscribe()
        {
            Join();

            bool subscribed = false;

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(
                x => {
                    var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, TestSubscriptionId);
                    var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                    _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                });

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {
                
            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            _connection.Subscribe(TestTopic, handlerSubscription, completionHandler);

            Assert.IsTrue(subscribed);
        }

        [Test]
        public void Subscribe_Cancel()
        {
            Join();

            bool subscribed = false;

            var t = new System.Threading.Thread(obj => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, TestSubscriptionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {

            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            var req = _connection.Subscribe(TestTopic, handlerSubscription, completionHandler);

            req.Dispose();
            t.Join(15);

            Assert.IsFalse(subscribed);
        }

        [Test]
        public void Unsubscribe()
        {
            Join();
            Subscribe();

            bool unsubscribed = false;

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).Returns(Substitute.For<INetworkMessage>());

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                var message = string.Format("[{0}, 1]", MsgCode.UNSUBSCRIBED);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            Subscriber.OnUnsubscribed completionHandler = error => {
                Assert.IsNull(error);
                unsubscribed = true;
            };

            _connection.Unsubscribe(new Subscriber.Subscription(TestSubscriptionId, TestTopic), completionHandler);

            Assert.IsTrue(unsubscribed);
        }

        [Test]
        public void Unsubscribe_Cancel()
        {
            Join();
            Subscribe();

            bool unsubscribed = false;

            var t = new System.Threading.Thread(obj => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 1]", MsgCode.UNSUBSCRIBED);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).Returns(Substitute.For<INetworkMessage>());

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.OnUnsubscribed completionHandler = error => {
                Assert.IsNull(error);
                unsubscribed = true;
            };

            var req = _connection.Unsubscribe(new Subscriber.Subscription(TestSubscriptionId, TestTopic), completionHandler);

            req.Dispose();
            t.Join(15);

            Assert.IsFalse(unsubscribed);
        }

        [Test]
        public void Call()
        {
            Join();

            bool called = false;

            const string testProcedure = "sparta.test_procedure";
            const string responseKey = "test_response";
            const string responseValue = "This is a test Response";
            const int requestArg1 = 987;
            const string requestArg2 = "Request string param";
            const string requestKey = "test_request";
            const bool requestValue = true;

            var sendingBuffer = String.Empty;
            _client.CreateMessage(Arg.Any<NetworkMessageData>()).Writer.When(x => x.Write(Arg.Any<string>()))
                .Do(x => {
                sendingBuffer += x.Arg<string>();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                //Check sent message
                var parser = new JsonAttrParser();
                var sentData = parser.ParseString(sendingBuffer).AsList;

                Assert.AreEqual(sentData.Count, 6);
                Assert.AreEqual(sentData[0].AsValue.ToInt(), MsgCode.CALL);
                Assert.AreEqual(sentData[3].AsValue.ToString(), testProcedure);

                Assert.AreEqual(sentData[4].AsList.Count, 2);
                var sentArgs = sentData[4].AsList;
                Assert.AreEqual(sentArgs[0].AsValue.ToInt(), requestArg1);
                Assert.AreEqual(sentArgs[1].AsValue.ToString(), requestArg2);

                Assert.AreEqual(sentData[5].AsDic.Count, 1);
                var sentKWArgs = sentData[5].AsDic;
                Assert.IsTrue(sentKWArgs.ContainsKey(requestKey));
                Assert.AreEqual(sentKWArgs.Get(requestKey).AsValue.ToBool(), requestValue);

                //Create the fake received message
                var serializer = new JsonAttrSerializer();
                var data = new AttrDic();
                data.SetValue(responseKey, responseValue);
                var message = string.Format("[{0}, 0, {{}}, [], {1}]", MsgCode.RESULT, serializer.SerializeString(data));
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            Caller.HandlerCall completionHandler = (error, respArgs, respKWArgs) => {
                Assert.IsNull(error);
                called = true;
                Assert.AreEqual(respArgs.Count, 0);
                Assert.AreEqual(respKWArgs.Count, 1);
                Assert.IsTrue(respKWArgs.ContainsKey(responseKey));
                Assert.AreEqual(respKWArgs.GetValue(responseKey).ToString(), responseValue);
            };

            var args = new AttrList();
            args.AddValue(requestArg1);
            args.AddValue(requestArg2);
            var kwargs = new AttrDic();
            kwargs.SetValue(requestKey, requestValue);

            _connection.Call(testProcedure, args, kwargs, completionHandler);

            Assert.IsTrue(called);
        }

        [Test]
        public void Call_Cancel()
        {
            Join();

            bool called = false;

            const string testProcedure = "sparta.test_procedure";

            var t = new System.Threading.Thread(obj => {
                System.Threading.Thread.Sleep(10);
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 0, {{}}, [], {{}}]", MsgCode.RESULT);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Caller.HandlerCall completionHandler = (error, respArgs, respKWArgs) => {
                Assert.IsNull(error);
                called = true;
            };

            var args = new AttrList();
            var kwargs = new AttrDic();

            var req = _connection.Call(testProcedure, args, kwargs, completionHandler);

            req.Dispose();
            t.Join(15);

            Assert.IsFalse(called);
        }
    }
}