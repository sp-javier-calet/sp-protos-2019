using SocialPoint.Attributes;
using SocialPoint.ServerSync;
using System;

namespace SocialPoint.ScriptEvents
{

    public struct ServerCommandAction
    {
        public string Name;
        public Attr Arguments;
    }

    public struct ServerCommandResponseEvent
    {
        public Command Command;
        public Attr Response;
    }

    public sealed class ServerCommandActionParser : BaseScriptEventParser<ServerCommandAction>
    {
        const string AttrKeyName = "name";
        const string AttrKeyArguments = "args";

        public ServerCommandActionParser() : base("action.server.command")
        {
        }

        override protected ServerCommandAction ParseEvent(Attr data)
        {
            return new ServerCommandAction {
                Name = data.AsDic[AttrKeyName].AsValue.ToString(),
                Arguments = (AttrDic)data.AsDic[AttrKeyArguments].Clone()
            };
        }
    }

    public sealed class ServerCommandResponseEventSerializer : BaseScriptEventSerializer<ServerCommandResponseEvent>
    {
        const string AttrKeyCommandName = "name";
        const string AttrKeyCommandArguments = "args";
        const string AttrKeyResponse = "response";

        public ServerCommandResponseEventSerializer() : base("event.server.command_response")
        {
        }

        override protected Attr SerializeEvent(ServerCommandResponseEvent ev)
        {
            var data = new AttrDic();
            if(ev.Command != null)
            {
                data.SetValue(AttrKeyCommandName, ev.Command.Name);
                if(ev.Command.Arguments != null)
                {
                    data.Set(AttrKeyCommandArguments, (Attr)ev.Command.Arguments.Clone());
                }
            }
            data.Set(AttrKeyResponse, (Attr)ev.Response.Clone());
            return data;
        }
    }

    public sealed class ServerSyncBridge :
        IEventsBridge, 
        IScriptEventsBridge
    {
        
        IEventDispatcher _dispatcher;
        ICommandQueue _queue;

        public ServerSyncBridge(ICommandQueue queue)
        {
            _queue = queue;
            _queue.CommandResponse += OnCommandResponse;
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<ServerCommandAction>(OnCommandAction);
        }

        public void Load(IScriptEventProcessor dispatcher)
        {
            dispatcher.RegisterParser(new ServerCommandActionParser());
            dispatcher.RegisterSerializer(new ServerCommandResponseEventSerializer());
        }

        public void Dispose()
        {
            if(_dispatcher != null)
            {
                _dispatcher.RemoveListener<ServerCommandAction>(OnCommandAction);
            }
            _queue.CommandResponse -= OnCommandResponse;
        }

        void OnCommandResponse(Command cmd, Attr resp)
        {
            if(_dispatcher == null)
            {
                return;
            }
            _dispatcher.Raise(new ServerCommandResponseEvent {
                Command = cmd,
                Response = resp
            });
        }

        void OnCommandAction(ServerCommandAction action)
        {
            _queue.Add(new Command(action.Name, action.Arguments));
        }
    }
}