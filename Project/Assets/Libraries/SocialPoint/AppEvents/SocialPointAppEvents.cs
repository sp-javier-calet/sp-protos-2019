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
            #if UNITY_EDITOR
            _appEvents = go.AddComponent<UnityAppEvents>();
            #elif UNITY_IOS
            _appEvents = go.AddComponent<IosAppEvents>();
            #elif UNITY_ANDROID
            _appEvents = go.AddComponent<AndroidAppEvents>();
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


        PriorityAction _default = new PriorityAction();

        public PriorityAction WillGoBackground
        {
            get
            {
                if(_appEvents == null)
                {
                    return _default;
                }
                return _appEvents.WillGoBackground;
            }
        }
        
        public PriorityAction GameWasLoaded
        {
            get
            {
                if(_appEvents == null)
                {
                    return _default;
                }
                return _appEvents.GameWasLoaded;
            }
        }
        
        public PriorityAction GameWillRestart
        {
            get
            {
                if(_appEvents == null)
                {
                    return _default;
                }
                return _appEvents.GameWillRestart;
            }
        }

        public void TriggerMemoryWarning()
        {
            if(_appEvents == null)
            {
                return;
            }
            _appEvents.TriggerMemoryWarning();
        }
        
        public void TriggerWillGoBackground()
        {
            if(_appEvents == null)
            {
                return;
            }
            _appEvents.TriggerWillGoBackground();
        }
        
        public void TriggerGameWasLoaded()
        {
            if(_appEvents == null)
            {
                return;
            }
            _appEvents.TriggerGameWasLoaded();
        }
                
        public void TriggerGameWillRestart()
        {
            if(_appEvents == null)
            {
                return;
            }
            _appEvents.TriggerGameWillRestart();
        }

        public event Action WasOnBackground
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.WasOnBackground += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.WasOnBackground -= value;
            }
        }

        public event Action WasCovered
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.WasCovered += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.WasOnBackground -= value;
            }
        }

        public event Action ReceivedMemoryWarning
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.ReceivedMemoryWarning += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.ReceivedMemoryWarning -= value;
            }
        }

        public AppSource Source
        {
            get
            {
                if(_appEvents == null)
                {
                    return null;
                }
                return _appEvents.Source;
            }
        }

        public event Action<AppSource> OpenedFromSource
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.OpenedFromSource += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.OpenedFromSource -= value;
            }
        }

        public event Action ApplicationQuit
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.ApplicationQuit += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.ApplicationQuit -= value;
            }
        }
        
        public event Action<int> LevelWasLoaded
        {
            add
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.LevelWasLoaded += value;
            }
            remove
            {
                if(_appEvents == null)
                {
                    return;
                }
                _appEvents.LevelWasLoaded -= value;
            }
        }

        #endregion
    }
}

