using NUnit.Framework;
using NSubstitute;

using System;
using SocialPoint.Network;
using SocialPoint.WAMP;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.WAMP
{
    [TestFixture]
    [Category("SocialPoint.WAMP")]
    class WAMPConnectionTests
    {
        const string TestTopic = "sparta.test.topic";
        const long SubscriptionId = 123456;
        const long PublicationId = 789012;

        const string TestProcedure = "sparta.test_procedure";
        const string ResponseKey = "test_response";
        const string ResponseValue = "This is a test Response";
        const int RequestArg1 = 987;
        const string RequestArg2 = "Request string param";
        const string RequestKey = "test_request";
        const bool RequestValue = true;

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
            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var del = obj as INetworkClientDelegate;
                del.OnClientConnected();
                semaphore.Release();
            });
            _client.When(x => x.Connect()).Do(x => t.Start(_delegate));

            bool connected = false;
            var req = _connection.Start(() => connected = true);

            req.Dispose();
            semaphore.Release();
            t.Join();

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

            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var del = obj as INetworkClientDelegate;
                del.OnClientDisconnected();
                semaphore.Release();
            });

            _client.When(x => x.Disconnect()).Do(x => t.Start(_delegate));
            var req = _connection.Stop(() => connected = false);

            req.Dispose();
            semaphore.Release();
            t.Join();

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

            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {{}}]", MsgCode.WELCOME, fakedSessionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
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
            semaphore.Release();
            t.Join();

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

            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {{}}, \"{1}\"]", MsgCode.GOODBYE, _fakeReason);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            var req = _connection.Leave((error, reason) => {
                Assert.IsNull(error);
                Assert.AreEqual(reason, _fakeReason);
                joined = true;
            }, "wamp.test_leaving");

            req.Dispose();
            semaphore.Release();
            t.Join();

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
                    var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, SubscriptionId);
                    var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                    _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                });

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {
                
            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            var req = _connection.CreateSubscribe(TestTopic, handlerSubscription, completionHandler);
            _connection.SendSubscribe(req);

            Assert.IsTrue(subscribed);
        }

        [Test]
        public void Subscribe_Cancel()
        {
            Join();

            bool subscribed = false;

            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, SubscriptionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {

            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            var req = _connection.CreateSubscribe(TestTopic, handlerSubscription, completionHandler);
            _connection.SendSubscribe(req);

            req.Dispose();
            semaphore.Release();
            t.Join();

            Assert.IsFalse(subscribed);
        }

        [Test]
        public void Subscribe_Error()
        {
            Join();

            Error requestError = null;

            const int requestId = 0;

            var semaphore = new System.Threading.Semaphore(0, 1);
            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {2}, \"\", \"\"]", MsgCode.ERROR, MsgCode.SUBSCRIBE, requestId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {

            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                requestError = error;
            };

            var req = _connection.CreateSubscribe(TestTopic, handlerSubscription, completionHandler);
            _connection.SendSubscribe(req);
            semaphore.Release();
            t.Join();

            Assert.IsTrue(!Error.IsNullOrEmpty(requestError));
        }

        [Test]
        public void Unsubscribe()
        {
            Join();
            Subscribe();

            bool unsubscribed = false;

            ClearMessageSubstitute();

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

            var req = _connection.CreateUnsubscribe(new Subscriber.Subscription(SubscriptionId, TestTopic), completionHandler);
            _connection.SendUnsubscribe(req);

            Assert.IsTrue(unsubscribed);
        }

        [Test]
        public void Unsubscribe_Cancel()
        {
            Join();
            Subscribe();

            bool unsubscribed = false;
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 1]", MsgCode.UNSUBSCRIBED);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            ClearMessageSubstitute();

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.OnUnsubscribed completionHandler = error => {
                Assert.IsNull(error);
                unsubscribed = true;
            };

            var req = _connection.CreateUnsubscribe(new Subscriber.Subscription(SubscriptionId, TestTopic), completionHandler);
            _connection.SendUnsubscribe(req);

            req.Dispose();
            semaphore.Release();
            t.Join();

            Assert.IsFalse(unsubscribed);
        }

        [Test]
        public void Unsubscribe_Error()
        {
            Join();
            Subscribe();

            Error requestError = null;

            const int requestId = 1;//0 was for subscribe
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {2}, \"\", \"\"]", MsgCode.ERROR, MsgCode.UNSUBSCRIBE, requestId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            ClearMessageSubstitute();

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Subscriber.OnUnsubscribed completionHandler = error => {
                requestError = error;
            };

            var req = _connection.CreateUnsubscribe(new Subscriber.Subscription(SubscriptionId, TestTopic), completionHandler);
            _connection.SendUnsubscribe(req);
            semaphore.Release();

            t.Join();

            Assert.IsTrue(!Error.IsNullOrEmpty(requestError));
        }

        [Test]
        public void Publish()
        {
            Join();

            bool published = false;

            var sendingBuffer = String.Empty;
            _client.CreateMessage(Arg.Any<NetworkMessageData>()).Writer.When(x => x.Write(Arg.Any<string>()))
                .Do(x => {
                sendingBuffer += x.Arg<string>();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(
                x => {
                    //Check sent message
                    var parser = new JsonAttrParser();
                    var sentData = parser.ParseString(sendingBuffer).AsList;

                    Assert.AreEqual(sentData.Count, 6);
                    Assert.AreEqual(sentData[0].AsValue.ToInt(), MsgCode.PUBLISH);
                    Assert.AreEqual(sentData[3].AsValue.ToString(), TestTopic);

                    CheckValidArgs(sentData[4]);
                    CheckValidKWArgs(sentData[5]);

                    //Create the fake received message
                    var message = string.Format("[{0}, 0, {1}]", MsgCode.PUBLISHED, PublicationId);
                    var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                    _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                });

            Publisher.OnPublished completionHandler = (error, publication) => {
                Assert.IsNull(error);
                Assert.AreEqual(publication.Id, PublicationId);
                Assert.AreEqual(publication.Topic, TestTopic);
                published = true;
            };

            var req = _connection.CreatePublish(TestTopic, CreateArgs(), CreateKWArgs(), true, completionHandler);
            _connection.SendPublish(req);

            Assert.IsTrue(published);
        }

        [Test]
        public void Publish_Cancel()
        {
            Join();

            bool published = false;
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 0, {1}]", MsgCode.PUBLISHED, PublicationId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Publisher.OnPublished completionHandler = (error, publication) => {
                Assert.IsNull(error);
                published = true;
            };

            var args = new AttrList();
            var kwargs = new AttrDic();
            var req = _connection.CreatePublish(TestTopic, args, kwargs, true, completionHandler);
            _connection.SendPublish(req);

            req.Dispose();
            semaphore.Release();
            t.Join();

            Assert.IsFalse(published);
        }

        [Test]
        public void Publish_Error()
        {
            Join();

            Error requestError = null;

            const int requestId = 0;
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {2}, \"\", \"\"]", MsgCode.ERROR, MsgCode.PUBLISH, requestId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Publisher.OnPublished completionHandler = (error, publication) => {
                requestError = error;
            };

            var args = new AttrList();
            var kwargs = new AttrDic();
            var req = _connection.CreatePublish(TestTopic, args, kwargs, true, completionHandler);
            _connection.SendPublish(req);
            semaphore.Release();
            t.Join();

            Assert.IsTrue(!Error.IsNullOrEmpty(requestError));
        }

        [Test]
        public void Subscribe_Publish()
        {
            Join();

            bool subscribed = false;
            bool eventReceived = false;

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, SubscriptionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
                        
                //Create the fake EVENT message
                var serializer = new JsonAttrSerializer();
                message = string.Format("[{0}, {1}, {2}, {{}}, {3}, {4}]", MsgCode.EVENT, SubscriptionId, PublicationId, serializer.SerializeString(CreateArgs()), serializer.SerializeString(CreateKWArgs()));
                reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {
                eventReceived = true;

                CheckValidArgs(attrList);
                CheckValidKWArgs(attrDict);
            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            var req = _connection.CreateSubscribe(TestTopic, handlerSubscription, completionHandler);
            _connection.SendSubscribe(req);

            Assert.IsTrue(subscribed);
            Assert.IsTrue(eventReceived);
        }

        [Test]
        public void Subscribe_Unsubscribe_Publish()
        {
            Join();

            bool subscribed = false;
            bool eventReceived = false;
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                //Create the fake EVENT message
                var serializer = new JsonAttrSerializer();
                var message = string.Format("[{0}, {1}, {2}, {{}}, {3}, {4}]", MsgCode.EVENT, SubscriptionId, PublicationId, serializer.SerializeString(CreateArgs()), serializer.SerializeString(CreateKWArgs()));
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                var message = string.Format("[{0}, 0, {1}]", MsgCode.SUBSCRIBED, SubscriptionId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);

                //Delay the fake EVENT message
                t.Start(_receiver);
            });

            Subscriber.HandlerSubscription handlerSubscription = (attrList, attrDict) => {
                eventReceived = true;

                CheckValidArgs(attrList);
                CheckValidKWArgs(attrDict);
            };

            Subscriber.OnSubscribed completionHandler = (error, subscription) => {
                Assert.IsNull(error);
                subscribed = true;
            };

            var reqSubs = _connection.CreateSubscribe(TestTopic, handlerSubscription, completionHandler);
            _connection.SendSubscribe(reqSubs);

            ClearMessageSubstitute();

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => {
                var message = string.Format("[{0}, 1]", MsgCode.UNSUBSCRIBED);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            var reqUnsubs = _connection.CreateUnsubscribe(new Subscriber.Subscription(SubscriptionId, TestTopic), null);
            _connection.SendUnsubscribe(reqUnsubs);

            semaphore.Release();
            t.Join();

            Assert.IsTrue(subscribed);
            Assert.IsFalse(eventReceived);
        }

        [Test]
        public void Call()
        {
            Join();

            bool called = false;

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
                Assert.AreEqual(sentData[3].AsValue.ToString(), TestProcedure);

                CheckValidArgs(sentData[4]);
                CheckValidKWArgs(sentData[5]);

                //Create the fake received message
                var serializer = new JsonAttrSerializer();
                var data = new AttrDic();
                data.SetValue(ResponseKey, ResponseValue);
                var message = string.Format("[{0}, 0, {{}}, [], {1}]", MsgCode.RESULT, serializer.SerializeString(data));
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                _receiver.OnMessageReceived(new NetworkMessageData(), reader);
            });

            Caller.HandlerCall completionHandler = (error, respArgs, respKWArgs) => {
                Assert.IsNull(error);
                called = true;
                Assert.AreEqual(respArgs.Count, 0);
                Assert.AreEqual(respKWArgs.Count, 1);
                Assert.IsTrue(respKWArgs.ContainsKey(ResponseKey));
                Assert.AreEqual(respKWArgs.GetValue(ResponseKey).ToString(), ResponseValue);
            };

            var req = _connection.CreateCall(TestProcedure, CreateArgs(), CreateKWArgs(), completionHandler);
            _connection.SendCall(req);

            Assert.IsTrue(called);
        }

        [Test]
        public void Call_Cancel()
        {
            Join();

            bool called = false;

            const string testProcedure = "sparta.test_procedure";
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, 0, {{}}, [], {{}}]", MsgCode.RESULT);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Caller.HandlerCall completionHandler = (error, respArgs, respKWArgs) => {
                Assert.IsNull(error);
                called = true;
            };

            var req = _connection.CreateCall(testProcedure, CreateArgs(), CreateKWArgs(), completionHandler);
            _connection.SendCall(req);

            req.Dispose();
            semaphore.Release();
            t.Join();

            Assert.IsFalse(called);
        }

        [Test]
        public void Call_Error()
        {
            Join();

            Error requestError = null;

            const string testProcedure = "sparta.test_procedure";
            const int requestId = 0;
            var semaphore = new System.Threading.Semaphore(0, 1);

            var t = new System.Threading.Thread(obj => {
                semaphore.WaitOne();
                var receiver = obj as INetworkMessageReceiver;
                var message = string.Format("[{0}, {1}, {2}, \"\", \"\"]", MsgCode.ERROR, MsgCode.CALL, requestId);
                var reader = new SocialPoint.WebSockets.WebSocketsTextReader(message);
                receiver.OnMessageReceived(new NetworkMessageData(), reader);
                semaphore.Release();
            });

            _client.CreateMessage(Arg.Any<NetworkMessageData>()).When(x => x.Send())
                .Do(x => t.Start(_receiver));

            Caller.HandlerCall completionHandler = (error, respArgs, respKWArgs) => {
                requestError = error;
            };

            var req = _connection.CreateCall(testProcedure, CreateArgs(), CreateKWArgs(), completionHandler);
            _connection.SendCall(req);

            semaphore.Release();
            t.Join();

            Assert.IsTrue(!Error.IsNullOrEmpty(requestError));
        }

        void ClearMessageSubstitute()
        {
            _client.CreateMessage(Arg.Any<NetworkMessageData>()).Returns(Substitute.For<INetworkMessage>());
        }

        static AttrList CreateArgs()
        {
            var args = new AttrList();
            args.AddValue(RequestArg1);
            args.AddValue(RequestArg2);
            return args;
        }

        static AttrDic CreateKWArgs()
        {
            var kwargs = new AttrDic();
            kwargs.SetValue(RequestKey, RequestValue);
            return kwargs;
        }

        static void CheckValidArgs(Attr args)
        {
            Assert.AreEqual(args.AsList.Count, 2);
            var sentArgs = args.AsList;
            Assert.AreEqual(sentArgs[0].AsValue.ToInt(), RequestArg1);
            Assert.AreEqual(sentArgs[1].AsValue.ToString(), RequestArg2);
        }

        static void CheckValidKWArgs(Attr args)
        {
            Assert.AreEqual(args.AsDic.Count, 1);
            var sentKWArgs = args.AsDic;
            Assert.IsTrue(sentKWArgs.ContainsKey(RequestKey));
            Assert.AreEqual(sentKWArgs.Get(RequestKey).AsValue.ToBool(), RequestValue);
        }
    }
}