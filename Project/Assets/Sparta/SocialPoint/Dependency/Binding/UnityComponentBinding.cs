using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public sealed class UnityComponentBinding<F> : IBinding where F : Component
    {
        DependencyContainer _container;
        Action<F> _setup;
        F _instance;

        public BindingKey Key
        {
            get
            {
                return new BindingKey(typeof(F), null);
            }
        }

        public bool Resolved { get; private set; }

        public int Priority { get; private set; }

        public UnityComponentBinding(DependencyContainer container, int priority)
        {
            _container = container;
            Priority = priority;
        }

        public UnityComponentBinding<F> WithSetup<T>(Action<F> setup)
        {
            _setup = setup;
            return this;
        }

        public object Resolve()
        {
            if(_instance == null)
            {
                var go = _container.Resolve<GameObject>();
                if(go != null)
                {
                    _instance = go.AddComponent<F>();
                }
            }
            return _instance;
        }

        public void OnResolved()
        {
            Resolved = true;

            if(_setup != null && _instance != null)
            {
                var setup = _setup;
                _setup = null;
                setup(_instance);
            }
        }

        public override string ToString()
        {
            return string.Format("[UnityComponentBinding {0}]", typeof(F));
        }
    }

    public static class DependencyContainerUnityExtensions
    {
        public static UnityComponentBinding<T> BindUnityComponent<T>(this DependencyContainer container, string tag = null) where T : Component
        {
            var bind = new UnityComponentBinding<T>(container, DependencyContainer.NormalBindingPriority);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static UnityComponentBinding<T> RebindUnityComponent<T>(this DependencyContainer container, string tag = null) where T : Component
        {
            var bind = new UnityComponentBinding<T>(container, DependencyContainer.NormalBindingPriority);
            if(!container.HasBinding<T>(tag))
            {
                container.AddBinding(bind, typeof(T), tag);
            }
            else
            {
                Log.w("DependencyContainer", string.Format("Skipping binding of {0} <{1}>", typeof(T).Name, tag ?? string.Empty));
            }
            return bind;
        }
    }
}