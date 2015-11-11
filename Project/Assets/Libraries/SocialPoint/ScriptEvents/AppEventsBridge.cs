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

    public class AppWillGoBackgroundEventConverter : BaseScriptEventConverter<AppWillGoBackgroundEvent>
    {
        public AppWillGoBackgroundEventConverter(): base("app.will_go_background")
        {
        }

        override protected AppWillGoBackgroundEvent ParseEvent(Attr data)
        {
            return new AppWillGoBackgroundEvent{ Priority = data.AsValue.ToInt() };
        }

        override protected Attr SerializeEvent(AppWillGoBackgroundEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public class AppGameWasLoadedEventConverter : BaseScriptEventConverter<AppGameWasLoadedEvent>
    {
        public AppGameWasLoadedEventConverter(): base("app.game_was_loaded")
        {
        }

        override protected AppGameWasLoadedEvent ParseEvent(Attr data)
        {
            return new AppGameWasLoadedEvent{ Priority = data.AsValue.ToInt() };
        }
        
        override protected Attr SerializeEvent(AppGameWasLoadedEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }
    
    public class AppGameWillRestartEventConverter : BaseScriptEventConverter<AppGameWillRestartEvent>
    {
        public AppGameWillRestartEventConverter(): base("app.game_will_restart")
        {
        }

        override protected AppGameWillRestartEvent ParseEvent(Attr data)
        {
            return new AppGameWillRestartEvent{ Priority = data.AsValue.ToInt() };
        }
        
        override protected Attr SerializeEvent(AppGameWillRestartEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public class AppLevelWasLoadedEventConverter : BaseScriptEventConverter<AppLevelWasLoadedEvent>
    {
        public AppLevelWasLoadedEventConverter(): base("app.level_was_loaded")
        {
        }

        override protected AppLevelWasLoadedEvent ParseEvent(Attr data)
        {
            return new AppLevelWasLoadedEvent{ Level = data.AsValue.ToInt() };
        }
        
        override protected Attr SerializeEvent(AppLevelWasLoadedEvent ev)
        {
            return new AttrInt(ev.Level);
        }
    }
    
    public class AppOpenedFromSourceEventConverter : BaseScriptEventConverter<AppOpenedFromSourceEvent>
    {
        const string AttrKeyUri = "uri";
        const string AttrKeyScheme = "scheme";
        const string AttrKeyParameters = "params";

        public AppOpenedFromSourceEventConverter(): base("app.opened_from_source")
        {
        }

        override protected AppOpenedFromSourceEvent ParseEvent(Attr data)
        {
            var source = new AppSource(data.AsDic[AttrKeyUri].AsValue.ToString());
            return new AppOpenedFromSourceEvent{ Source = source };
        }
        
        override protected Attr SerializeEvent(AppOpenedFromSourceEvent ev)
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
    }

    public class AppEventsBridge :
        IEventsBridge,
        IScriptEventsBridge
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

        const string AppWasOnBackgroundEventName = "app.was_on_background";
        const string AppWasCoveredEventName = "app.was_covered";
        const string AppReceivedMemoryWarningEventName = "app.received_memory_warning";

        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddConverter(new AppWillGoBackgroundEventConverter());
            dispatcher.AddConverter(new AppGameWasLoadedEventConverter());
            dispatcher.AddConverter(new AppGameWillRestartEventConverter());
            dispatcher.AddConverter(new AppLevelWasLoadedEventConverter());
            dispatcher.AddConverter(new AppOpenedFromSourceEventConverter());
            dispatcher.AddConverter(new ScriptEventConverter<AppWasOnBackgroundEvent>(AppWasOnBackgroundEventName));
            dispatcher.AddConverter(new ScriptEventConverter<AppWasCoveredEvent>(AppWasCoveredEventName));
            dispatcher.AddConverter(new ScriptEventConverter<AppReceivedMemoryWarningEvent>(AppReceivedMemoryWarningEventName));
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