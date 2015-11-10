using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.ScriptEvents
{
    public struct AppWillGoBackgroundEvent
    {
        public int Priority;
    }

    public struct AppGameWasLoadedEvent
    {
        public int Priority;
    }

    public struct AppGameWillRestartEvent
    {
        public int Priority;
    }

    public struct AppWasOnBackgroundEvent
    {
    }

    public struct AppWasCoveredEvent
    {
    }

    public struct AppReceivedMemoryWarningEvent
    {
    }

    public struct AppOpenedFromSourceEvent
    {
        public AppSource Source;
    }

    public struct AppQuitEvent
    {
    }

    public struct AppLevelWasLoadedEvent
    {
        public int Level;
    }

    public class AppEventsBridge : IEventsBridge,
        ISerializer<AppWillGoBackgroundEvent>,
        ISerializer<AppGameWasLoadedEvent>,
        ISerializer<AppGameWillRestartEvent>,
        ISerializer<AppOpenedFromSourceEvent>,
        ISerializer<AppLevelWasLoadedEvent>
    {
        IEventDispatcher _dispatcher;
        IAppEvents _appEvents;

        public AppEventsBridge(IAppEvents appEvents)
        {
            _appEvents = appEvents;
            _appEvents.WillGoBackground.Add(OnWillGoBackground);
            _appEvents.GameWasLoaded.Add(OnGameWasLoaded);
            _appEvents.GameWillRestart.Add(OnGameWillRestart);
            _appEvents.WasOnBackground += OnWasOnBackground;
            _appEvents.WasCovered += OnWasCovered;
            _appEvents.OpenedFromSource += OnOpenedFromSource;
            _appEvents.ApplicationQuit += OnApplicationQuit;
            _appEvents.LevelWasLoaded += OnLevelWasLoaded;
        }

        public Attr Serialize(AppWillGoBackgroundEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
        
        public Attr Serialize(AppGameWasLoadedEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
        
        public Attr Serialize(AppGameWillRestartEvent ev)
        {
            return new AttrInt(ev.Priority);
        }

        public Attr Serialize(AppLevelWasLoadedEvent ev)
        {
            return new AttrInt(ev.Level);
        }

        const string AttrKeyUri = "uri";
        const string AttrKeyScheme = "scheme";
        const string AttrKeyParameters = "params";
                
        public Attr Serialize(AppOpenedFromSourceEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyUri, ev.Source.Uri.ToString());
            data.SetValue(AttrKeyScheme, ev.Source.Scheme);
            var parms = new AttrDic();
            data.Set(AttrKeyParameters, parms);
            foreach(var kvp in ev.Source.Parameters)
            {
                parms.SetValue(kvp.Key, kvp.Value);
            }
            return data;
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispose()
        {
            _appEvents.WillGoBackground.Remove(OnWillGoBackground);
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.GameWillRestart.Remove(OnGameWillRestart);
            _appEvents.WasOnBackground -= OnWasOnBackground;
            _appEvents.WasCovered -= OnWasCovered;
            _appEvents.OpenedFromSource -= OnOpenedFromSource;
            _appEvents.ApplicationQuit -= OnApplicationQuit;
            _appEvents.LevelWasLoaded -= OnLevelWasLoaded;
        }

        void OnWillGoBackground(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWillGoBackgroundEvent{
                Priority = priority
            });
        }

        void OnGameWasLoaded(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppGameWasLoadedEvent{
                Priority = priority
            });
        }

        void OnGameWillRestart(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppGameWillRestartEvent{
                Priority = priority
            });
        }
        
        void OnWasOnBackground()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWasOnBackgroundEvent{});
        }

        void OnWasCovered()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWasCoveredEvent{});
        }

        void OnReceivedMemoryWarning()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppReceivedMemoryWarningEvent{});
        }

        void OnOpenedFromSource(AppSource source)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppOpenedFromSourceEvent{
                Source = source
            });
        }

        void OnApplicationQuit()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppQuitEvent{});
        }

        void OnLevelWasLoaded(int level)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppLevelWasLoadedEvent{
                Level = level
            });
        }

    }

}