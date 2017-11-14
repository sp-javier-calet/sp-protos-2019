using NUnit.Framework;
using NSubstitute;
using SocialPoint.Physics;
using SocialPoint.Network;
using Jitter.LinearMath;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    public class MultiplayerPhysicsTests
    {
        LocalNetworkServer _server;
        NetworkServerSceneController _controller;
        NetworkPhysicsWorld _physicsWorld;

        [SetUp]
        public void Setup()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _controller = new NetworkServerSceneController(_server, new NetworkSceneContext());
            _controller.Restart(_server);
            _server.Start();

            _physicsWorld = new NetworkPhysicsWorld(true);
            _controller.Scene.AddBehaviour(_physicsWorld);
            var physicsType = NetworkRigidBody.ControlType.Dynamic;
            var sphereShape = new PhysicsSphereShape(1);
            _controller.RegisterBehaviour(0, null, new NetworkRigidBody().Init(sphereShape, physicsType, _physicsWorld));
        }

        void UpdateServerInterval()
        {
            _controller.Update(_controller.SyncController.SyncInterval);
        }

        [Test]
        public void CreatePhysicsWorldAndNetworkRigidBody()
        {
            var go = _controller.Instantiate(0);
            var behaviour = go.Behaviours.Get<NetworkRigidBody>();
            Assert.IsNotNull(behaviour);
        }

        [Test]
        public void NetworkRigidBodyCollides()
        {
            var collisionHandler = Substitute.For<PhysicsRigidBody.CollisionHandler>();
            var go0 = _controller.Instantiate(0);
            go0.Init(_controller.Context, _controller.Scene.FreeObjectId, false, new Transform(new JVector(0)));
            var go1 = _controller.Instantiate(0);
            go1.Init(_controller.Context, _controller.Scene.FreeObjectId, false, new Transform(new JVector(0, 2, 0)));
            UpdateServerInterval();
            var rigidbody = go1.GetBehaviour<NetworkRigidBody>();
            rigidbody.AddCollisionEnterHandler(collisionHandler);
            rigidbody.AddForce(new JVector(0, -5, 0));
            for(int i = 0; i < 10; i++)
            {
                UpdateServerInterval();
            }
            collisionHandler.ReceivedWithAnyArgs().Invoke(Arg.Any<Jitter.Dynamics.RigidBody>(), Arg.Any<List<PhysicsContact>>());
        }

    }
}
