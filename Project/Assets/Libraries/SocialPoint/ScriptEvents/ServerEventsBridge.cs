using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using System;

namespace SocialPoint.ScriptEvents
{
    public struct ServerTrackAction
    {
        public string Name;
        public AttrDic Arguments;
    }

    public class ServerTrackActionParser : BaseScriptEventParser<ServerTrackAction>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";
        
        public ServerTrackActionParser(): base("action.server.track")
        {
        }
        
        override protected ServerTrackAction ParseEvent(Attr data)
        {
            return new ServerTrackAction{
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (AttrDic)data.AsDic[AttrKeyArguments].AsDic.Clone()
            };
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
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<ServerTrackAction>(OnTrackAction);
        }

        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddParser(new ServerTrackActionParser());
        }

        public void Dispose()
        {
            if(_dispatcher != null)
            {
                _dispatcher.RemoveListener<ServerTrackAction>(OnTrackAction);
            }
        }

        void OnTrackAction(ServerTrackAction action)
        {
            _tracker.TrackEvent(action.Name, action.Arguments);
        }

    }
}