using NUnit.Framework;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class PredictionTests
    {
        LocalNetworkServer _server;
        LocalNetworkClient _client;

        NetworkServerSceneController _serverCtrl;
        NetworkClientSceneController _clientCtrl;

        TestMultiplayerServerBehaviour _serverReceiver;

        [SetUp]
        public void SetUp()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _client = new LocalNetworkClient(localServer);
            _serverCtrl = new NetworkServerSceneController(_server);
            _clientCtrl = new NetworkClientSceneController(_client);
            _serverReceiver = new TestMultiplayerServerBehaviour(_server, _serverCtrl);
            _serverCtrl.RegisterReceiver(_serverReceiver);

            _serverCtrl.RegisterActionDelegate<TestInstatiateAction>(new TestInstantiateActionDelegate());
            _serverCtrl.RegisterActionDelegate<TestMovementAction>(new TestMovementActionDelegate());
            _clientCtrl.RegisterActionDelegate<TestInstatiateAction>(new TestInstantiateActionDelegate());
            _clientCtrl.RegisterActionDelegate<TestMovementAction>(new TestMovementActionDelegate());

            _server.Start();
            _client.Connect();
        }

        [Test]
        public void ActionPrediction()
        {
            NetworkMessageData msgData;
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));

            //Instantiate
            var instantiateAction = new TestInstatiateAction {
                Position = Vector3.Zero
            };
            msgData = new NetworkMessageData {
                MessageType = InstatiateActionType
            };
            _clientCtrl.ApplyActionAndSend<TestInstatiateAction>(instantiateAction, msgData);

            Assert.That(!_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl.PredictionEquals(_serverCtrl.Scene));
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl.PredictionEquals(_serverCtrl.Scene));

            //Move
            var movementAction = new TestMovementAction {
                Movement = Vector3.One
            };
            msgData = new NetworkMessageData {
                MessageType = MovementActionType
            };
            _clientCtrl.ApplyActionAndSend<TestMovementAction>(movementAction, msgData);

            Assert.That(!_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl.PredictionEquals(_serverCtrl.Scene));
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl.PredictionEquals(_serverCtrl.Scene));
        }

        /* Helper Classes */

        const byte InstatiateActionType = 0;
        const byte MovementActionType = 1;

        class TestInstatiateAction : INetworkShareable
        {
            public Vector3 Position;

            public void Deserialize(IReader reader)
            {
                Position = Vector3Parser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                Vector3Serializer.Instance.Serialize(Position, writer);
            }
        }

        class TestMovementAction : INetworkShareable
        {
            public Vector3 Movement;

            public void Deserialize(IReader reader)
            {
                Movement = Vector3Parser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                Vector3Serializer.Instance.Serialize(Movement, writer);
            }
        }

        class TestInstantiateActionDelegate : INetworkActionDelegate
        {
            public void ApplyAction(object action, NetworkScene scene)
            {
                TestInstatiateAction instantiateAction = (TestInstatiateAction)action;
                Transform newObjTransform = Transform.Identity;
                newObjTransform.Position = instantiateAction.Position;
                var go = new NetworkGameObject(scene.FreeObjectId, newObjTransform);
                scene.AddObject(go);
            }
        }

        class TestMovementActionDelegate : INetworkActionDelegate
        {
            public void ApplyAction(object action, NetworkScene scene)
            {
                TestMovementAction movementAction = (TestMovementAction)action;
                var itr = scene.GetObjectEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current;
                    go.Transform.Position += movementAction.Movement;
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
                    _controller.OnAction<TestInstatiateAction>(ac);
                }
                else if(data.MessageType == MovementActionType)
                {
                    var ac = reader.Read<TestMovementAction>();
                    _controller.OnAction<TestMovementAction>(ac);
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

