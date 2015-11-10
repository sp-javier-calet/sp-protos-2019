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

    public class ServerEventsBridge : IEventsBridge, ISerializer<ServerEvent>
    {
        IEventDispatcher _dispatcher;
        IEventTracker _tracker;

        public ServerEventsBridge(IEventTracker tracker)
        {
            _tracker = tracker;
            _tracker.EventTracked += OnEventTracked;
        }

        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";

        public Attr Serialize(ServerEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyName, ev.Name);
            data.Set(AttrKeyArguments, (Attr)ev.Arguments.Clone());
            return data;
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        void OnEventTracked(string name, Attr args)
        {
            if(_dispatcher == null)
            {
                return;
            }
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