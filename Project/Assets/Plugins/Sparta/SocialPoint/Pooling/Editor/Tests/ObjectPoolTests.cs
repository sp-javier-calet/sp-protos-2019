using NUnit.Framework;
using System;
using System.Collections.Generic;

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

        class TestDisposable : IDisposable
        {
            public int DisposeCount;

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        class TestRecyclable : IRecyclable
        {
            public int SpawnCount;
            public int RecycleCount;

            public void OnSpawn()
            {
                SpawnCount++;
            }

            public void OnRecycle()
            {
                RecycleCount++;
            }
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
            var obj = new TestDisposable();
            _pool.Return(obj);
            Assert.AreEqual(0, obj.DisposeCount);
            var obj2 = _pool.TryGet<TestDisposable>();
            Assert.AreEqual(0, obj.DisposeCount);
            Assert.AreEqual(obj, obj2);
            _pool.Return(obj);
            Assert.AreEqual(0, obj.DisposeCount);
            _pool.Clear();
            Assert.AreEqual(1, obj.DisposeCount);
        }

        [Test]
        public void DisposableById()
        {
            var obj = new TestDisposable();
            _pool.Return(1, obj);
            Assert.AreEqual(0, obj.DisposeCount);
            var obj2 = _pool.TryGet<TestDisposable>(1);
            Assert.AreEqual(0, obj.DisposeCount);
            Assert.AreEqual(obj, obj2);
            _pool.Return(1, obj);
            Assert.AreEqual(0, obj.DisposeCount);
            _pool.Clear();
            Assert.AreEqual(1, obj.DisposeCount);
        }

        [Test]
        public void RecyclabeByType()
        {
            var obj = new TestRecyclable();
            _pool.Return(obj);
            Assert.AreEqual(1, obj.RecycleCount);
            Assert.AreEqual(0, obj.SpawnCount);
            var obj2 = _pool.TryGet<TestRecyclable>();
            Assert.AreEqual(obj, obj2);
            Assert.AreEqual(1, obj.RecycleCount);
            Assert.AreEqual(1, obj.SpawnCount);
        }


        [Test]
        public void RecyclabeById()
        {
            var obj = new TestRecyclable();
            _pool.Return(1, obj);
            Assert.AreEqual(1, obj.RecycleCount);
            Assert.AreEqual(0, obj.SpawnCount);
            var obj2 = _pool.TryGet<IRecyclable>(1);
            Assert.AreEqual(obj, obj2);
            Assert.AreEqual(1, obj.RecycleCount);
            Assert.AreEqual(1, obj.SpawnCount);
        }

        [Test]
        public void ClearCollection()
        {
            var list = new List<int>();
            list.Add(42);
            _pool.Return(list);
            Assert.AreEqual(0, list.Count);
            var dict = new Dictionary<int, int>();
            dict[42] = 42;
            _pool.Return(1, dict);
            Assert.AreEqual(0, dict.Count);

        }

    }
}
