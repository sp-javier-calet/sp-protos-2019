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

    public class ServerEventConverter : BaseScriptEventConverter<ServerEvent>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";

        public ServerEventConverter(): base("server")
        {
        }
        
        override protected ServerEvent ParseEvent(Attr data)
        {
            return new ServerEvent{
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (Attr)data.AsDic[AttrKeyArguments].AsDic.Clone()
            };
        }
        
        override protected Attr SerializeEvent(ServerEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyName, ev.Name);
            data.Set(AttrKeyArguments, (Attr)ev.Arguments.Clone());
            return data;
        }
    }

    public class ServerEventsBridge :
        IEventsBridge, 
        IScriptEventsBridge
    {

        IEventDispatcher _dispatcher;
        IEventTracker _tracker;

        public ServerEventsBridge(IEventTracker tracker)
        {
            _tracker = tracker;
            _tracker.EventTracked += OnEventTracked;
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        const string EventName = "server";

        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddConverter(new ServerEventConverter());
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