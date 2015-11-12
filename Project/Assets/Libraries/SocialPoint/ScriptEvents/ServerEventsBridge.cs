using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using System;

namespace SocialPoint.ScriptEvents
{
    public struct ServerTrackEvent
    {
        public string Name;
        public AttrDic Arguments;    	
    }

    public struct ServerTrackAction
    {
        public string Name;
        public AttrDic Arguments;
    }

    public class ServerTrackEventConverter : BaseScriptEventConverter<ServerTrackEvent>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";

        public ServerTrackEventConverter(): base("events.server.track")
        {
        }
        
        override protected ServerTrackEvent ParseEvent(Attr data)
        {
            return new ServerTrackEvent{
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (AttrDic)data.AsDic[AttrKeyArguments].AsDic.Clone()
            };
        }
        
        override protected Attr SerializeEvent(ServerTrackEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyName, ev.Name);
            data.Set(AttrKeyArguments, (Attr)ev.Arguments.Clone());
            return data;
        }
    }

    public class ServerTrackActionConverter : BaseScriptEventConverter<ServerTrackAction>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";
        
        public ServerTrackActionConverter(): base("actions.server.track")
        {
        }
        
        override protected ServerTrackAction ParseEvent(Attr data)
        {
            return new ServerTrackAction{
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (AttrDic)data.AsDic[AttrKeyArguments].AsDic.Clone()
            };
        }
        
        override protected Attr SerializeEvent(ServerTrackAction action)
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyName, action.Name);
            data.Set(AttrKeyArguments, (Attr)action.Arguments.Clone());
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
            _dispatcher.AddListener<ServerTrackAction>(OnTrackAction);
        }

        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddConverter(new ServerTrackEventConverter());
            dispatcher.AddConverter(new ServerTrackActionConverter());
        }

        public void Dispose()
        {
            if(_dispatcher != null)
            {
                _dispatcher.RemoveListener<ServerTrackAction>(OnTrackAction);
            }
            _tracker.EventTracked -= OnEventTracked;
        }

        void OnTrackAction(ServerTrackAction action)
        {
            _tracker.TrackEvent(action.Name, action.Arguments);
        }

        void OnEventTracked(string name, AttrDic args)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new ServerTrackEvent{
                Name = name,
                Arguments = args
            });
        }
    }
}