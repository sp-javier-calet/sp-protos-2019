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

        GameObject[] _prefabs;

        [SetUp]
        public void SetUp()
        {
            _prefabs = new [] {
                new GameObject("CreatePoolTestGO", typeof(TestBehaviour)),
                new GameObject("CreatePoolTestGO", typeof(TestBehaviour))
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach(var prefab in _prefabs)
            {
                Object.DestroyImmediate(prefab);
            }
            Object.DestroyImmediate(UnityObjectPool.Instance);
        }

        [Test]
        public void CreatePool()
        {
            var list = UnityObjectPool.CreatePool(_prefabs[0], 1);
            Assert.AreEqual(1, list.Count);
            Assert.IsNotNull(list[0].GetComponent<TestBehaviour>());
            Assert.IsTrue(UnityObjectPool.IsPooled(_prefabs[0]));
        }

        [Test]
        public void SpawnObject()
        {
            var spawned = UnityObjectPool.Spawn(_prefabs[0], null, Vector3.one, Quaternion.identity);
            Assert.IsTrue(UnityObjectPool.IsPooled(_prefabs[0]));
            Assert.IsTrue(UnityObjectPool.IsSpawned(spawned));

            Assert.IsNotNull(spawned.GetComponent<TestBehaviour>());
            Assert.IsNull(spawned.transform.parent);
            Assert.AreEqual(Quaternion.identity, spawned.transform.rotation);
            Assert.AreEqual(Vector3.one, spawned.transform.position);
        }

        [Test]
        public void SpawnAndRecycleObject()
        {
            var spawned = UnityObjectPool.Spawn(_prefabs[0]);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[0]));

            UnityObjectPool.Recycle(spawned);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[0]));
        }

        [Test]
        public void SpawnAndRecycleDifferent()
        {
            var spawned1 = UnityObjectPool.Spawn(_prefabs[0]);
            var spawned2 = UnityObjectPool.Spawn(_prefabs[1]);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[1]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[1]));
            Assert.AreEqual(0, UnityObjectPool.CountAllPooled());

            UnityObjectPool.Recycle(spawned1);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[1]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[1]));
            Assert.AreEqual(1, UnityObjectPool.CountAllPooled());

            UnityObjectPool.Recycle(spawned2);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_prefabs[1]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[1]));
            Assert.AreEqual(2, UnityObjectPool.CountAllPooled());
        }

        [Test]
        public void SpawnAndRecycleMultipleObject()
        {
            var spawned1 = UnityObjectPool.Spawn(_prefabs[0]);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[0]));

            var spawned2 = UnityObjectPool.Spawn(_prefabs[0]);
            Assert.AreEqual(2, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[0]));

            UnityObjectPool.Recycle(spawned1);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[0]));

            var spawned3 = UnityObjectPool.Spawn(_prefabs[0]);
            Assert.AreEqual(2, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(0, UnityObjectPool.CountPooled(_prefabs[0]));

            UnityObjectPool.Recycle(spawned2);
            Assert.AreEqual(1, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(1, UnityObjectPool.CountPooled(_prefabs[0]));

            UnityObjectPool.Recycle(spawned3);
            Assert.AreEqual(0, UnityObjectPool.CountSpawned(_prefabs[0]));
            Assert.AreEqual(2, UnityObjectPool.CountPooled(_prefabs[0]));
        }

        [Test]
        public void ClearPool()
        {
            UnityObjectPool.CreatePool(_prefabs[0], 1);
            Assert.AreEqual(1, UnityObjectPool.CountAllPooled());
            UnityObjectPool.ClearPool();
            Assert.AreEqual(0, UnityObjectPool.CountAllPooled());
        }
    }
}
