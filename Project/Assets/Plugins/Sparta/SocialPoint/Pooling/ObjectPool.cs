using System;
using System.Collections.Generic;

namespace SocialPoint.Pooling
{
    public class ObjectPool
    {
        Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();
        Dictionary<int, object> idPoolDictionary = new Dictionary<int, object>();

        #region typepool
        public T Get<T>(Func<T> fallback = null)
        {
            var objType = typeof(T);
            object pooledObjects = null;

            if(poolDictionary.TryGetValue(objType, out pooledObjects))
            {
                var pooledObjectsCasted = (Stack<T>)pooledObjects;
                if(pooledObjectsCasted.Count > 0)
                {
                    return pooledObjectsCasted.Pop();
                }
            }

            return fallback != null ? fallback() : (T)CreateInstance(objType);
        }

        public void Return<T>(T obj)
        {
            DoReturnByType(obj);
        }

        public void Return<T>(List<T> list)
        {
            list.Clear();
            DoReturnByType(list);
        }

        void DoReturnByType<T>(T obj)
        {
            if(obj == null)
            {
                return;
            }

            var objType = typeof(T);
            object pooledObjects = null;
            Stack<T> pooledObjectsCasted = null;

            if(!poolDictionary.TryGetValue(objType, out pooledObjects))
            {
                pooledObjectsCasted = new Stack<T>();
                poolDictionary.Add(objType, pooledObjectsCasted);
            }
            else
            {
                pooledObjectsCasted = (Stack<T>)pooledObjects;
            }

            pooledObjectsCasted.Push(obj);
        }
        #endregion typepool

        #region idpool
        public T Get<T>(int id, Func<T> fallback = null)
        {
            var objType = typeof(T);
            object pooledObjects = null;

            if(idPoolDictionary.TryGetValue(id, out pooledObjects))
            {
                var pooledObjectsCasted = (Stack<T>)pooledObjects;
                if(pooledObjectsCasted.Count > 0)
                {
                    return pooledObjectsCasted.Pop();
                }
            }

            return fallback != null ? fallback() : (T)CreateInstance(objType);
        }

        public void Return<T>(int id, List<T> list)
        {
            list.Clear();
            DoReturnById(id, list);
        }

        public void Return<T>(int id, T obj)
        {
            DoReturnById(id, obj);
        }

        void DoReturnById<T>(int id, T obj)
        {
            if(obj == null)
            {
                return;
            }

            object pooledObjects = null;
            Stack<T> pooledObjectsCasted = null;

            if(!idPoolDictionary.TryGetValue(id, out pooledObjects))
            {
                pooledObjectsCasted = new Stack<T>();
                idPoolDictionary.Add(id, pooledObjectsCasted);
            }
            else
            {
                pooledObjectsCasted = (Stack<T>)pooledObjects;
            }

            pooledObjectsCasted.Push(obj);
        }
        #endregion idpool

        object CreateInstance(Type t)
        {
            return Activator.CreateInstance(t);
        }

        public void Clear()
        {
            poolDictionary.Clear();
            idPoolDictionary.Clear();
        }
    }
}