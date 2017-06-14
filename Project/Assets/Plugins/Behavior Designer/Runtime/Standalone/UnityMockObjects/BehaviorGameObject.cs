using System;
using System.Collections;
#if !BEHAVIOR_DESIGNER_STANDALONE
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime.Standalone
{
#if !BEHAVIOR_DESIGNER_STANDALONE
    public class BehaviorGameObject : MonoBehaviour
    {
        new public T GetComponent<T>()
        {
            T instance = _container.Get<T>();
            return !object.Equals(instance, default(T)) ? instance : base.GetComponent<T>();
        }

        new public object GetComponent(Type type)
        {
            return _container.Get(type) ?? base.GetComponent(type);
        }
#else
    public class BehaviorGameObject
    {
        public string name = "GameObject";
        public string tag = "untagged";
        public bool enabled = true;

        public BehaviorGameObject gameObject {get { return this; } }

        public BehaviorGameObject()
        {}

        public BehaviorGameObject(string name)
        {
            this.name = name;
        }

        public virtual int GetInstanceID()
        {
            return GetHashCode();
        }

        public virtual Coroutine StartCoroutine(string coroutineName)
        {
            throw new NotImplementedException(string.Format("{0} StartCoroutine is not implemented in Standalone version", GetType()));
        }

        public virtual Coroutine StartCoroutine(IEnumerator coroutine)
        {
            throw new NotImplementedException(string.Format("{0} StartCoroutine is not implemented in Standalone version", GetType()));
        }

        public virtual void StopAllCoroutines()
        {
            throw new NotImplementedException(string.Format("{0} StopAllCoroutines is not implemented in Standalone version", GetType()));
        }

        public virtual void StopCoroutine(string coroutineName)
        {
            throw new NotImplementedException(string.Format("{0} StopCoroutine is not implemented in Standalone version", GetType()));
        }

        public T GetComponent<T>()
        {
            return _container.Get<T>();
        }

        public object GetComponent(Type type)
        {
            return _container.Get(type);
        }
#endif
        Container _container = new Container();

        public T AddComponent<T>(T instance)
        {
            if (instance == null)
            {
                return instance;
            }
            _container.Set(instance.GetType().Name, instance);
            return instance;
        }

        public T AddComponent<T>()
        {
            T instance = System.Activator.CreateInstance<T>();
            return AddComponent(instance);
        }
    }
}
