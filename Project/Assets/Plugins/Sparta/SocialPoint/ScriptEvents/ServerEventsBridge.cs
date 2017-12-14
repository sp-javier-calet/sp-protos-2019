using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using SocialPoint.Lifecycle;

namespace SocialPoint.ScriptEvents
{
    public struct ServerTrackAction
    {
        public string Name;
        public AttrDic Arguments;
    }

    public sealed class ServerTrackActionParser : BaseScriptEventParser<ServerTrackAction>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";

        public ServerTrackActionParser() : base("action.server.track")
        {
        }

        override protected ServerTrackAction ParseEvent(Attr data)
        {
            return new ServerTrackAction {
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (AttrDic)data.AsDic[AttrKeyArguments].AsDic.Clone()
            };
        }
    }

    public sealed class ServerEventsBridge :
        IScriptEventsBridge
    {
        IEventProcessor _processor;
        IEventTracker _tracker;

        public ServerEventsBridge(IEventTracker tracker)
        {
            _tracker = tracker;
        }

        public void Load(IScriptEventProcessor scriptProcessor, IEventProcessor processor)
        {
            _processor = processor;
            _processor.RegisterHandler<ServerTrackAction>(OnTrackAction);
            scriptProcessor.RegisterParser(new ServerTrackActionParser());
        }

        public void Dispose()
        {
            if(_processor != null)
            {
                _processor.UnregisterHandler<ServerTrackAction>(OnTrackAction);
            }
        }

        void OnTrackAction(ServerTrackAction action)
        {
            _tracker.TrackSystemEvent(action.Name, action.Arguments);
        }

    }
}