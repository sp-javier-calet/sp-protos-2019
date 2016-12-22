using NUnit.Framework;
using SocialPoint.Geometry;
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
            _serverCtrl = new NetworkServerSceneController(_server);
            _clientCtrl1 = new NetworkClientSceneController(_client1);
            _clientCtrl2 = new NetworkClientSceneController(_client2);

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
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));

            //Move
            _clientCtrl1.ApplyAction(new TestMovementAction {
                Movement = JVector.One
            });

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
            _clientCtrl1.ApplyAction(new TestInstatiateAction {
                Position = JVector.Zero
            });
            _serverCtrl.Update(0.001f);
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
                _serverCtrl.Update(0.001f);
                Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
                Assert.That(!_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            }
            _client1.SendNextMessage();
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
            _clientCtrl1.ApplyAction(new TestInstatiateAction {
                Position = JVector.Zero
            });
            _serverCtrl.Update(0.001f);
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
            _serverCtrl.Update(0.001f);
            Assert.That(_clientCtrl1.Equals(_serverCtrl.Scene));
            Assert.That(_clientCtrl1.PredictionEquals(_serverCtrl.Scene));
            Assert.That(_clientCtrl2.Equals(_serverCtrl.Scene));
            Assert.That(!_clientCtrl2.PredictionEquals(_serverCtrl.Scene));
            _client2.SendNextMessage();
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
            public Vector Position;

            public void Deserialize(IReader reader)
            {
                Position = VectorParser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                VectorSerializer.Instance.Serialize(Position, writer);
            }

            public static void Apply(NetworkScene scene, TestInstatiateAction action)
            {
                Transform newObjTransform = Transform.Identity;
                newObjTransform.Position = action.Position;
                var go = new NetworkGameObject(scene.FreeObjectId, newObjTransform);
                scene.AddObject(go);
            }
        }

        class TestMovementAction : INetworkShareable, INetworkSceneAction
        {
            public Vector Movement;

            public void Deserialize(IReader reader)
            {
                Movement = VectorParser.Instance.Parse(reader);
            }

            public void Serialize(IWriter writer)
            {
                VectorSerializer.Instance.Serialize(Movement, writer);
            }

            public void Apply(NetworkScene scene)
            {
                var itr = scene.GetObjectEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current;
                    JVector pos = go.Transform.Position;
                    pos += Movement;
                    go.Transform.Position = pos;
                }
                itr.Dispose();
            }
        }
    }
}

