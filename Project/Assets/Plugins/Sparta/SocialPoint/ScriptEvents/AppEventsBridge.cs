using System.Collections;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Lifecycle;

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

    public sealed class AppWillGoBackgroundEventSerializer : BaseScriptEventSerializer<AppWillGoBackgroundEvent>
    {
        public AppWillGoBackgroundEventSerializer() : base("event.app.will_go_background")
        {
        }

        override protected Attr SerializeEvent(AppWillGoBackgroundEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public sealed class AppGameWasLoadedEventSerializer : BaseScriptEventSerializer<AppGameWasLoadedEvent>
    {
        public AppGameWasLoadedEventSerializer() : base("event.app.game_was_loaded")
        {
        }

        override protected Attr SerializeEvent(AppGameWasLoadedEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public sealed class AppGameWillRestartEventSerializer : BaseScriptEventSerializer<AppGameWillRestartEvent>
    {
        public AppGameWillRestartEventSerializer() : base("event.app.game_will_restart")
        {
        }

        override protected Attr SerializeEvent(AppGameWillRestartEvent ev)
        {
            return new AttrInt(ev.Priority);
        }
    }

    public sealed class AppLevelWasLoadedEventSerializer : BaseScriptEventSerializer<AppLevelWasLoadedEvent>
    {
        public AppLevelWasLoadedEventSerializer() : base("event.app.level_was_loaded")
        {
        }

        override protected Attr SerializeEvent(AppLevelWasLoadedEvent ev)
        {
            return new AttrInt(ev.Level);
        }
    }

    public sealed class AppOpenedFromSourceEventSerializer : BaseScriptEventSerializer<AppOpenedFromSourceEvent>
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

    public sealed class AppWasOnBackgroundEventSerializer : BaseScriptEventSerializer<AppWasOnBackgroundEvent>
    {
        public AppWasOnBackgroundEventSerializer() : base("event.app.was_on_background")
        {
        }

        override protected Attr SerializeEvent(AppWasOnBackgroundEvent ev)
        {
            return new AttrEmpty();
        }
    }

    public sealed class AppWasCoveredEventSerializer : BaseScriptEventSerializer<AppWasCoveredEvent>
    {
        public AppWasCoveredEventSerializer() : base("event.app.was_covered")
        {
        }

        override protected Attr SerializeEvent(AppWasCoveredEvent ev)
        {
            return new AttrEmpty();
        }
    }

    public sealed class AppReceivedMemoryWarningEventSerializer : BaseScriptEventSerializer<AppReceivedMemoryWarningEvent>
    {
        public AppReceivedMemoryWarningEventSerializer() : base("event.app.memory_warning")
        {
        }

        override protected Attr SerializeEvent(AppReceivedMemoryWarningEvent ev)
        {
            return new AttrEmpty();
        }
    }

    public sealed class AppQuitEventSerializer : BaseScriptEventSerializer<AppQuitEvent>
    {
        public AppQuitEventSerializer() : base("event.app.quit")
        {
        }

        override protected Attr SerializeEvent(AppQuitEvent ev)
        {
            return new AttrEmpty();
        }
    }

    public sealed class AppEventsBridge :
        IScriptEventsBridge
    {
        IEventProcessor _processor;
        IAppEvents _appEvents;

        public AppEventsBridge(IAppEvents appEvents)
        {
            _appEvents = appEvents;
            _appEvents.WillGoBackground.Add(OnWillGoBackground);
            _appEvents.GameWasLoaded.Add(OnGameWasLoaded);
            _appEvents.AfterGameWasLoaded.Add(AfterGameWasLoaded);
            _appEvents.GameWillRestart.Add(OnGameWillRestart);
            _appEvents.WasOnBackground.Add(0, OnWasOnBackground);
            _appEvents.WasCovered += OnWasCovered;
            _appEvents.OpenedFromSource += OnOpenedFromSource;
            _appEvents.ApplicationQuit += OnApplicationQuit;
            _appEvents.ReceivedMemoryWarning += OnReceivedMemoryWarning;
        }

        public void Load(IScriptEventProcessor scriptProcessor, IEventProcessor processor)
        {
            _processor = processor;
            scriptProcessor.RegisterSerializer(new AppWillGoBackgroundEventSerializer());
            scriptProcessor.RegisterSerializer(new AppGameWasLoadedEventSerializer());
            scriptProcessor.RegisterSerializer(new AppGameWillRestartEventSerializer());
            scriptProcessor.RegisterSerializer(new AppLevelWasLoadedEventSerializer());
            scriptProcessor.RegisterSerializer(new AppOpenedFromSourceEventSerializer());
            scriptProcessor.RegisterSerializer(new AppWasOnBackgroundEventSerializer());
            scriptProcessor.RegisterSerializer(new AppWasCoveredEventSerializer());
            scriptProcessor.RegisterSerializer(new AppReceivedMemoryWarningEventSerializer());
            scriptProcessor.RegisterSerializer(new AppQuitEventSerializer());
        }

        public void Dispose()
        {
            _appEvents.WillGoBackground.Remove(OnWillGoBackground);
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.AfterGameWasLoaded.Remove(AfterGameWasLoaded);
            _appEvents.GameWillRestart.Remove(OnGameWillRestart);
            _appEvents.WasOnBackground.Remove(OnWasOnBackground);
            _appEvents.WasCovered -= OnWasCovered;
            _appEvents.OpenedFromSource -= OnOpenedFromSource;
            _appEvents.ApplicationQuit -= OnApplicationQuit;
            _appEvents.ReceivedMemoryWarning -= OnReceivedMemoryWarning;
        }

        void OnWillGoBackground(int priority)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppWillGoBackgroundEvent {
                Priority = priority
            });
        }

        void OnGameWasLoaded(int priority)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppGameWasLoadedEvent {
                Priority = priority
            });
        }

        IEnumerator AfterGameWasLoaded(int priority)
        {
            if(_processor == null)
            {
                yield break;
            }
            _processor.Process(new AppAfterGameWasLoadedEvent {
                Priority = priority
            });
            yield break;
        }

        void OnGameWillRestart(int priority)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppGameWillRestartEvent {
                Priority = priority
            });
        }

        void OnWasOnBackground()
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppWasOnBackgroundEvent());
        }

        void OnWasCovered()
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppWasCoveredEvent());
        }

        void OnReceivedMemoryWarning()
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppReceivedMemoryWarningEvent());
        }

        void OnOpenedFromSource(AppSource source)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppOpenedFromSourceEvent {
                Source = source
            });
        }

        void OnApplicationQuit()
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new AppQuitEvent());
        }
    }

}