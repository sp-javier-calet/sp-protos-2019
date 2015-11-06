using SocialPoint.AppEvents;
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


    public class EventDispatcherAppEventsBridge : IDisposable
    {
        IEventDispatcher _dispatcher;
        IAppEvents _appEvents;

        public EventDispatcherAppEventsBridge(IEventDispatcher dispatcher, IAppEvents appEvents)
        {
            _dispatcher = dispatcher;
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
            _dispatcher.Raise(new AppWillGoBackgroundEvent{
                Priority = priority
            });
        }

        void OnGameWasLoaded(int priority)
        {
            _dispatcher.Raise(new AppGameWasLoadedEvent{
                Priority = priority
            });
        }

        void OnGameWillRestart(int priority)
        {
            _dispatcher.Raise(new AppGameWillRestartEvent{
                Priority = priority
            });
        }
        
        void OnWasOnBackground()
        {
            _dispatcher.Raise(new AppWasOnBackgroundEvent{});
        }

        void OnWasCovered()
        {
            _dispatcher.Raise(new AppWasCoveredEvent{});
        }

        void OnReceivedMemoryWarning()
        {
            _dispatcher.Raise(new AppReceivedMemoryWarningEvent{});
        }

        void OnOpenedFromSource(AppSource source)
        {
            _dispatcher.Raise(new AppOpenedFromSourceEvent{
                Source = source
            });
        }

        void OnApplicationQuit()
        {
            _dispatcher.Raise(new AppQuitEvent{});
        }

        void OnLevelWasLoaded(int level)
        {
            _dispatcher.Raise(new AppLevelWasLoadedEvent{
                Level = level
            });
        }


    }

}