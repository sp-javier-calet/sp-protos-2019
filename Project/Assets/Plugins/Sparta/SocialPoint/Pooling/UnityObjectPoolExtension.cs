using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Pooling
{
    public static class ObjectPoolExtensions
    {
        public static void CreatePool<T>(this T prefab) where T : Component
        {
            UnityObjectPool.CreatePool(prefab, 0);
        }
    
        public static void CreatePool<T>(this T prefab, int initialPoolSize) where T : Component
        {
            UnityObjectPool.CreatePool(prefab, initialPoolSize);
        }
    
        public static void CreatePool(this GameObject prefab)
        {
            UnityObjectPool.CreatePool(prefab, 0);
        }
    
        public static void CreatePool(this GameObject prefab, int initialPoolSize)
        {
            UnityObjectPool.CreatePool(prefab, initialPoolSize);
        }
    
        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, parent, position, rotation);
        }
    
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, null, position, rotation);
        }
    
        public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
    
        public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
    
        public static T Spawn<T>(this T prefab, Transform parent) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
    
        public static T Spawn<T>(this T prefab) where T : Component
        {
            return UnityObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }
    
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            return UnityObjectPool.Spawn(prefab, parent, position, rotation);
        }
    
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return UnityObjectPool.Spawn(prefab, null, position, rotation);
        }
    
        public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
        {
            return UnityObjectPool.Spawn(prefab, parent, position, Quaternion.identity);
        }
    
        public static GameObject Spawn(this GameObject prefab, Vector3 position)
        {
            return UnityObjectPool.Spawn(prefab, null, position, Quaternion.identity);
        }
    
        public static GameObject Spawn(this GameObject prefab, Transform parent)
        {
            return UnityObjectPool.Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }
    
        public static GameObject Spawn(this GameObject prefab)
        {
            return UnityObjectPool.Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }
    
        public static void Recycle<T>(this T obj) where T : Component
        {
            UnityObjectPool.Recycle(obj);
        }
    
        public static void Recycle(this GameObject obj)
        {
            UnityObjectPool.Recycle(obj);
        }

        public static void Recycle(this GameObject obj, bool keepWorldScale)
        {
            UnityObjectPool.Recycle(obj, keepWorldScale);
        }

        public static void RecycleAll<T>(this T prefab) where T : Component
        {
            UnityObjectPool.RecycleAll(prefab);
        }
    
        public static void RecycleAll(this GameObject prefab)
        {
            UnityObjectPool.RecycleAll(prefab);
        }
    
        public static int CountPooled<T>(this T prefab) where T : Component
        {
            return UnityObjectPool.CountPooled(prefab);
        }
    
        public static int CountPooled(this GameObject prefab)
        {
            return UnityObjectPool.CountPooled(prefab);
        }
    
        public static int CountSpawned<T>(this T prefab) where T : Component
        {
            return UnityObjectPool.CountSpawned(prefab);
        }
    
        public static int CountSpawned(this GameObject prefab)
        {
            return UnityObjectPool.CountSpawned(prefab);
        }
    
        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return UnityObjectPool.GetSpawned(prefab, list, appendList);
        }
    
        public static List<GameObject> GetSpawned(this GameObject prefab, List<GameObject> list)
        {
            return UnityObjectPool.GetSpawned(prefab, list, false);
        }
    
        public static List<GameObject> GetSpawned(this GameObject prefab)
        {
            return UnityObjectPool.GetSpawned(prefab, null, false);
        }
    
        public static List<T> GetSpawned<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return UnityObjectPool.GetSpawned(prefab, list, appendList);
        }
    
        public static List<T> GetSpawned<T>(this T prefab, List<T> list) where T : Component
        {
            return UnityObjectPool.GetSpawned(prefab, list, false);
        }
    
        public static List<T> GetSpawned<T>(this T prefab) where T : Component
        {
            return UnityObjectPool.GetSpawned(prefab, null, false);
        }
    
        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list, bool appendList)
        {
            return UnityObjectPool.GetPooled(prefab, list, appendList);
        }
    
        public static List<GameObject> GetPooled(this GameObject prefab, List<GameObject> list)
        {
            return UnityObjectPool.GetPooled(prefab, list, false);
        }
    
        public static List<GameObject> GetPooled(this GameObject prefab)
        {
            return UnityObjectPool.GetPooled(prefab, null, false);
        }
    
        public static List<T> GetPooled<T>(this T prefab, List<T> list, bool appendList) where T : Component
        {
            return UnityObjectPool.GetPooled(prefab, list, appendList);
        }
    
        public static List<T> GetPooled<T>(this T prefab, List<T> list) where T : Component
        {
            return UnityObjectPool.GetPooled(prefab, list, false);
        }
    
        public static List<T> GetPooled<T>(this T prefab) where T : Component
        {
            return UnityObjectPool.GetPooled(prefab, null, false);
        }
    
        public static void DestroyPooled(this GameObject prefab)
        {
            UnityObjectPool.DestroyPooled(prefab);
        }
    
        public static void DestroyPooled<T>(this T prefab) where T : Component
        {
            UnityObjectPool.DestroyPooled(prefab.gameObject);
        }
    
        public static void DestroyAll(this GameObject prefab)
        {
            UnityObjectPool.DestroyAll(prefab);
        }
    
        public static void DestroyAll<T>(this T prefab) where T : Component
        {
            UnityObjectPool.DestroyAll(prefab.gameObject);
        }
    }
}
