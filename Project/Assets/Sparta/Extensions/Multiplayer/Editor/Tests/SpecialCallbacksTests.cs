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
        LocalNetworkServer _server;
        NetworkServerSceneController _serverCtrl;

        [SetUp]
        public void Setup()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _serverCtrl = new NetworkServerSceneController(_server);
            _serverCtrl.Restart(_server);
            _server.Start();
        }

        void UpdateServerInterval()
        {
            _serverCtrl.Update(_serverCtrl.SyncInterval);
        }

        [Test]
        public void InstantiateObject()
        {
            _serverCtrl.RegisterBehaviours(TestType, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = _serverCtrl.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            UpdateServerInterval();
            Assert.IsTrue(behaviour.CallbackCalled);
        }

        [Test]
        public void DestroyObject()
        {
            _serverCtrl.RegisterBehaviours(TestType, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = _serverCtrl.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            UpdateServerInterval();
            Assert.IsTrue(behaviour.CallbackCalled);
            behaviour.CallbackCalled = false;
            _serverCtrl.Destroy(go.UniqueId);
            UpdateServerInterval();
            Assert.IsFalse(behaviour.CallbackCalled);
        }

        [Test]
        public void AddBehaviour()
        {
            var go = _serverCtrl.Instantiate(TestType);

            var behaviour = new TestSpecialBehaviour();
            ((NetworkGameObject<INetworkBehaviour>)go).AddBehaviour(behaviour);
            Assert.IsFalse(behaviour.CallbackCalled);
            UpdateServerInterval();
            Assert.IsTrue(behaviour.CallbackCalled);
        }

        [Test]
        public void RemoveBehaviour()
        {
            _serverCtrl.RegisterBehaviours(TestType, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go = _serverCtrl.Instantiate(TestType);
            var behaviour = go.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour.CallbackCalled);
            UpdateServerInterval();
            Assert.IsTrue(behaviour.CallbackCalled);
            behaviour.CallbackCalled = false;
            go.RemoveBehaviour<TestSpecialBehaviour>();
            UpdateServerInterval();
            Assert.IsFalse(behaviour.CallbackCalled);
        }

        [Test]
        public void MultipleObject()
        {
            _serverCtrl.RegisterBehaviours(TestType, new INetworkBehaviour[] {
                new TestSpecialBehaviour(),
            });
            var go1 = _serverCtrl.Instantiate(TestType);
            var go2 = _serverCtrl.Instantiate(TestType);
            var behaviour1 = go1.Behaviours.Get<TestSpecialBehaviour>();
            var behaviour2 = go2.Behaviours.Get<TestSpecialBehaviour>();

            Assert.IsFalse(behaviour1.CallbackCalled);
            Assert.IsFalse(behaviour2.CallbackCalled);
            UpdateServerInterval();
            Assert.IsTrue(behaviour1.CallbackCalled);
            Assert.IsTrue(behaviour2.CallbackCalled);
            behaviour1.CallbackCalled = false;
            behaviour2.CallbackCalled = false;
            _serverCtrl.Destroy(go1.UniqueId);
            UpdateServerInterval();
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

            public void OnDestroy()
            {
            }

            public object Clone()
            {
                return new TestSpecialBehaviour();
            }

            public void Dispose()
            {
                
            }
        }
    }
}