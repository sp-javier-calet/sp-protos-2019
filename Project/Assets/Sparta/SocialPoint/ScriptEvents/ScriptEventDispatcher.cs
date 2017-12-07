using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Lifecycle;

namespace SocialPoint.ScriptEvents
{
    public interface IScriptEventDispatcher : IDisposable
    {
        void AddListener(string name, Action<Attr> listener);

        void AddListener(Action<string, Attr> listener);

        void AddListener(IScriptCondition condition, Action<string, Attr> listener);

        bool RemoveListener(Action<Attr> listener);

        bool RemoveListener(Action<string, Attr> listener);

        void AddConverter(IScriptEventConverter converter);

        void AddSerializer(IScriptEventSerializer serializer);

        void AddParser(IScriptEventParser parser);

        void AddBridge(IScriptEventsBridge bridge);

        void Raise(string name, Attr args);

        object Parse(string name, Attr args);
    }

    public static class ScriptEventDispatcherExtension
    {
        const string AttrKeyActionName = "name";
        const string AttrKeyActionArguments = "args";

        public static object Parse(this IScriptEventDispatcher dispatcher, Attr data)
        {
            if(data.AttrType == AttrType.DICTIONARY)
            {
                return dispatcher.Parse(
                    data.AsDic[AttrKeyActionName].AsValue.ToString(),
                    data.AsDic[AttrKeyActionArguments]);
            }
            if(data.AttrType == AttrType.LIST)
            {
                return dispatcher.Parse(
                    data.AsList[0].AsValue.ToString(),
                    data.AsList[1]);
            }
            if(data.AttrType == AttrType.VALUE)
            {
                return dispatcher.Parse(
                    data.AsValue.ToString(),
                    new AttrEmpty());
            }
            return null;
        }
    }

    public interface IScriptEventsBridge : IDisposable
    {
        void Load(IScriptEventDispatcher dispatcher);
    }

    public interface IScriptCondition
    {
        bool Matches(string name, Attr arguments);
    }        

    public sealed class ScriptEventDispatcher : IScriptEventDispatcher
    {
        public struct ScriptEventData
        {
            public string Name;
            public Attr Arguments;
        }

        class EventHandlerWrapper : IStateValidatedEventHandler<ScriptEventData>
        {
            readonly string _name;
            readonly Action<Attr> _action;

            public EventHandlerWrapper(string name, Action<Attr> action)
            {
                _name = name;
                _action = action;
            }

            public void Handle(object state, ScriptEventData ev, bool success, object result)
            {
                if(_action != null && _name == ev.Name)
                {
                    _action(ev.Arguments);
                }
            }
        }

        class EventHandlerDefaultWrapper : IStateValidatedEventHandler<ScriptEventData>
        {
            readonly Action<string, Attr> _action;

            public EventHandlerDefaultWrapper(Action<string, Attr> action)
            {
                _action = action;
            }

            public void Handle(object state, ScriptEventData ev, bool success, object result)
            {
                if(_action != null)
                {
                    _action(ev.Name, ev.Arguments);
                }
            }
        }

        class EventHandlerConditionWrapper : IStateValidatedEventHandler<ScriptEventData>
        {
            readonly IScriptCondition _condition;
            readonly Action<string, Attr> _action;

            public EventHandlerConditionWrapper(IScriptCondition condition, Action<string, Attr> action)
            {
                _condition = condition;
                _action = action;
            }

            public void Handle(object state, ScriptEventData ev, bool success, object result)
            {
                if(_action != null && _condition.Matches(ev.Name, ev.Arguments))
                {
                    _action(ev.Name, ev.Arguments);
                }
            }
        }

        readonly EventProcessor<ScriptEventData> _processor;

        readonly List<IScriptEventParser> _parsers;
        readonly List<IScriptEventSerializer> _serializers;
        readonly List<IScriptEventsBridge> _bridges;
        IEventDispatcher _dispatcher;

        public ScriptEventDispatcher(IEventDispatcher dispatcher)
        {
            _processor = new EventProcessor<ScriptEventData>();
            _parsers = new List<IScriptEventParser>();
            _serializers = new List<IScriptEventSerializer>();
            _bridges = new List<IScriptEventsBridge>();
            _dispatcher = dispatcher;
            _dispatcher.AddDefaultListener(OnRaised);
        }

        public void Dispose()
        {
            _dispatcher.RemoveDefaultListener(OnRaised);
            for(int i = 0, _bridgesCount = _bridges.Count; i < _bridgesCount; i++)
            {
                var bridge = _bridges[i];
                bridge.Dispose();
            }
            _processor.Dispose();
            _serializers.Clear();
            _parsers.Clear();
            _bridges.Clear();
        }

        public void AddBridge(IScriptEventsBridge bridge)
        {
            if(bridge != null && !_bridges.Contains(bridge))
            {
                bridge.Load(this);
                _bridges.Add(bridge);
            }
        }

        public void AddListener(string name, Action<Attr> listener)
        {
            _processor.DoRegisterHandler(listener, new EventHandlerWrapper(name, listener));
        }

        public void AddListener(IScriptCondition condition, Action<string, Attr> listener)
        {
            _processor.DoRegisterHandler(listener, new EventHandlerConditionWrapper(condition, listener));
        }

        public void AddListener(Action<string, Attr> listener)
        {
            _processor.DoRegisterHandler(listener, new EventHandlerDefaultWrapper(listener));
        }

        public bool RemoveListener(Action<Attr> listener)
        {
            return _processor.DoUnregisterHandler<ScriptEventData>(listener);
        }

        public bool RemoveListener(Action<string, Attr> listener)
        {           
            return _processor.DoUnregisterHandler<ScriptEventData>(listener);
        }

        public void AddConverter(IScriptEventConverter converter)
        {
            AddSerializer(converter);
            AddParser(converter);
        }

        public void AddSerializer(IScriptEventSerializer serializer)
        {
            if(!_serializers.Contains(serializer))
            {
                _serializers.Add(serializer);
            }
        }

        public void AddParser(IScriptEventParser parser)
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
            return new ScriptEventData {
                Name = name,
                Arguments = args
            };
        }

        public void Raise(string name, Attr args)
        {
            var ev = Parse(name, args);
            if(ev != null)
            {
                _dispatcher.Raise(ev);
            }
            else
            {
                OnRaised(name, args);
            }
        }

        void OnRaised(object ev)
        {
            Attr args = null;
            string name = null;
            if(ev is ScriptEventData)
            {
                var sev = (ScriptEventData)ev;
                name = sev.Name;
                args = sev.Arguments;
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
                if(args == null || string.IsNullOrEmpty(name))
                {
                    return;
                }
            }

            OnRaised(name, args);
        }

        void OnRaised(string name, Attr args)
        {
            _processor.Process(new ScriptEventData {
                Name = name,
                Arguments = args
            });
        }

    }
}