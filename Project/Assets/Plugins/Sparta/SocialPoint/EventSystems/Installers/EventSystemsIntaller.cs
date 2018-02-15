using System;
using SocialPoint.Dependency;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class EventSystemsIntaller : Installer, IInitializable, IDisposable
    {
        [Serializable]
        public class SettingsData
        {
            public bool CreateAlways;
            public EventSystem EventSystemPrefab;
        }

        public SettingsData Settings = new SettingsData();

        EventSystem _eventSystem;
        SPStandaloneInputModule _actionStandaloneInputModule;

        public override void InstallBindings()
        {
            if(Settings.CreateAlways)
            {
                Container.Bind<IInitializable>().ToInstance(this);
            }

            Container.Add<IDisposable, EventSystemsIntaller>(this);
            Container.Rebind<EventSystem>().ToMethod<EventSystem>(CreateEventSystem);
        }

        public void Initialize()
        {
            Container.Resolve<EventSystem>();
        }

        EventSystem CreateEventSystem()
        {
            _eventSystem = GameObject.Instantiate(Settings.EventSystemPrefab);
            _eventSystem.name = Settings.EventSystemPrefab.name;

            var parent = Container.Resolve<Transform>();
            if(parent != null)
            {
                _eventSystem.transform.SetParent(parent);
            }
            else
            {
                DontDestroyOnLoad(_eventSystem);
            }

            _actionStandaloneInputModule = _eventSystem.GetComponent<SPStandaloneInputModule>();
            if(_actionStandaloneInputModule != null)
            {
                Container.Rebind<SPStandaloneInputModule>().ToInstance(_actionStandaloneInputModule);
            }

            return _eventSystem;
        }

        public void Dispose()
        {
            if(_eventSystem != null)
            {
                Destroy(_eventSystem);
            }
        }
    }
}
