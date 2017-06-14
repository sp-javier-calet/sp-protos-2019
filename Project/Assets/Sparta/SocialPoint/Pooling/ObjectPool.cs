﻿using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Pooling
{
    public sealed class ObjectPool : MonoBehaviour
    {
        public enum StartupPoolModeEnum
        {
            Awake,
            Start,
            CallManually
        }

        [System.Serializable]
        public sealed class StartupPool
        {
            public int size;
            public GameObject prefab;
        }

        public bool AllowAutoPoolCreation;
        public StartupPoolModeEnum StartupPoolMode;
        public StartupPool[] StartupPools;

        static ObjectPool _instance;

        public static ObjectPool Instance
        {
            get
            {
                if(_instance != null)
                {
                    return _instance;
                }
            
                _instance = Object.FindObjectOfType<ObjectPool>();
                if(_instance != null)
                {
                    return _instance;
                }
            
                var go = new GameObject("ObjectPool");
                _instance = go.AddComponent<ObjectPool>();
                return _instance;
            }
        }

        static List<GameObject> _recycleList = new List<GameObject>();
        Dictionary<GameObject, List<GameObject>> _pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> _spawnedObjects = new Dictionary<GameObject, GameObject>();
        HashSet<GameObject> _nonPulledPrefabs = new HashSet<GameObject>();
        bool startupPoolsCreated;

        void Awake()
        {
            _instance = this;
            if(StartupPoolMode == StartupPoolModeEnum.Awake)
            {
                CreateStartupPools();
            }
        }

        void Start()
        {
            if(StartupPoolMode == StartupPoolModeEnum.Start)
            {
                CreateStartupPools();
            }
        }

        public static void CreateStartupPools()
        {
            if(!Instance.startupPoolsCreated)
            {
                Instance.startupPoolsCreated = true;
                var pools = Instance.StartupPools;
                if(pools != null && pools.Length > 0)
                {
                    for(int i = 0; i < pools.Length; ++i)
                    {
                        CreatePool(pools[i].prefab, pools[i].size);
                    }
                }
            }
        }

        bool GetOrCreatePool(GameObject prefab, out List<GameObject> list)
        {
            bool found = Instance._pooledObjects.TryGetValue(prefab, out list);
            if(!found && AllowAutoPoolCreation)
            {
                list = CreatePool(prefab, 1);
                found = list != null;
            }
            return found;
        }

        public static List<GameObject> CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            return CreatePool(prefab.gameObject, initialPoolSize);
        }

        public static List<GameObject> CreatePool(GameObject prefab, int initialPoolSize)
        {
            List<GameObject> list = null;

            if(prefab == null || initialPoolSize == 0)
            {
                return null;
            }

            if(!Instance._pooledObjects.ContainsKey(prefab))
            {
                list = new List<GameObject>();
                Instance._pooledObjects.Add(prefab, list);

                bool active = prefab.activeSelf;
                prefab.SetActive(false);
                Transform parent = new GameObject().transform;
                parent.parent = _instance.transform;
                parent.name = "Pool_" + prefab.name;
                while(list.Count < initialPoolSize)
                {
                    var obj = Object.Instantiate(prefab);
                    SetupTransform(parent, Vector3.zero, Quaternion.identity, obj);
                    list.Add(obj);
                }
                prefab.SetActive(active);
            }

            return list;
        }

        public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
        }

        public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
        }

        public static T Spawn<T>(T prefab, Vector3 position) where T : Component
        {
            return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
        }

        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if(!prefab)
            {
                return null;
            }

            GameObject obj;
            List<GameObject> list;

            if(Instance.GetOrCreatePool(prefab, out list))
            {
                obj = null;
                if(list.Count > 0)
                {
                    while(obj == null && list.Count > 0)
                    {
                        obj = list[0];
                        list.RemoveAt(0);
                    }
                }
                GameObject spawnedObj = CreateObject(prefab, parent, position, rotation, obj, true);
                
                // Spawn 'notification'
                Component[] recyclables = spawnedObj.GetComponents(typeof(IRecyclable));
                for(int i = 0; i < recyclables.Length; ++i)
                {
                    ((IRecyclable)recyclables[i]).OnSpawn();
                }
                return spawnedObj;
            }
            LogWarningSpawningPrefabNotInPool(prefab);
            return CreateObject(prefab, parent, position, rotation, null, false);
        }

        static GameObject CreateObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, GameObject obj, bool addToSpawnedObjects)
        {
            if(obj == null)
            {
                obj = Object.Instantiate(prefab);
            }
            SetupTransform(parent, position, rotation, obj);
            SetupParticleSystems(obj);
            if(addToSpawnedObjects)
            {
                Instance._spawnedObjects.Add(obj, prefab);
            }
            return obj;
        }

        static void SetupTransform(Transform parent, Vector3 position, Quaternion rotation, GameObject obj)
        {
            obj.transform.SetParent(parent);
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
        }

        static void SetupParticleSystems(GameObject obj)
        {
            ParticleSystem[] m_particleSystems = obj.GetComponentsInChildren<ParticleSystem>(true);

            for(int j = 0; j < m_particleSystems.Length; ++j)
            {
                m_particleSystems[j].Stop();
                var emission = m_particleSystems[j].emission;
                emission.enabled = false;
            }

            for(int j = 0; j < m_particleSystems.Length; ++j)
            {
                m_particleSystems[j].Play();
                var emission = m_particleSystems[j].emission;
                emission.enabled = true;
            }
            obj.SetActive(false);
            obj.SetActive(true);
        }

        static void LogWarningSpawningPrefabNotInPool(GameObject nonPooledPrefab)
        {
            if(!Instance._nonPulledPrefabs.Contains(nonPooledPrefab))
            {
                Instance._nonPulledPrefabs.Add(nonPooledPrefab);
                Log.w("ObjectPool: " + nonPooledPrefab.name);
            }
        }

        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
        {
            return Spawn(prefab, parent, position, Quaternion.identity);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, null, position, rotation);
        }

        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            return Spawn(prefab, null, position, Quaternion.identity);
        }

        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }

        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }

        public static void Recycle(GameObject obj)
        {
            if(!Instance || obj == null)
            {           
                SocialPoint.Base.DebugUtils.Assert(obj != null, "ObjectPool: Trying to recycle null object");
                return;
            }

            GameObject prefab;
            if(Instance._spawnedObjects.TryGetValue(obj, out prefab))
            {
            
                // Recycle 'notification'
                Component[] recyclables = obj.GetComponents(typeof(IRecyclable));
                for(int i = 0; i < recyclables.Length; ++i)
                {
                    ((IRecyclable)recyclables[i]).OnRecycle();
                }

                // Recycle itself
                Recycle(obj, prefab);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        static void Recycle(GameObject obj, GameObject prefab)
        {
            Instance._pooledObjects[prefab].Add(obj);
            Instance._spawnedObjects.Remove(obj);
            Transform _parent = Instance.gameObject.transform.FindChild("Pool_" + prefab.name).transform;
            if(obj)
            {
                SetupTransform(_parent ? _parent : _instance.transform, Vector3.zero, Quaternion.identity, obj);
                obj.SetActive(false);
            }
        }

        public static void RecycleAll<T>(T prefab) where T : Component
        {
            RecycleAll(prefab.gameObject);
        }

        public static void RecycleAll(GameObject prefab)
        {
            var itr = Instance._spawnedObjects.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                if(item.Value == prefab)
                {
                    _recycleList.Add(item.Key);
                }
            }
            itr.Dispose();

            for(int i = 0; i < _recycleList.Count; ++i)
            {
                Recycle(_recycleList[i]);
            }
            _recycleList.Clear();
        }

        public static void RecycleAll()
        {
            _recycleList.AddRange(Instance._spawnedObjects.Keys);
            for(int i = 0; i < _recycleList.Count; ++i)
            {
                Recycle(_recycleList[i]);
            }
            _recycleList.Clear();
        }

        public static bool IsSpawned(GameObject obj)
        {
            return Instance._spawnedObjects.ContainsKey(obj);
        }

        public static int CountPooled<T>(T prefab) where T : Component
        {
            return CountPooled(prefab.gameObject);
        }

        public static int CountPooled(GameObject prefab)
        {
            List<GameObject> list;
            return Instance._pooledObjects.TryGetValue(prefab, out list) ? list.Count : 0;
        }

        public static int CountSpawned<T>(T prefab) where T : Component
        {
            return CountSpawned(prefab.gameObject);
        }

        public static int CountSpawned(GameObject prefab)
        {
            int count = 0;
            var itr = Instance._spawnedObjects.Values.GetEnumerator();
            while(itr.MoveNext())
            {
                var instancePrefab = itr.Current;
                if(prefab == instancePrefab)
                {
                    ++count;
                }
            }
            itr.Dispose();
            return count;
        }

        public static int CountAllPooled()
        {
            int count = 0;
            var itr = Instance._pooledObjects.Values.GetEnumerator();
            while(itr.MoveNext())
            {
                var list = itr.Current;
                count += list.Count;
            }
            itr.Dispose();
            return count;
        }

        public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if(list == null)
            {
                list = new List<GameObject>();
            }
            if(!appendList)
            {
                list.Clear();
            }
            List<GameObject> pooled;
            if(Instance._pooledObjects.TryGetValue(prefab, out pooled))
            {
                list.AddRange(pooled);
            }
            return list;
        }

        public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if(list == null)
            {
                list = new List<T>();
            }
            if(!appendList)
            {
                list.Clear();
            }
            List<GameObject> pooled;
            if(Instance._pooledObjects.TryGetValue(prefab.gameObject, out pooled))
            {
                for(int i = 0; i < pooled.Count; ++i)
                {
                    list.Add(pooled[i].GetComponent<T>());
                }
            }
            return list;
        }

        public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if(list == null)
            {
                list = new List<GameObject>();
            }
            if(!appendList)
            {
                list.Clear();
            }
            var itr = Instance._spawnedObjects.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                if(item.Value == prefab)
                {
                    list.Add(item.Key);
                }
            }
            itr.Dispose();
            return list;
        }

        public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if(list == null)
            {
                list = new List<T>();
            }
            if(!appendList)
            {
                list.Clear();
            }
            var prefabObj = prefab.gameObject;
            var itr = Instance._spawnedObjects.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                if(item.Value == prefabObj)
                {
                    list.Add(item.Key.GetComponent<T>());
                }
            }
            itr.Dispose();
            return list;
        }

        public static void DestroyPooled(GameObject prefab)
        {
            List<GameObject> pooled;
            if(Instance._pooledObjects.TryGetValue(prefab, out pooled))
            {
                for(int i = 0; i < pooled.Count; ++i)
                {
                    GameObject.Destroy(pooled[i]);
                }
                pooled.Clear();
            }
        }

        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        public static void DestroyAll(GameObject prefab)
        {
            RecycleAll(prefab);
            DestroyPooled(prefab);
        }

        public static void DestroyAll<T>(T prefab) where T : Component
        {
            DestroyAll(prefab.gameObject);
        }
    }
}
