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
            var bind = new UnityComponentBinding<T>(container);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static UnityComponentBinding<T> RebindUnityComponent<T>(this DependencyContainer container, string tag = null) where T : Component
        {
            var bind = new UnityComponentBinding<T>(container);
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

    public sealed class ServiceLocator : MonoBehaviourSingleton<ServiceLocator>
    {
        DependencyContainer _container;
        InitializableManager _initializables;
        bool _requiresGlobalInstall = true;

        public T Resolve<T>(string tag = null, T def = default(T))
        {
            return _container.Resolve<T>(tag, def);
        }

        public List<T> ResolveList<T>(string tag = null)
        {
            return _container.ResolveList<T>(tag);
        }

        public void Install(IInstaller installer)
        {
            CheckInitialization();
            _container.OnPhaseStart(DependencyContainer.InstallationPhase.Install);
            _container.Install(installer);
            _container.OnPhaseEnd();
        }

        public void Install(IInstaller[] installers)
        {
            CheckInitialization();
            _container.OnPhaseStart(DependencyContainer.InstallationPhase.Install);
            _container.Install(installers);
            _container.OnPhaseEnd();
        }

        public void Initialize()
        {
            CheckInitialization();
            _container.OnPhaseStart(DependencyContainer.InstallationPhase.Initialization);
            _initializables.Initialize();
            _container.OnPhaseEnd();
        }

        public void InstallGlobalDependencies()
        {
            _container = new DependencyContainer();
            _initializables = new InitializableManager(_container);

            _container.OnPhaseStart(DependencyContainer.InstallationPhase.Global);
            _container.Bind<GameObject>().ToInstance(gameObject);
            _container.Bind<Transform>().ToGetter<GameObject>((go) => go.transform);

            var globalConfig = GlobalDependencyConfigurer.Load();
            if(globalConfig != null)
            {
                _container.Install(globalConfig);
            }
            else
            {
                Log.e("GlobalDependencyConfigurer asset not found");
            }
            _container.OnPhaseEnd();
            _requiresGlobalInstall = false;
        }

        public void Clear()
        {
            _requiresGlobalInstall = true;
        }

        void CheckInitialization()
        {
            if(_requiresGlobalInstall)
            {
                Dispose();
                InstallGlobalDependencies();
            }
        }

        void Dispose()
        {
            if(_container != null)
            {
                _container.Dispose();
            }

            _container = null;
            _initializables = null;

            foreach(var mb in gameObject.GetComponents<MonoBehaviour>())
            {
                if(!(mb is ServiceLocator))
                {
                    mb.Destroy();
                }
            }
            gameObject.RemoveChildren();
        }

        #region Singleton and Monobehaviour events

        protected override void SingletonAwakened()
        {
            base.SingletonAwakened();
            CheckInitialization();
        }

        protected override void SingletonStarted()
        {
            base.SingletonStarted();
            Initialize();
        }

        void OnLevelWasLoaded(int level)
        {
            // After a Clear, wait to Initialize in a scene with a DependencyConfigurer.
            if(!_requiresGlobalInstall)
            {
                Initialize();
            }
        }

        #endregion
    }
}
