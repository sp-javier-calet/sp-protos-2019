using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public sealed class Services : MonoBehaviourSingleton<Services>
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

            var behaviours = gameObject.GetComponents<MonoBehaviour>();
            for(var i = 0; i < behaviours.Length; ++i)
            {
                var mb = behaviours[i];
                if(!(mb is Services))
                {
                    mb.Destroy();
                }
            }
            gameObject.RemoveChildren();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #region Singleton and Monobehaviour events

        protected override void SingletonAwakened()
        {
            base.SingletonAwakened();
            CheckInitialization();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void SingletonStarted()
        {
            base.SingletonStarted();
            Initialize();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
