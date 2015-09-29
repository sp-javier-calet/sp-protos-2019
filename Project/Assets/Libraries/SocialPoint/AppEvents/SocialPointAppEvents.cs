using System;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.AppEvents
{
    public class SocialPointAppEvents : IAppEvents
    {
        BaseAppEvents _appEvents;
        const string GameObjectName = "SocialPointAppEvents";

        public SocialPointAppEvents(MonoBehaviour behaviour=null)
        {
            if(behaviour != null)
            {
                Setup(behaviour.gameObject);
            }
            else
            {
                Setup(null);
            }
        }

        ~SocialPointAppEvents()
        {
            DestroyAppEvents();
        }

        public void Dispose()
        {
            DestroyAppEvents();
        }

        private void Setup(GameObject go)
        {
            if(go == null)
            {
                go = new GameObject();
                go.name = GameObjectName;
                GameObject.DontDestroyOnLoad(go);
            }
            DestroyAppEvents();
            #if UNITY_IOS
            _appEvents = go.AddComponent<IosAppEvents>();
            #elif UNITY_ANDROID
            _appEvents = go.AddComponent<AndroidAppEvents>();
            #else
            _appEvents = go.AddComponent<UnityAppEvents>();
            #endif
        }

        private void DestroyAppEvents()
        {
            if(_appEvents != null)
            {
                MonoBehaviour.Destroy(_appEvents);
                _appEvents = null;
            }
        }

        #region IAppEvents implementation

        public void TriggerMemoryWarning()
        {
            _appEvents.TriggerMemoryWarning();
        }
        
        public void TriggerWillGoBackground()
        {
            _appEvents.TriggerWillGoBackground();
        }

        public void RegisterWillGoBackground(int priority, Action action)
        {
            _appEvents.RegisterWillGoBackground(priority, action);
        }

        public void UnregisterWillGoBackground(Action action)
        {
            _appEvents.UnregisterWillGoBackground(action);
        }

        public event Action WasOnBackground
        {
            add
            {
                _appEvents.WasOnBackground += value;
            }
            remove
            {
                _appEvents.WasOnBackground -= value;
            }
        }

        public event Action WasCovered
        {
            add
            {
                _appEvents.WasCovered += value;
            }
            remove
            {
                _appEvents.WasOnBackground -= value;
            }
        }

        public event Action ReceivedMemoryWarning
        {
            add
            {
                _appEvents.ReceivedMemoryWarning += value;
            }
            remove
            {
                _appEvents.ReceivedMemoryWarning -= value;
            }
        }

        public AppSource Source
        {
            get
            {
                return _appEvents.Source;
            }
        }

        public event Action<AppSource> OpenedFromSource
        {
            add
            {
                _appEvents.OpenedFromSource += value;
            }
            remove
            {
                _appEvents.OpenedFromSource -= value;
            }
        }

        public event Action ApplicationQuit
        {
            add
            {
                _appEvents.ApplicationQuit += value;
            }
            remove
            {
                _appEvents.ApplicationQuit -= value;
            }
        }

        public event Action<int> LevelWasLoaded
        {
            add
            {
                _appEvents.LevelWasLoaded += value;
            }
            remove
            {
                _appEvents.LevelWasLoaded -= value;
            }
        }

        #endregion
    }
}

