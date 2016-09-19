﻿using NUnit.Framework;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class PredictionTests
    {
        LocalNetworkServer _server;
        LocalNetworkClient _client1;
        LocalNetworkClient _client2;

        NetworkServerSceneController _serverCtrl;
        NetworkClientSceneController _clientCtrl1;
        NetworkClientSceneController _clientCtrl2;

        TestMultiplayerServerBehaviour _serverReceiver;

        [SetUp]
        public void SetUp()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _client1 = new LocalNetworkClient(localServer);
            _client2 = new LocalNetworkClient(localServer);
            _serverCtrl = new NetworkServerSceneController(_server);
            _clientCtrl1 = new NetworkClientSceneController(_client1);
            _clientCtrl2 = new NetworkClientSceneController(_client2);
            _serverReceiver = new TestMultiplayerServerBehaviour(_server, _serverCtrl);
            _serverCtrl.RegisterReceiver(_serverReceiver);

            _serverCtrl.RegisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            _clientCtrl1.RegisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            _clientCtrl2.RegisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);

            _server.Start();
            _client1.Connect();
            _client2.Connect();
        }

        [Test]
        public void UnregisterActionDelegate()
        {
            _serverCtrl.RegisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            bool success = _serverCtrl.UnregisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            Assert.That(success);
            success = _serverCtrl.UnregisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            Assert.That(!success);

            _clientCtrl1.RegisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            success = _clientCtrl1.UnregisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            Assert.That(success);
            success = _clientCtrl1.UnregisterActionDelegate<TestInstatiateAction>(TestInstatiateAction.Apply);
            Assert.That(!success);
        }

        [Test]
        public void ActionPrediction()
        {
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            NetworkMessageData msgData;

            //Instantiate
            var instantiateAction = new TestInstatiateAction {
                Position = JVector.Zero
            };
            msgData = new NetworkMessageData {
                MessageType = InstatiateActionType
            };
            _clientCtrl1.ApplyActionAndSend<TestInstatiateAction>(instantiateAction, msgData);

            Assert.That(!_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Move
            var movementAction = new TestMovementAction {
                Movement = JVector.One
            };
            msgData = new NetworkMessageData {
                MessageType = MovementActionType
            };
            _clientCtrl1.ApplyActionAndSend<TestMovementAction>(movementAction, msgData);

            Assert.That(!_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
        }

        [Test]
        public void MultipleActionPrediction()
        {
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Instantiate
            var instantiateAction = new TestInstatiateAction {
                Position = JVector.Zero
            };
            NetworkMessageData instatiateMsgData = new NetworkMessageData {
                MessageType = InstatiateActionType
            };
            _clientCtrl1.ApplyActionAndSend<TestInstatiateAction>(instantiateAction, instatiateMsgData);
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            int totalActions = 3;
            NetworkMessageData[] msgData = new NetworkMessageData[totalActions];
            TestMovementAction[] actions = new TestMovementAction[totalActions];

            //Move in client only
            for(int i = 0; i < totalActions; i++)
            {
                actions[i] = new TestMovementAction {
                    Movement = JVector.One * (i + 1)
                };
                msgData[i] = new NetworkMessageData {
                    MessageType = MovementActionType
                };
                _clientCtrl1.ApplyAction<TestMovementAction>(actions[i]);
            }

            //Start moving in server
            int finalAction = totalActions - 1;
            for(int i = 0; i < finalAction; i++)
            {
                _client1.SendMessage(msgData[i], actions[i]);
                _serverCtrl.Update(0.001f);
                Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
                Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            }
            _client1.SendMessage(msgData[finalAction], actions[finalAction]);
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
        }

        [Test]
        public void MultipleClientActionPrediction()
        {
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.PredictionEquals(_serverCtrl.Scene));

            //Instantiate in one client and update for all
            var instantiateAction = new TestInstatiateAction {
                Position = JVector.Zero
            };
            NetworkMessageData instatiateMsgData = new NetworkMessageData {
                MessageType = InstatiateActionType
            };
            _clientCtrl1.ApplyActionAndSend<TestInstatiateAction>(instantiateAction, instatiateMsgData);
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.PredictionEquals(_serverCtrl.Scene));

            var movementAction = new TestMovementAction {
                Movement = JVector.One
            };
            NetworkMessageData msgData = new NetworkMessageData {
                MessageType = MovementActionType
            };
            //Move in one client locally
            _clientCtrl1.ApplyAction<TestMovementAction>(movementAction);
            //Move in one the other client locally
            _clientCtrl2.ApplyAction<TestMovementAction>(movementAction);

            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl2.PredictionEquals(_serverCtrl.Scene));

            //Start updating
            _client1.SendMessage(msgData, movementAction);
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl2.PredictionEquals(_serverCtrl.Scene));
            _client2.SendMessage(msgData, movementAction);
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.PredictionEquals(_serverCtrl.Scene));
        }

        /* Helper Classes */

        const byte InstatiateActionType = 0;
        const byte MovementActionType = 1;

        class TestInstatiateAction : INetworkShareable
        {
            public JVector Position;

            public void Deserialize(IReader reader)
            {
                Position = Vector3Parser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                Vector3Serializer.Instance.Serialize(Position, writer);
            }

            public static void Apply(TestInstatiateAction action, NetworkScene scene)
            {
                Transform newObjTransform = Transform.Identity;
                newObjTransform.Position = action.Position;
                var go = new NetworkGameObject(scene.FreeObjectId, newObjTransform);
                scene.AddObject(go);
            }
        }

        class TestMovementAction : INetworkShareable, IAppliableSceneAction
        {
            public JVector Movement;

            public void Deserialize(IReader reader)
            {
                Movement = Vector3Parser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                Vector3Serializer.Instance.Serialize(Movement, writer);
            }

            public void Apply(NetworkScene scene)
            {
                var itr = scene.GetObjectEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current;
                    go.Transform.Position += Movement;
                }
                itr.Dispose();
            }
        }

        class TestMultiplayerServerBehaviour : INetworkServerSceneReceiver
        {
            NetworkServerSceneController _controller;

            public TestMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl)
            {
                _controller = ctrl;
            }

            void INetworkServerSceneBehaviour.Update(float dt, NetworkScene scene, NetworkScene oldScene)
            {
            }

            void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
            {
                if(data.MessageType == InstatiateActionType)
                {
                    var ac = reader.Read<TestInstatiateAction>();
                    _controller.OnAction<TestInstatiateAction>(ac, data.ClientId);
                }
                else if(data.MessageType == MovementActionType)
                {
                    var ac = reader.Read<TestMovementAction>();
                    _controller.OnAction<TestMovementAction>(ac, data.ClientId);
                }
            }

            void INetworkServerSceneBehaviour.OnClientConnected(byte clientId)
            {
            }

            void INetworkServerSceneBehaviour.OnClientDisconnected(byte clientId)
            {
            }
        }
    }
}

