using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.Pooling
{
    public class UnityObjectPool : MonoBehaviourSingleton<UnityObjectPool>
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

        const string PoolPrefix = "Pool_";
            
        public StartupPoolModeEnum StartupPoolMode;
        public StartupPool[] StartupPools;

        static List<GameObject> _recycleList = new List<GameObject>();
        Dictionary<GameObject, List<GameObject>> _pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> _spawnedObjects = new Dictionary<GameObject, GameObject>();
        bool startupPoolsCreated;

        #region Singleton and Monobehaviour events

        protected override void SingletonAwakened()
        {
            base.SingletonAwakened();
            if(StartupPoolMode == StartupPoolModeEnum.Awake)
            {
                CreateStartupPools();
            }
        }

        protected override void SingletonStarted()
        {
            base.SingletonStarted();
            if(StartupPoolMode == StartupPoolModeEnum.Start)
            {
                CreateStartupPools();
            }
        }

        #endregion

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
            if(!found)
            {
                list = CreatePool(prefab, 1);
                found = list != null;

                LogWarningSpawningPrefabNotInPool(prefab);
            }
            return found;
        }

        public static List<GameObject> CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            return CreatePool(prefab.gameObject, initialPoolSize);
        }

        static IEnumerator HidePrefab(GameObject prefab, bool active, Vector3 position)
        {
            Camera camera = Camera.main ?? Camera.allCameras[0];

            // We force to upload a model to GPU in order to prevent performance
            // spikes when a model is first visible. We place it in middle of the
            // camera frustum to avoid culling.
            float frustumMidPoint = (camera.farClipPlane - camera.nearClipPlane) * 0.5f;
            prefab.transform.position = camera.transform.position + camera.transform.forward * frustumMidPoint;
            prefab.SetActive(true);

            yield return 0;

            // We set back the previous values.
            prefab.transform.position = position;
            prefab.SetActive(active);
        }

        public static List<GameObject> CreatePool(GameObject prefab, int initialPoolSize)
        {
            List<GameObject> list = null;

            if(prefab == null || initialPoolSize == 0)
            {
                return null;
            }

            bool active = prefab.activeSelf;
            Transform parent; 
            if(!Instance._pooledObjects.ContainsKey(prefab))
            {
                list = new List<GameObject>();
                Instance._pooledObjects.Add(prefab, list);

                parent = new GameObject().transform;
                parent.SetParent(Instance.transform);
                parent.name = string.Concat(PoolPrefix, prefab.name);
            }
            else
            {
                list = Instance._pooledObjects[prefab];
                initialPoolSize += list.Count;

                parent = FindChildPoolGameObject(prefab);
            }

            prefab.SetActive(false);
            while(list.Count < initialPoolSize)
            {
                var obj = Object.Instantiate(prefab);
                SetupTransform(parent ?? Instance.transform, Vector3.zero, Quaternion.identity, obj);

                list.Add(obj);
            }
            Instance.StartCoroutine(HidePrefab(prefab, active, prefab.transform.position));

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

        static void SetupTransform(Transform parent, Vector3 position, Quaternion rotation, GameObject obj, bool keepWorldScale = true)
        {
            obj.transform.SetParent(parent, keepWorldScale);
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;

            if(parent == null)
            {
                SceneManager.MoveGameObjectToScene(obj, SceneManager.GetActiveScene());
            }
        }

        static void SetupParticleSystems(GameObject obj)
        {
            ParticleSystem[] m_particleSystems = obj.GetComponentsInChildren<ParticleSystem>(true);

            for(int j = 0; j < m_particleSystems.Length; ++j)
            {
                ParticleSystem particleSystem = m_particleSystems[j];

                if(particleSystem.gameObject.activeSelf)
                {
                    particleSystem.Stop();
                    particleSystem.Clear();
                    var emission = particleSystem.emission;
                    emission.enabled = false;
                }
            }

            for(int j = 0; j < m_particleSystems.Length; ++j)
            {
                ParticleSystem particleSystem = m_particleSystems[j];

                if(particleSystem.gameObject.activeSelf)
                {
                    particleSystem.Play();
                    var emission = particleSystem.emission;
                    emission.enabled = true;
                }
            }
            obj.SetActive(false);
            obj.SetActive(true);
        }

        static void LogWarningSpawningPrefabNotInPool(GameObject nonPooledPrefab)
        {
            Log.w("UnityObjectPool - created not existing pooled prefab: " + nonPooledPrefab.name);
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
            Recycle(obj, true);
        }
            
        public static void Recycle(GameObject obj, bool keepWorldScale)
        {
            if(!Instance || obj == null)
            {           
                DebugUtils.Assert(obj != null, "ObjectPool: Trying to recycle null object");
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
                Recycle(obj, prefab, keepWorldScale);
            }
            else
            {
                obj.DestroyAnyway();
            }
        }

        static void Recycle(GameObject obj, GameObject prefab, bool keepWorldScale)
        {
            if(Instance._pooledObjects.ContainsKey(prefab))
            {
                Instance._pooledObjects[prefab].Add(obj);
                Instance._spawnedObjects.Remove(obj);

                Transform parent = FindChildPoolGameObject(prefab);

                if(obj)
                {
                    SetupTransform(parent ?? Instance.transform, Vector3.zero, Quaternion.identity, obj, keepWorldScale);
                    obj.SetActive(false);
                }
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

            Instance._spawnedObjects.Clear();
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

        public static bool IsPooled(GameObject obj)
        {
            return Instance._pooledObjects.ContainsKey(obj);
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

        static void DestroyPooled(GameObject prefab, bool removePool)
        {
            List<GameObject> pooled;
            if(Instance._pooledObjects.TryGetValue(prefab, out pooled))
            {
                for(int i = 0; i < pooled.Count; ++i)
                {
                    pooled[i].DestroyAnyway();
                }
                pooled.Clear();
                if(removePool)
                {
                    Instance._pooledObjects.Remove(prefab);
                }
            }
        }

        public static void DestroyPooled(GameObject prefab)
        {
            DestroyPooled(prefab, true);
        }

        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        static void DestroyAll(GameObject prefab, bool removePool)
        {
            RecycleAll(prefab);
            DestroyPooled(prefab, removePool);

            ClearPoolRef(prefab);
        }

        public static void DestroyAll(GameObject prefab)
        {
            DestroyAll(prefab, true);
        }

        static void ClearPoolRef(GameObject prefab)
        {
            var poolRef = FindChildPoolGameObject(prefab);
            if(poolRef != null)
            {
                poolRef.gameObject.DestroyAnyway();
            }
        }

        public static Transform FindChildPoolGameObject(GameObject prefab)
        {
            return Instance.transform.FindChild(string.Concat(PoolPrefix, prefab.name));
        }

        public static void DestroyAll<T>(T prefab) where T : Component
        {
            DestroyAll(prefab.gameObject);
        }

        public static void ClearPool()
        {
            var itr = Instance._pooledObjects.GetEnumerator();
            while(itr.MoveNext())
            {
                DestroyAll(itr.Current.Key, false);
            }
            itr.Dispose();
        }
    }
}
            