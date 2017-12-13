using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

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

    public struct ScriptEventAction
    {
        public string Name;
        public Attr Arguments;
    }

    public sealed class ScriptEventDispatcher : IScriptEventDispatcher
    {
        public struct ConditionListener
        {
            public IScriptCondition Condition;
            public Action<string, Attr> Action;
        }

        readonly Dictionary<string, List<Action<Attr>>> _listeners = new Dictionary<string, List<Action<Attr>>>();
        readonly List<Action<string, Attr>> _defaultListeners = new List<Action<string, Attr>>();
        readonly List<IScriptEventParser> _parsers = new List<IScriptEventParser>();
        readonly List<IScriptEventSerializer> _serializers = new List<IScriptEventSerializer>();
        readonly List<IScriptEventsBridge> _bridges = new List<IScriptEventsBridge>();
        readonly List<ConditionListener> _conditionListeners = new List<ConditionListener>();
        IEventDispatcher _dispatcher;

        public event Action<Exception> ExceptionThrown;

        public ScriptEventDispatcher(IEventDispatcher dispatcher)
        {
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
            Clear();
        }

        public void Clear()
        {
            _listeners.Clear();
            _defaultListeners.Clear();
            _serializers.Clear();
            _parsers.Clear();
            _bridges.Clear();
        }

        public void AddBridges(IEnumerable<IScriptEventsBridge> bridges)
        {
            var itr = bridges.GetEnumerator();
            while(itr.MoveNext())
            {
                var bridge = itr.Current;
                AddBridge(bridge);
            }
            itr.Dispose();
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
            List<Action<Attr>> d;
            if(!_listeners.TryGetValue(name, out d))
            {
                d = new List<Action<Attr>>();
                _listeners[name] = d;
            }
            d.Add(listener);
        }

        public void AddListener(IScriptCondition condition, Action<string, Attr> action)
        {
            var listener = new ConditionListener{ Condition = condition, Action = action };
            if(!_conditionListeners.Contains(listener))
            {
                _conditionListeners.Add(listener);
            }
        }

        public void AddListener(Action<string, Attr> listener)
        {
            if(!_defaultListeners.Contains(listener))
            {
                _defaultListeners.Add(listener);
            }
        }

        public bool RemoveListener(Action<Attr> listener)
        {
            bool found = false;
            var itr = _listeners.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                found |= kvp.Value.Remove(listener);
            }
            itr.Dispose();
            return found;
        }

        public bool RemoveListener(Action<string, Attr> listener)
        {           
            bool found = false || _defaultListeners.Remove(listener);
            found |= _conditionListeners.RemoveAll(l => l.Action == listener) > 0;
            return found;
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
            return new ScriptEventAction {
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
            if(ev is ScriptEventAction)
            {
                var sev = (ScriptEventAction)ev;
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
            // default  listeners
            var ddlgList = new List<Action<string, Attr>>(_defaultListeners);
            for(int i = 0, ddlgListCount = ddlgList.Count; i < ddlgListCount; i++)
            {
                var dlg = ddlgList[i];
                if(dlg != null)
                {
                    try
                    {
                        dlg(name, args);
                    }
                    catch(Exception ex)
                    {
                        if(ExceptionThrown != null)
                        {
                            ExceptionThrown(ex);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
            
            // event listeners
            List<Action<Attr>> dlgList;
            if(_listeners.TryGetValue(name, out dlgList))
            {
                dlgList = new List<Action<Attr>>(dlgList);
                for(int i = 0, dlgListCount = dlgList.Count; i < dlgListCount; i++)
                {
                    var dlg = dlgList[i];
                    if(dlg != null)
                    {
                        try
                        {
                            dlg(args);
                        }
                        catch(Exception ex)
                        {
                            if(ExceptionThrown != null)
                            {
                                ExceptionThrown(ex);
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
            
            // condition listeners
            var cdlgList = new List<ConditionListener>(_conditionListeners);
            for(int i = 0, cdlgListCount = cdlgList.Count; i < cdlgListCount; i++)
            {
                var listener = cdlgList[i];
                if(listener.Action != null && (listener.Condition == null || listener.Condition.Matches(name, args)))
                {
                    try
                    {
                        listener.Action(name, args);
                    }
                    catch(Exception ex)
                    {
                        if(ExceptionThrown != null)
                        {
                            ExceptionThrown(ex);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

    }
}