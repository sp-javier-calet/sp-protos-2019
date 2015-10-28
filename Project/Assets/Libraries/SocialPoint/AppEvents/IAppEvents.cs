using System;
using SocialPoint.Utils;

namespace SocialPoint.AppEvents
{
    public interface IAppEvents : IDisposable
    {   
        // Native events
        void RegisterWillGoBackground(int priority, Action action);
        void UnregisterWillGoBackground(Action action);

        void RegisterGameWasLoaded(int priority, Action action);
        void UnregisterGameWasLoaded(Action action);

        void RegisterGameWillRestart(int priority, Action action);
        void UnregisterGameWillRestart(Action action);

        event Action WasOnBackground;
        event Action WasCovered;
        event Action ReceivedMemoryWarning;
        event Action<AppSource> OpenedFromSource;

        AppSource Source { get; }

        // Unity events
        event Action ApplicationQuit;
        event Action<int> LevelWasLoaded;

        void TriggerMemoryWarning();
        void TriggerWillGoBackground();

        void TriggerGameWasLoaded();
        void RestartGame();
    }
}
