using System;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    public class SocialPointAppEvents : IAppEvents
    {
        AppEventsBase _appEvents;
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
            #if UNITY_IOS && !UNITY_EDITOR
            _appEvents = go.AddComponent<IosAppEvents>();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            _appEvents = go.AddComponent<AndroidAppEvents>();
            #elif UNITY_EDITOR
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

        public event Action WillGoBackground
        {
            add
            {
                _appEvents.WillGoBackground += value;
            }
            remove
            {
                _appEvents.WillGoBackground -= value;
            }
        }

        public event Action GoBackground
        {
            add
            {
                _appEvents.GoBackground += value;
            }
            remove
            {
                _appEvents.GoBackground -= value;
            }
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

