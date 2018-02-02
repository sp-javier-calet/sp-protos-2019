using System;
using System.Collections.Generic;

namespace SocialPoint.Pooling
{
    public class ObjectPool : IDisposable
    {
        Dictionary<Type, Stack<object>> _types = new Dictionary<Type, Stack<object>>();
        Dictionary<int, Stack<object>> _ids = new Dictionary<int, Stack<object>>();

        void OnSpawned(object obj)
        {
            var recycleObj = obj as IRecyclable;
            if(recycleObj != null)
            {
                recycleObj.OnSpawn();
            }
        }

        void OnRecycled(object obj)
        {
            var recycleObj = obj as IRecyclable;
            if(recycleObj != null)
            {
                recycleObj.OnRecycle();
            }
        }

        void OnDisposed(object obj)
        {
            var disposeObj = obj as IDisposable;
            if(disposeObj != null)
            {
                disposeObj.Dispose();
            }
        }

        #region typepool

        public T Get<T>(Func<T> fallback = null) where T : class
        {
            var objType = typeof(T);
            Stack<object> pooledObjects = null;
            T obj = null;
            if(_types.TryGetValue(objType, out pooledObjects))
            {
                if(pooledObjects.Count > 0)
                {
                    obj = (T)pooledObjects.Pop();
                }
            }
            if(obj == null)
            {
                obj = fallback != null ? fallback() : (T)CreateInstance(objType);
            }
            OnSpawned(obj);
            return obj;
        }

        public void Return<T>(T obj) where T : class
        {
            DoReturnByType(obj);
        }

        void DoReturnByType<T>(T obj) where T : class
        {
            if(obj == null)
            {
                return;
            }

            var objType = typeof(T);
            Stack<object> pooledObjects = null;

            if(!_types.TryGetValue(objType, out pooledObjects))
            {
                pooledObjects = new Stack<object>();
                _types.Add(objType, pooledObjects);
            }
            OnRecycled(obj);
            pooledObjects.Push(obj);
        }

        #endregion typepool

        #region idpool

        public T Get<T>(int id, Func<T> fallback = null) where T : class
        {
            var objType = typeof(T);
            Stack<object> pooledObjects = null;
            T obj = null;
            if(_ids.TryGetValue(id, out pooledObjects))
            {
                if(pooledObjects.Count > 0)
                {
                    obj = (T)pooledObjects.Pop();
                }
            }
            if(obj == null)
            {
                obj = fallback != null ? fallback() : (T)CreateInstance(objType);
            }
            OnSpawned(obj);
            return obj;
        }

        public void Return<T>(int id, List<T> list) where T : class
        {
            list.Clear();
            DoReturnById(id, list);
        }

        public void Return<T>(int id, T obj) where T : class
        {
            DoReturnById(id, obj);
        }

        void DoReturnById<T>(int id, T obj) where T : class
        {
            if(obj == null)
            {
                return;
            }

            Stack<object> pooledObjects = null;

            if(!_ids.TryGetValue(id, out pooledObjects))
            {
                pooledObjects = new Stack<object>();
                _ids.Add(id, pooledObjects);
            }
            OnRecycled(obj);
            pooledObjects.Push(obj);
        }

        #endregion idpool

        object CreateInstance(Type t)
        {
            return Activator.CreateInstance(t);
        }

        public void Dispose()
        {
            Clear();
        }

        void OnStackDisposed(Stack<Object> stack)
        {
            var itr = stack.GetEnumerator();
            while(itr.MoveNext())
            {
                OnDisposed(itr.Current);
            }
            itr.Dispose();
        }

        public void Clear()
        {
            {
                var itr = _types.GetEnumerator();
                while(itr.MoveNext())
                {
                    OnStackDisposed(itr.Current.Value);
                }
                itr.Dispose();
            }
            {
                var itr = _ids.GetEnumerator();
                while(itr.MoveNext())
                {
                    OnStackDisposed(itr.Current.Value);
                }
                itr.Dispose();
            }
            _types.Clear();
            _ids.Clear();
        }
    }
}