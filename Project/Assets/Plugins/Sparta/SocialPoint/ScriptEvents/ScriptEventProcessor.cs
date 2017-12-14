using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Lifecycle;

namespace SocialPoint.ScriptEvents
{
    public interface IScriptEventProcessor : IDisposable
    {
        void RegisterHandler(string name, Action<Attr> handler);

        void RegisterHandler(Action<string, Attr> handler);

        void RegisterHandler(IScriptCondition condition, Action<string, Attr> handler);

        bool UnregisterHandler(Action<Attr> handler);

        bool UnregisterHandler(Action<string, Attr> handler);

        void RegisterSerializer(IScriptEventSerializer serializer);

        void RegisterParser(IScriptEventParser parser);

        void RegisterBridge(IScriptEventsBridge bridge);

        void Process(string name, Attr args);

        object Parse(string name, Attr args);
    }

    public static class ScriptEventProcessorExtensions
    {
        const string AttrKeyActionName = "name";
        const string AttrKeyActionArguments = "args";

        public static object Parse(this IScriptEventProcessor processor, Attr data)
        {
            if(data.AttrType == AttrType.DICTIONARY)
            {
                return processor.Parse(
                    data.AsDic[AttrKeyActionName].AsValue.ToString(),
                    data.AsDic[AttrKeyActionArguments]);
            }
            if(data.AttrType == AttrType.LIST)
            {
                return processor.Parse(
                    data.AsList[0].AsValue.ToString(),
                    data.AsList[1]);
            }
            if(data.AttrType == AttrType.VALUE)
            {
                return processor.Parse(
                    data.AsValue.ToString(),
                    new AttrEmpty());
            }
            return null;
        }

        public static void RegisterConverter(this IScriptEventProcessor processor, IScriptEventConverter converter)
        {
            processor.RegisterSerializer(converter);
            processor.RegisterParser(converter);
        }
    }

    public interface IScriptEventsBridge : IDisposable
    {
        void Load(IScriptEventProcessor scriptProcessor, IEventProcessor processor);
    }

    public interface IScriptCondition
    {
        bool Matches(string name, Attr arguments);
    }

    public struct ScriptEvent
    {
        public string Name;
        public Attr Arguments;
    }

    public sealed class ScriptEventProcessor : IScriptEventProcessor, IEventHandler<object>
    {
        class EventHandlerWrapper : IStateValidatedEventHandler<ScriptEvent>
        {
            readonly string _name;
            readonly Action<Attr> _action;

            public EventHandlerWrapper(string name, Action<Attr> action)
            {
                _name = name;
                _action = action;
            }

            public void Handle(object state, ScriptEvent ev, bool success, object result)
            {
                if(_action != null && _name == ev.Name)
                {
                    _action(ev.Arguments);
                }
            }
        }

        class EventHandlerDefaultWrapper : IStateValidatedEventHandler<ScriptEvent>
        {
            readonly Action<string, Attr> _action;

            public EventHandlerDefaultWrapper(Action<string, Attr> action)
            {
                _action = action;
            }

            public void Handle(object state, ScriptEvent ev, bool success, object result)
            {
                if(_action != null)
                {
                    _action(ev.Name, ev.Arguments);
                }
            }
        }

        class EventHandlerConditionWrapper : IStateValidatedEventHandler<ScriptEvent>
        {
            readonly IScriptCondition _condition;
            readonly Action<string, Attr> _action;

            public EventHandlerConditionWrapper(IScriptCondition condition, Action<string, Attr> action)
            {
                _condition = condition;
                _action = action;
            }

            public void Handle(object state, ScriptEvent ev, bool success, object result)
            {
                if(_action != null && _condition.Matches(ev.Name, ev.Arguments))
                {
                    _action(ev.Name, ev.Arguments);
                }
            }
        }

        readonly EventProcessor _processor;
        readonly EventProcessor<ScriptEvent> _scriptProcessor;

        readonly List<IScriptEventParser> _parsers;
        readonly List<IScriptEventSerializer> _serializers;
        readonly List<IScriptEventsBridge> _bridges;

        public ScriptEventProcessor()
        {
            _processor = new EventProcessor();
            _scriptProcessor = new EventProcessor<ScriptEvent>();
            _parsers = new List<IScriptEventParser>();
            _serializers = new List<IScriptEventSerializer>();
            _bridges = new List<IScriptEventsBridge>();

            _processor.DerivedEventSupport = true;
            _processor.RegisterHandler(this);
        }

        public void Dispose()
        {
            _processor.UnregisterHandler(this);
            for(int i = 0, _bridgesCount = _bridges.Count; i < _bridgesCount; i++)
            {
                var bridge = _bridges[i];
                bridge.Dispose();
            }
            _scriptProcessor.Dispose();
            _serializers.Clear();
            _parsers.Clear();
            _bridges.Clear();
        }

        public void RegisterBridge(IScriptEventsBridge bridge)
        {
            if(bridge != null && !_bridges.Contains(bridge))
            {
                bridge.Load(this, _processor);
                _bridges.Add(bridge);
            }
        }

        public void RegisterHandler(string name, Action<Attr> listener)
        {
            _scriptProcessor.DoRegisterHandler(listener, new EventHandlerWrapper(name, listener));
        }

        public void RegisterHandler(IScriptCondition condition, Action<string, Attr> listener)
        {
            _scriptProcessor.DoRegisterHandler(listener, new EventHandlerConditionWrapper(condition, listener));
        }

        public void RegisterHandler(Action<string, Attr> listener)
        {
            _scriptProcessor.DoRegisterHandler(listener, new EventHandlerDefaultWrapper(listener));
        }

        public bool UnregisterHandler(Action<Attr> listener)
        {
            return _scriptProcessor.DoUnregisterHandler<ScriptEvent>(listener);
        }

        public bool UnregisterHandler(Action<string, Attr> listener)
        {           
            return _scriptProcessor.DoUnregisterHandler<ScriptEvent>(listener);
        }
            
        public void RegisterSerializer(IScriptEventSerializer serializer)
        {
            if(!_serializers.Contains(serializer))
            {
                _serializers.Add(serializer);
            }
        }

        public void RegisterParser(IScriptEventParser parser)
        {
            if(!_parsers.Contains(parser))
            {
                _parsers.Add(parser);
            }
        }

        public object Parse(string name, Attr args)
        {
            IScriptEventParser parser = null;
            for(int i = 0, _parsersCount = _parsers.Count; i < _parsersCount; i++)
            {
                var p = _parsers[i];
                if(p.Name == name)
                {
                    parser = p;
                }
            }
            if(parser != null)
            {
                return parser.Parse(args);
            }
            return new ScriptEvent {
                Name = name,
                Arguments = args
            };
        }

        public void Process(string name, Attr args)
        {
            var ev = Parse(name, args);
            if(ev != null)
            {
                _processor.Process(ev);
            }
            else
            {
                Handle(name, args);
            }
        }

        void IEventHandler<object>.Handle(object ev)
        {
            Attr args = null;
            string name = null;
            if(ev is ScriptEvent)
            {
                var sev = (ScriptEvent)ev;
                args = sev.Arguments;
                name = sev.Name;
            }
            else
            {
                for(int i = 0, _serializersCount = _serializers.Count; i < _serializersCount; i++)
                {
                    var serializer = _serializers[i];
                    if(serializer != null)
                    {
                        args = serializer.Serialize(ev);
                        if(args != null)
                        {
                            name = serializer.Name;
                            break;
                        }
                    }
                }
            }
            Handle(name, args);
        }

        void Handle(string name, Attr args)
        {
            _scriptProcessor.Process(new ScriptEvent {
                Name = name,
                Arguments = args
            });
        }

    }
}