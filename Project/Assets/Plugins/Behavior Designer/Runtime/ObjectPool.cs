using System;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public class ObjectPool
    {
        private Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();

        public T Get<T>()
        {
            if (poolDictionary.ContainsKey(typeof(T))) {
                var pooledObjects = poolDictionary[typeof(T)] as Stack<T>;
                if (pooledObjects.Count > 0) {
                    return pooledObjects.Pop();
                }
            }
            return (T)TaskUtility.CreateInstance(typeof(T));
        }

        public void Return<T>(T obj)
        {
            if (obj == null) {
                return;
            }
            if (poolDictionary.ContainsKey(typeof(T))) {
                var pooledObjects = poolDictionary[typeof(T)] as Stack<T>;
                pooledObjects.Push(obj);
            } else {
                var pooledObjects = new Stack<T>();
                pooledObjects.Push(obj);
                poolDictionary.Add(typeof(T), pooledObjects);
            }
        }
    }
}