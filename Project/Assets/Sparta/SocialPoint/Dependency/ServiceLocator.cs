using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public sealed class UnityComponentBinding<F> : IBinding where F : Component
    {
        DependencyContainer _container;
        Action<F> _setup;
        F _instance;

        public UnityComponentBinding(DependencyContainer container)
        {
            _container = container;
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

        public void OnResolutionFinished()
        {
            if(_setup != null && _instance != null)
            {
                _setup(_instance);
                _setup = null;
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
            var bind = new UnityComponentBinding<T>(container);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static UnityComponentBinding<T> RebindUnityComponent<T>(this DependencyContainer container, string tag = null) where T : Component
        {
            container.Remove<T>(tag);
            return container.BindUnityComponent<T>(tag);
        }
    }

    public sealed class ServiceLocator : MonoBehaviourSingleton<ServiceLocator>
    {
        DependencyContainer _container;
        InitializableManager _initializables;

        public T Resolve<T>(string tag=null, T def=default(T))
        {
            return _container.Resolve<T>(tag, def);
        }

        public List<T> ResolveList<T>(string tag=null)
        {
            return _container.ResolveList<T>(tag);
        }

        public void Install(IInstaller installer)
        {
            _container.Install(installer);
        }

        public void Install(IInstaller[] installers)
        {
            _container.Install(installers);
        }

        public void Initialize()
        {
            _initializables.Initialize();
        }

        const string GlobalInstallersResource = "GlobalDependencyConfigurer";

        override protected void SingletonAwakened()
        {
            base.SingletonAwakened();

            _container = new DependencyContainer();
            _initializables = new InitializableManager(_container);

            _container.Bind<GameObject>().ToInstance(gameObject);
            _container.Bind<Transform>().ToGetter<GameObject>((go) => go.transform);

            var globalConfig = Resources.Load<GlobalDependencyConfigurer>(GlobalInstallersResource);
            if(globalConfig != null)
            {
                Install(globalConfig.Installers);
            }
        }
            
        protected override void SingletonStarted()
        {
            base.SingletonStarted();
            Initialize();
        }

        void OnLevelWasLoaded(int level)
        {
            Initialize();
        }
    }
}
