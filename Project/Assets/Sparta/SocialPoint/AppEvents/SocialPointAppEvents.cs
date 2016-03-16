using System;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    public class SocialPointAppEvents : IAppEvents
    {
        BaseAppEvents _appEvents;

        public SocialPointAppEvents(Transform parent = null)
        {
            Setup(parent);
        }

        public void Dispose()
        {
            DestroyAppEvents();
        }

        void Setup(Transform parent)
        {
            var go = new GameObject(GetType().ToString());
            if(parent == null)
            {
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            else
            {
                go.transform.SetParent(parent);
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

        void DestroyAppEvents()
        {
            if(_appEvents != null)
            {
                UnityEngine.Object.Destroy(_appEvents);
                _appEvents = null;
            }
        }

        #region IAppEvents implementation


        PriorityAction _default = new PriorityAction();

        public PriorityAction WillGoBackground
        {
            get
            {
                return _appEvents == null ? _default : _appEvents.WillGoBackground;
            }
        }

        public PriorityAction GameWasLoaded
        {
            get
            {
                return _appEvents == null ? _default : _appEvents.GameWasLoaded;
            }
        }

        public PriorityAction GameWillRestart
        {
            get
            {
                return _appEvents == null ? _default : _appEvents.GameWillRestart;
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

        public void TriggerApplicationQuit()
        {
            if(_appEvents == null)
            {
                return;
            }
            _appEvents.TriggerApplicationQuit();
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
                _appEvents.WasCovered -= value;
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
                return _appEvents == null ? null : _appEvents.Source;
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

