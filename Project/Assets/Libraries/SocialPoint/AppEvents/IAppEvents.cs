using System;
using SocialPoint.Utils;

namespace SocialPoint.AppEvents
{
    public interface IAppEvents : IDisposable
    {
        // Native events
        PriorityAction WillGoBackground{ get; }

        event Action WasOnBackground;
        event Action WasCovered;
        event Action ReceivedMemoryWarning;

        event Action<AppSource> OpenedFromSource;
        AppSource Source { get; }

        // Unity events
        event Action ApplicationQuit;
        event Action<int> LevelWasLoaded;

        // testing
        void TriggerMemoryWarning();
        void TriggerWillGoBackground();
    }
}
