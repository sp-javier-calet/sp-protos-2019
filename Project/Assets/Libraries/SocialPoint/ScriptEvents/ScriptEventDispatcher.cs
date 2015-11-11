using System;
using System.Linq;
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
        void AddConverter(IScriptEventConverter config);
        void Raise(string name, Attr args);
    }

    public interface IScriptEventsBridge : IDisposable
    {
        void Load(IScriptEventDispatcher dispatcher);
    }

    public interface IScriptEventConverter : ISerializer<object>, IParser<object>
    {
        string Name { get; }
    }

    public abstract class BaseScriptEventConverter<T> : IScriptEventConverter
    {
        public string Name { get; private set; }

        public BaseScriptEventConverter(string name)
        {
            Name = name;
        }

        public object Parse(Attr data)
        {
            return ParseEvent(data);
        }

        abstract protected T ParseEvent(Attr data);

        public Attr Serialize(object ev)
        {
            if(ev is T)
            {
                return SerializeEvent((T)ev);
            }
            return null;
        }

        abstract protected Attr SerializeEvent(T ev);
    }

    public class ScriptEventConverter<T> : BaseScriptEventConverter<T>
    {
        ISerializer<T> _serializer;
        IParser<T> _parser;

        public ScriptEventConverter(string name, IParser<T> parser=null, ISerializer<T> serializer=null): base(name)
        {
            _serializer = serializer;
            _parser = parser;
        }

        override protected T ParseEvent(Attr data)
        {
            if(_parser != null)
            {
                return _parser.Parse(data);
            }
            else
            {
                return default(T);
            }
        }

        override protected Attr SerializeEvent(T ev)
        {

            if(_serializer != null)
            {
                return _serializer.Serialize((T)ev);
            }
            else
            {
                return new AttrEmpty();
            }
        }
    }

    public interface IScriptCondition
    {
        bool Matches(string name, Attr arguments);
    }


    public class ScriptEventDispatcher : IScriptEventDispatcher
    {
        public struct ConditionListener
        {
            public IScriptCondition Condition;
            public Action<string, Attr> Action;
        }

        readonly Dictionary<string, List<Action<Attr>>> _listeners = new Dictionary<string, List<Action<Attr>>>();
        readonly List<Action<string, Attr>> _defaultListeners = new List<Action<string, Attr>>();
        readonly List<IScriptEventConverter> _converters = new List<IScriptEventConverter>();
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
            foreach(var bridge in _bridges)
            {
                bridge.Dispose();
            }
            Clear();
        }
        
        public void Clear()
        {
            _listeners.Clear();
            _defaultListeners.Clear();
            _converters.Clear();
            _bridges.Clear();
        }

        public void AddBridges(IEnumerable<IScriptEventsBridge> bridges)
        {
            foreach(var bridge in bridges)
            {
                AddBridge(bridge);
            }
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
            foreach(var kvp in _listeners)
            {
                if(kvp.Value.Remove(listener))
                {
                    found = true;
                }
            }
            return found;
        }
        
        public bool RemoveListener(Action<string, Attr> listener)
        {           
            bool found = false;
            if(_defaultListeners.Remove(listener))
            {
                found = true;
            }
            if(_conditionListeners.RemoveAll(l => l.Action == listener) > 0)
            {
                found = true;
            }
            return found;
        }

        public void AddConverter(IScriptEventConverter serializer)
        {
            if(!_converters.Contains(serializer))
            {
                _converters.Add(serializer);
            }
        }

        public void Raise(string name, Attr args)
        {
            var converter = _converters.FirstOrDefault(c => c.Name == name);
            if(converter != null)
            {
                var ev = converter.Parse(args);
                _dispatcher.Raise(ev);
            }
        }

        public void OnRaised(object ev)
        {
            Attr data = null;
            string name = null;
            foreach(var converter in _converters)
            {
                if(converter != null)
                {
                    data = converter.Serialize(ev);
                    if(data != null)
                    {
                        name = converter.Name;
                        break;
                    }
                }
            }
            if(data == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            // default  listeners
            var ddlgList = new List<Action<string, Attr>>(_defaultListeners);
            foreach(var dlg in ddlgList)
            {
                if(dlg != null)
                {
                    try
                    {
                        dlg(name, data);
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
                foreach(var dlg in dlgList)
                {
                    if(dlg != null)
                    {
                        try
                        {
                            dlg(data);
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
            foreach(var listener in cdlgList)
            {
                if(listener.Action != null && (listener.Condition == null || listener.Condition.Matches(name, data)))
                {
                    try
                    {
                        listener.Action(name, data);
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