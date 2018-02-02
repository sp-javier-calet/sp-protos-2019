using NUnit.Framework;
using NSubstitute;
using System;

namespace SocialPoint.Pooling
{
    [TestFixture]
    [Category("SocialPoint.Pooling")]
    public class ObjectPoolTests
    {
        ObjectPool _pool;

        class TestObject
        {
            public int Value = 42;
        }

        [SetUp]
        public void SetUp()
        {
            _pool = new ObjectPool();
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
        }

        [Test]
        public void NormalByType()
        {
            var obj = _pool.Get<TestObject>();
            Assert.AreEqual(42, obj.Value);
            _pool.Return(obj);
            var obj2 = _pool.Get<TestObject>();
            Assert.AreEqual(obj, obj2);
            _pool.Return(obj);
            _pool.Clear();
            obj2 = _pool.Get<TestObject>();
            Assert.AreNotEqual(obj, obj2);
        }

        [Test]
        public void NormalById()
        {
            var obj = _pool.Get<TestObject>(1);
            Assert.AreEqual(42, obj.Value);
            _pool.Return(1, obj);
            var obj2 = _pool.Get<TestObject>(1);
            Assert.AreEqual(obj, obj2);
            _pool.Return(1, obj);
            obj2 = _pool.Get<TestObject>(2);
            Assert.AreNotEqual(obj, obj2);
            _pool.Return(2, obj2);
            obj2 = _pool.Get<TestObject>(1);
            Assert.AreEqual(obj, obj2);
            _pool.Return(1, obj);
            _pool.Clear();
            obj2 = _pool.Get<TestObject>(2);
            Assert.AreNotEqual(obj, obj2);
        }

        [Test]
        public void DisposableByType()
        {
            var obj = Substitute.For<IDisposable>();
            _pool.Return<IDisposable>(obj);
            obj.DidNotReceive().Dispose();
            var obj2 = _pool.Get<IDisposable>();
            obj.DidNotReceive().Dispose();
            Assert.AreEqual(obj, obj2);
            _pool.Return<IDisposable>(obj);
            obj.DidNotReceive().Dispose();
            _pool.Clear();
            obj.Received(1).Dispose();
        }

        [Test]
        public void DisposableById()
        {
            var obj = Substitute.For<IDisposable>();
            _pool.Return<IDisposable>(1, obj);
            obj.DidNotReceive().Dispose();
            var obj2 = _pool.Get<IDisposable>(1);
            obj.DidNotReceive().Dispose();
            Assert.AreEqual(obj, obj2);
            _pool.Return<IDisposable>(1, obj);
            obj.DidNotReceive().Dispose();
            _pool.Clear();
            obj.Received(1).Dispose();
        }

        [Test]
        public void RecyclabeByType()
        {
            var obj = Substitute.For<IRecyclable>();
            _pool.Return<IRecyclable>(obj);
            obj.Received(1).OnRecycle();
            obj.DidNotReceive().OnSpawn();
            var obj2 = _pool.Get<IRecyclable>();
            Assert.AreEqual(obj, obj2);
            obj.Received(1).OnRecycle();
            obj.Received(1).OnSpawn();
        }


        [Test]
        public void RecyclabeById()
        {
            var obj = Substitute.For<IRecyclable>();
            _pool.Return<IRecyclable>(1, obj);
            obj.Received(1).OnRecycle();
            obj.DidNotReceive().OnSpawn();
            var obj2 = _pool.Get<IRecyclable>(1);
            Assert.AreEqual(obj, obj2);
            obj.Received(1).OnRecycle();
            obj.Received(1).OnSpawn();
        }

    }
}
