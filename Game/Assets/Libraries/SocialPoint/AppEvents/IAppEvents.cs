using System;

namespace SocialPoint.AppEvents
{
    public interface IAppEvents
    {
        // Native events
        event Action WillGoBackground;
        event Action GoBackground;
        event Action WasOnBackground;
        event Action WasCovered;
        event Action ReceivedMemoryWarning;

        event Action<AppSource> OpenedFromSource;
        AppSource Source { get; }

        // Unity events
        event Action ApplicationQuit;
        event Action<int> LevelWasLoaded;
    }
}
