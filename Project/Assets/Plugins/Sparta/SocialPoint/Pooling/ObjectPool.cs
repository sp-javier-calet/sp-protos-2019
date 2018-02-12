using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Pooling
{
    public class ObjectPool : IDisposable
    {
        Dictionary<Type, Stack<object>> _types = new Dictionary<Type, Stack<object>>();
        Dictionary<int, Stack<object>> _ids = new Dictionary<int, Stack<object>>();
        Func<Type, object> _createDelegate;

        void OnSpawned(object obj)
        {
            if(obj == null)
            {
                return;
            }
            var recycleObj = obj as IRecyclable;
            if(recycleObj != null)
            {
                recycleObj.OnSpawn();
            }
        }

        void OnRecycled(object obj)
        {
            if(obj == null)
            {
                return;
            }
            var recycle = obj as IRecyclable;
            if(recycle != null)
            {
                recycle.OnRecycle();
            }
            var list = obj as IList;
            if(list != null)
            {
                list.Clear();
            }
            var dict = obj as IDictionary;
            if(dict != null)
            {
                dict.Clear();
            }
        }

        void OnDisposed(object obj)
        {
            var dispose = obj as IDisposable;
            if(dispose != null)
            {
                dispose.Dispose();
            }
        }

        #region typepool

        public T Get<T>() where T : class, new()
        {
            var obj = DoGet<T>();
            if(obj == null)
            {
                obj = CreateInstance<T>();
            }
            OnSpawned(obj);
            return obj;
        }

        public T Get<T>(Func<T> fallback) where T : class
        {
            var obj = DoGet<T>();
            if(obj == null && fallback != null)
            {
                obj = fallback();
            }
            OnSpawned(obj);
            return obj;
        }

        public T TryGet<T>() where T : class
        {
            var obj = DoGet<T>();
            OnSpawned(obj);
            return obj;
        }

        T DoGet<T>() where T : class
        {
            var objType = typeof(T);
            Stack<object> pooledObjects = null;
            T obj = null;
            if(_types != null && _types.TryGetValue(objType, out pooledObjects))
            {
                if(pooledObjects.Count > 0)
                {
                    obj = (T)pooledObjects.Pop();
                }
            }
            return obj;
        }

        public void Return(object obj)
        {
            if(obj == null || _types == null)
            {
                return;
            }

            var objType = obj.GetType();
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

        public T Get<T>(int id) where T : class, new()
        {
            T obj = DoGet<T>(id);
            if(obj == null)
            {
                obj = CreateInstance<T>();
            }
            OnSpawned(obj);
            return obj;
        }

        public T Get<T>(int id, Func<T> fallback) where T : class
        {
            T obj = DoGet<T>(id);
            if(obj == null && fallback != null)
            {
                obj = fallback();
            }
            OnSpawned(obj);
            return obj;
        }

        public T TryGet<T>(int id) where T : class
        {
            T obj = DoGet<T>(id);
            OnSpawned(obj);
            return obj;
        }

        T DoGet<T>(int id) where T : class
        {
            Stack<object> pooledObjects = null;
            T obj = null;
            if(_ids != null && _ids.TryGetValue(id, out pooledObjects))
            {
                if(pooledObjects.Count > 0)
                {
                    obj = (T)pooledObjects.Pop();
                }
            }
            return obj;
        }


        public void Return(int id, object obj)
        {
            if(obj == null || _ids == null)
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

        public void RegisterCreationDelegate(Func<Type, object> dlg)
        {
            _createDelegate = dlg;
        }

        T CreateInstance<T>() where T : class
        {
            var type = typeof(T);
            T obj = null;
            if(obj == null && _createDelegate != null)
            {
                obj = _createDelegate(type) as T;
            }
            if(obj == null)
            {
                obj = Activator.CreateInstance(typeof(T)) as T;
            }
            return obj;
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
                var types = _types;
                _types = null;
                if(types != null)
                {
                    var itr = types.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        OnStackDisposed(itr.Current.Value);
                    }
                    itr.Dispose();
                    types.Clear();
                }
            }
            {
                var ids = _ids;
                _ids = null;
                if(ids != null)
                {
                    var itr = ids.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        OnStackDisposed(itr.Current.Value);
                    }
                    itr.Dispose();
                    ids.Clear();
                }
            }
        }
    }
}
