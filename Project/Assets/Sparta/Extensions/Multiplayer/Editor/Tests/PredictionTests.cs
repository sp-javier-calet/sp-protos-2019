using NUnit.Framework;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Physics;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class PredictionTests
    {
        LocalNetworkServer _server;
        SimulateNetworkClient _client1;
        SimulateNetworkClient _client2;

        NetworkServerSceneController _serverCtrl;
        NetworkClientSceneController _clientCtrl1;
        NetworkClientSceneController _clientCtrl2;

        [SetUp]
        public void SetUp()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _client1 = new SimulateNetworkClient(localServer);
            _client2 = new SimulateNetworkClient(localServer);
            _serverCtrl = new NetworkServerSceneController(_server, new NetworkSceneContext());
            _clientCtrl1 = new NetworkClientSceneController(_client1, new NetworkSceneContext(), true);
            _clientCtrl2 = new NetworkClientSceneController(_client2, new NetworkSceneContext(), true);

            _serverCtrl.RegisterAction<TestInstatiateAction>(InstatiateActionType, TestInstatiateAction.Apply);
            _serverCtrl.RegisterAction<TestMovementAction>(MovementActionType);
            _clientCtrl1.RegisterAction<TestInstatiateAction>(InstatiateActionType, TestInstatiateAction.Apply);
            _clientCtrl1.RegisterAction<TestMovementAction>(MovementActionType);
            _clientCtrl2.RegisterAction<TestInstatiateAction>(InstatiateActionType, TestInstatiateAction.Apply);
            _clientCtrl2.RegisterAction<TestMovementAction>(MovementActionType);

            _server.Start();
            _client1.Connect();
            _client2.Connect();
        }

        void UpdateServerInterval()
        {
            _serverCtrl.Update(_serverCtrl.SyncController.SyncInterval);
        }

        [Test]
        public void ActionPrediction()
        {
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Instantiate
            _clientCtrl1.ApplyAction(new TestInstatiateAction {
                Position = JVector.Zero
            });

            Assert.That(!_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Move
            _clientCtrl1.ApplyAction(new TestMovementAction {
                Movement = JVector.One
            });

            Assert.That(!_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
        }

        [Test]
        public void MultipleActionPrediction()
        {
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Instantiate
            _clientCtrl1.ApplyAction(new TestInstatiateAction {
                Position = JVector.Zero
            });
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            int totalActions = 3;
            TestMovementAction[] actions = new TestMovementAction[totalActions];


            _client1.BlockEmission = true;

            //Move in client only
            for(int i = 0; i < totalActions; i++)
            {
                actions[i] = new TestMovementAction {
                    Movement = JVector.One * (i + 1)
                };
                _clientCtrl1.ApplyAction(actions[i]);
                Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            }

            //Start moving in server
            int finalAction = totalActions - 1;
            for(int i = 0; i < finalAction; i++)
            {
                _client1.SendNextMessage();
                UpdateServerInterval();
                Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
                Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            }
            _client1.SendNextMessage();
            UpdateServerInterval();
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
            _clientCtrl1.ApplyAction(new TestInstatiateAction {
                Position = JVector.Zero
            });
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.PredictionEquals(_serverCtrl.Scene));

            var movementAction = new TestMovementAction {
                Movement = JVector.One
            };

            _client1.BlockEmission = true;
            _client2.BlockEmission = true;

            //Move in one client locally
            _clientCtrl1.ApplyAction(movementAction);
            //Move in one the other client locally
            _clientCtrl2.ApplyAction(movementAction);

            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl2.PredictionEquals(_serverCtrl.Scene));

            //Start updating
            _client1.SendNextMessage();
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl2.PredictionEquals(_serverCtrl.Scene));
            _client2.SendNextMessage();
            UpdateServerInterval();
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.PredictionEquals(_serverCtrl.Scene));
        }

        /* Helper Classes */

        const byte InstatiateActionType = SceneMsgType.Highest + 1;
        const byte MovementActionType = SceneMsgType.Highest + 2;

        class TestInstatiateAction : INetworkShareable
        {
            public JVector Position;

            public void Deserialize(IReader reader)
            {
                Position = JVectorParser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                JVectorSerializer.Instance.Serialize(Position, writer);
            }

            public static void Apply(NetworkSceneMemento scene, TestInstatiateAction action)
            {
                Transform newObjTransform = Transform.Identity;
                newObjTransform.Position = action.Position;
                var go = new NetworkGameObject();
                go.Init(new NetworkSceneContext(), scene.CurrentScene.FreeObjectId, true, newObjTransform);
                scene.CurrentScene.AddObject(go);
            }
        }

        class TestMovementAction : INetworkShareable, INetworkSceneAction
        {
            public JVector Movement;

            public void Deserialize(IReader reader)
            {
                Movement = JVectorParser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                JVectorSerializer.Instance.Serialize(Movement, writer);
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
    }
}

