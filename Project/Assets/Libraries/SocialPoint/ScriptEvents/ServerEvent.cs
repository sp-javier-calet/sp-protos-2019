using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using System;

namespace SocialPoint.ScriptEvents
{
    public struct ServerEvent
    {
        public string Name;
        public Attr Arguments;    	
    }

    public class EventDispatcherEventTrackerBridge : IDisposable
    {
        IEventDispatcher _dispatcher;
        IEventTracker _tracker;

        public EventDispatcherEventTrackerBridge(IEventDispatcher dispatcher, IEventTracker tracker)
        {
            _dispatcher = dispatcher;
            _tracker = tracker;

            _tracker.EventTracked += OnEventTracked;
        }

        void OnEventTracked(string name, Attr args)
        {
            _dispatcher.Raise(new ServerEvent{
                Name = name,
                Arguments = args
            });
        }

        public void Dispose()
        {
            _tracker.EventTracked -= OnEventTracked;
        }
    }
}