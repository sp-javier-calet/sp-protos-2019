using NUnit.Framework;
using NSubstitute;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public interface ITestBehaviour
    {
        void Update();
    }

    public class TestBehaviour : ITestBehaviour
    {
        int _data;

        public TestBehaviour(int data = 0)
        {
            _data = data;
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var behaviour = obj as TestBehaviour;
            if(behaviour == null)
            {
                return false;
            }
            return behaviour._data == _data;
        }

        public void Update()
        {
        }
    }

    class UpdateAddBehaviour : ITestBehaviour
    {
        NetworkBehaviourContainer<ITestBehaviour> _container;
        ITestBehaviour _behaviour;

        public UpdateAddBehaviour(NetworkBehaviourContainer<ITestBehaviour> container, ITestBehaviour behaviour)
        {
            _container = container;
            _behaviour = behaviour;
        }

        public object Clone()
        {
            return new UpdateAddBehaviour(_container, _behaviour);
        }

        public void OnStart(NetworkGameObject go)
        {
        }

        public void OnDestroy()
        {
        }


        public void Update()
        {
            _container.Add(_behaviour);
        }
    }

    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class BehaviourTests
    {
        [Test]
        public void DoubleAdd()
        {
            var container = new NetworkBehaviourContainer<ITestBehaviour>();
            var behaviour = new TestBehaviour();
            container.Add(behaviour);
            Assert.AreEqual(1, container.Count);
            container.Add(behaviour);
            Assert.AreEqual(1, container.Count);
            var behaviour2 = new TestBehaviour(1);
            container.Add(behaviour2);
            Assert.AreEqual(2, container.Count);
            var behaviour3 = new TestBehaviour(1);
            container.Add(behaviour3);
            Assert.AreEqual(2, container.Count);
        }

        [Test]
        public void Compare()
        {
            var container1 = new NetworkBehaviourContainer<ITestBehaviour>();
            var container2 = new NetworkBehaviourContainer<ITestBehaviour>();
            Assert.AreEqual(container1, container2);
            container1.Add(new TestBehaviour(1));
            Assert.AreNotEqual(container1, container2);
            container2.Add(new TestBehaviour(1));
            Assert.AreEqual(container1, container2);
        }

        [Test]
        public void AddInLoop()
        {
            var tmpList = new List<ITestBehaviour>();
            
            var container = new NetworkBehaviourContainer<ITestBehaviour>();
            var behaviour = Substitute.For<ITestBehaviour>();
            container.Add(new UpdateAddBehaviour(container, behaviour));
            var itr = container.GetEnumerator(tmpList);
            while(itr.MoveNext())
            {
                itr.Current.Update();
            }
            itr.Dispose();
            Assert.AreEqual(2, container.Count);
            behaviour.DidNotReceive().Update();

            var behaviour2 = Substitute.For<ITestBehaviour>();
            container.Add(new UpdateAddBehaviour(container, behaviour2));
            itr = container.GetEnumerator(tmpList);
            while(itr.MoveNext())
            {
                itr.Current.Update();
            }
            itr.Dispose();
            Assert.AreEqual(4, container.Count);
            behaviour.Received().Update();
        }
    }
}
