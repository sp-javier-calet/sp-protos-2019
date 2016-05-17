using System.Collections;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;

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

    public struct AppAfterGameWasLoadedEvent
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

    public class AppWillGoBackgroundEventSerializer : BaseScriptEventSerializer<AppWillGoBackgroundEvent>
    {
        public AppWillGoBackgroundEventSerializer() : base("event.app.will_go_background")
        {
        }

        override protected Attr SerializeEvent(AppWillGoBackgroundEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public class AppGameWasLoadedEventSerializer : BaseScriptEventSerializer<AppGameWasLoadedEvent>
    {
        public AppGameWasLoadedEventSerializer() : base("event.app.game_was_loaded")
        {
        }

        override protected Attr SerializeEvent(AppGameWasLoadedEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public class AppGameWillRestartEventSerializer : BaseScriptEventSerializer<AppGameWillRestartEvent>
    {
        public AppGameWillRestartEventSerializer() : base("event.app.game_will_restart")
        {
        }

        override protected Attr SerializeEvent(AppGameWillRestartEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public class AppLevelWasLoadedEventSerializer : BaseScriptEventSerializer<AppLevelWasLoadedEvent>
    {
        public AppLevelWasLoadedEventSerializer() : base("event.app.level_was_loaded")
        {
        }

        override protected Attr SerializeEvent(AppLevelWasLoadedEvent ev)
        {
            return new AttrInt(ev.Level);
        }
    }

    public class AppOpenedFromSourceEventSerializer : BaseScriptEventSerializer<AppOpenedFromSourceEvent>
    {
        const string AttrKeyUri = "uri";
        const string AttrKeyScheme = "scheme";
        const string AttrKeyParameters = "params";

        public AppOpenedFromSourceEventSerializer() : base("event.app.opened_from_source")
        {
        }

        override protected Attr SerializeEvent(AppOpenedFromSourceEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyUri, ev.Source.ToString());
            data.SetValue(AttrKeyScheme, ev.Source.Scheme);
            var parms = new AttrDic();
            data.Set(AttrKeyParameters, parms);
            var itr = ev.Source.Parameters.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                parms.SetValue(kvp.Key, kvp.Value);
            }
            itr.Dispose();
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
            _appEvents.AfterGameWasLoaded.Add(AfterGameWasLoaded);
            _appEvents.GameWillRestart.Add(OnGameWillRestart);
            _appEvents.WasOnBackground += OnWasOnBackground;
            _appEvents.WasCovered += OnWasCovered;
            _appEvents.OpenedFromSource += OnOpenedFromSource;
            _appEvents.ApplicationQuit += OnApplicationQuit;
            _appEvents.LevelWasLoaded += OnLevelWasLoaded;
            _appEvents.ReceivedMemoryWarning += OnReceivedMemoryWarning;
        }

        const string AppWasOnBackgroundEventName = "event.app.was_on_background";
        const string AppWasCoveredEventName = "event.app.was_covered";
        const string AppReceivedMemoryWarningEventName = "event.app.memory_warning";
        const string AppQuitEventName = "event.app.quit";

        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddSerializer(new AppWillGoBackgroundEventSerializer());
            dispatcher.AddSerializer(new AppGameWasLoadedEventSerializer());
            dispatcher.AddSerializer(new AppGameWillRestartEventSerializer());
            dispatcher.AddSerializer(new AppLevelWasLoadedEventSerializer());
            dispatcher.AddSerializer(new AppOpenedFromSourceEventSerializer());
            dispatcher.AddSerializer(new ScriptEventSerializer<AppWasOnBackgroundEvent>(AppWasOnBackgroundEventName));
            dispatcher.AddSerializer(new ScriptEventSerializer<AppWasCoveredEvent>(AppWasCoveredEventName));
            dispatcher.AddSerializer(new ScriptEventSerializer<AppReceivedMemoryWarningEvent>(AppReceivedMemoryWarningEventName));
            dispatcher.AddSerializer(new ScriptEventSerializer<AppQuitEvent>(AppQuitEventName));
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispose()
        {
            _appEvents.WillGoBackground.Remove(OnWillGoBackground);
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.AfterGameWasLoaded.Remove(AfterGameWasLoaded);
            _appEvents.GameWillRestart.Remove(OnGameWillRestart);
            _appEvents.WasOnBackground -= OnWasOnBackground;
            _appEvents.WasCovered -= OnWasCovered;
            _appEvents.OpenedFromSource -= OnOpenedFromSource;
            _appEvents.ApplicationQuit -= OnApplicationQuit;
            _appEvents.LevelWasLoaded -= OnLevelWasLoaded;
            _appEvents.ReceivedMemoryWarning -= OnReceivedMemoryWarning;
        }

        void OnWillGoBackground(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWillGoBackgroundEvent {
                Priority = priority
            });
        }

        void OnGameWasLoaded(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppGameWasLoadedEvent {
                Priority = priority
            });
        }

        IEnumerator AfterGameWasLoaded(int priority)
        {
            if(_dispatcher == null)
            {
                yield break;
            }
            _dispatcher.Raise(new AppAfterGameWasLoadedEvent {
                Priority = priority
            });
            yield break;
        }

        void OnGameWillRestart(int priority)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppGameWillRestartEvent {
                Priority = priority
            });
        }

        void OnWasOnBackground()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWasOnBackgroundEvent());
        }

        void OnWasCovered()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppWasCoveredEvent());
        }

        void OnReceivedMemoryWarning()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppReceivedMemoryWarningEvent());
        }

        void OnOpenedFromSource(AppSource source)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppOpenedFromSourceEvent {
                Source = source
            });
        }

        void OnApplicationQuit()
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppQuitEvent());
        }

        void OnLevelWasLoaded(int level)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new AppLevelWasLoadedEvent {
                Level = level
            });
        }

    }

}