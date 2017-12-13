using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using NSubstitute;

namespace SocialPoint.Pooling
{
    public class UnityObjectPoolTests
    {
        class TestBehaviour : MonoBehaviour
        {

        }

        GameObject _testGo;

        [SetUp]
        public void SetUp()
        {
            _testGo = new GameObject("CreatePoolTestGO", typeof(TestBehaviour));
        }
    
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testGo);
        }

        [Test]
        public void CreatePool()
        {
            var list = UnityObjectPool.CreatePool(_testGo, 1);
            Assert.AreEqual(1, list.Count);
            Assert.IsNotNull(list.First().GetComponent<TestBehaviour>());
            Assert.IsTrue(UnityObjectPool.IsPooled(_testGo));
        }

        [Test]
        public void SpawnObject()
        {
            var spawned = UnityObjectPool.Spawn(_testGo, null, Vector3.one, Quaternion.identity);
            Assert.IsTrue(UnityObjectPool.IsPooled(_testGo));
            Assert.IsTrue(UnityObjectPool.IsSpawned(spawned));

            Assert.IsNotNull(spawned.GetComponent<TestBehaviour>());
            Assert.IsNull(spawned.transform.parent);
            Assert.AreEqual(Quaternion.identity, spawned.transform.rotation);
            Assert.AreEqual(Vector3.one, spawned.transform.position);
        }

        [Test]
        public void SpawnAndRecycleObject()
        {
            var spawned = UnityObjectPool.Spawn(_testGo);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_testGo));
            UnityObjectPool.Recycle(spawned);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_testGo));
        }

        [Test]
        public void SpawnAndRecycleMultipleObject()
        {
            var spawned1 = UnityObjectPool.Spawn(_testGo);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_testGo));

            var spawned2 = UnityObjectPool.Spawn(_testGo);
            Assert.AreEqual(2, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_testGo));

            UnityObjectPool.Recycle(spawned1);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_testGo));

            var spawned3 = UnityObjectPool.Spawn(_testGo);
            Assert.AreEqual(2, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_testGo));

            UnityObjectPool.Recycle(spawned2);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_testGo));

            UnityObjectPool.Recycle(spawned3);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_testGo));
            Assert.AreEqual(2, UnityObjectPool.CountPooled(_testGo));
        }

        [Test]
        public void ClearPool()
        {
            UnityObjectPool.CreatePool(_testGo, 1);
            Assert.AreEqual(1, UnityObjectPool.CountAllPooled());
            UnityObjectPool.ClearPool();
            Assert.AreEqual(0, UnityObjectPool.CountAllPooled());
        }
    }
}
