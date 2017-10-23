using NUnit.Framework;
using NSubstitute;
using System.IO;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class SpecialCallbacksTests
    {
        const byte TestType = 0;
        static INetworkServer EmptyServer = Substitute.For<INetworkServer>();

        [SetUp]
        public void Setup()
        {
            EmptyServer.Running.Returns(true);
        }

        [Test]
        public void InstantiateObject()
        {
            var context = new NetworkSceneContext();
            var gameObjectPrefab = new NetworkGameObject(context);

            var sceneController = new NetworkServerSceneController(EmptyServer, new NetworkSceneContext(), null);
            sceneController.RegisterBehaviours(TestType, gameObjectPrefab, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = sceneController.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            sceneController.Update(0.0f);
            Assert.IsTrue(behaviour.CallbackCalled);
        }

        [Test]
        public void DestroyObject()
        {
            var context = new NetworkSceneContext();
            var gameObjectPrefab = new NetworkGameObject(context);

            var sceneController = new NetworkServerSceneController(EmptyServer, new NetworkSceneContext(), null);
            sceneController.RegisterBehaviours(TestType, gameObjectPrefab, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = sceneController.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            sceneController.Update(0.0f);
            Assert.IsTrue(behaviour.CallbackCalled);
            behaviour.CallbackCalled = false;
            sceneController.Destroy(go.UniqueId);
            sceneController.Update(0.0f);
            Assert.IsFalse(behaviour.CallbackCalled);
        }

        [Test]
        public void AddBehaviour()
        {
            var sceneController = new NetworkServerSceneController(EmptyServer, new NetworkSceneContext(), null);
            var go = sceneController.Instantiate(TestType);

            var behaviour = new TestSpecialBehaviour();
            var behaviourType = behaviour.GetType();

            go.AddBehaviour(behaviour, behaviourType);
            Assert.IsFalse(behaviour.CallbackCalled);
            sceneController.Update(0.0f);
            Assert.IsTrue(behaviour.CallbackCalled);
        }

        [Test]
        public void RemoveBehaviour()
        {
            var context = new NetworkSceneContext();
            var gameObjectPrefab = new NetworkGameObject(context);
            
            var sceneController = new NetworkServerSceneController(EmptyServer, new NetworkSceneContext(), null);
            sceneController.RegisterBehaviours(TestType, gameObjectPrefab, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = sceneController.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            sceneController.Update(0.0f);
            Assert.IsTrue(behaviour.CallbackCalled);
            behaviour.CallbackCalled = false;
            go.RemoveBehaviour<TestSpecialBehaviour>();
            sceneController.Update(0.0f);
            Assert.IsFalse(behaviour.CallbackCalled);
        }

        [Test]
        public void MultipleObject()
        {
            var context = new NetworkSceneContext();
            var gameObjectPrefab = new NetworkGameObject(context);

            var sceneController = new NetworkServerSceneController(EmptyServer, new NetworkSceneContext(), null);
            sceneController.RegisterBehaviours(TestType, gameObjectPrefab, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go1 = sceneController.Instantiate(TestType);
            var go2 = sceneController.Instantiate(TestType);
            var behaviour1 = go1.Behaviours.Get<TestSpecialBehaviour>();
            var behaviour2 = go2.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour1.CallbackCalled);
            Assert.IsFalse(behaviour2.CallbackCalled);
            sceneController.Update(0.0f);
            Assert.IsTrue(behaviour1.CallbackCalled);
            Assert.IsTrue(behaviour2.CallbackCalled);
            behaviour1.CallbackCalled = false;
            behaviour2.CallbackCalled = false;
            sceneController.Destroy(go1.UniqueId);
            sceneController.Update(0.0f);
            Assert.IsFalse(behaviour1.CallbackCalled);
            Assert.IsTrue(behaviour2.CallbackCalled);
        }

        class TestSpecialBehaviour : INetworkBehaviour, ILateUpdateable
        {
            public bool CallbackCalled { get; set; }

            public void LateUpdate(float dt)
            {
                CallbackCalled = true;
            }

            public void OnAwake()
            {
            }

            public NetworkGameObject GameObject
            {
                set
                {
                }
            }

            public void OnStart()
            {
            }

            public void Update(float dt)
            {
            }

            public void Dispose()
            {
            }

            public void OnDestroy()
            {
            }

            public object Clone()
            {
                return new TestSpecialBehaviour();
            }
        }
    }
}