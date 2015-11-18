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
        void AddConverter(IScriptEventConverter converter);
        void AddSerializer(IScriptEventSerializer serializer);
        void AddParser(IScriptEventParser parser);
        void AddBridge(IScriptEventsBridge bridge);
        void Raise(string name, Attr args);
    }

    public interface IScriptEventsBridge : IDisposable
    {
        void Load(IScriptEventDispatcher dispatcher);
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
            _serializers.Clear();
            _parsers.Clear();
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

        public void Raise(string name, Attr args)
        {
            var parser = _parsers.FirstOrDefault(c => c.Name == name);
            if(parser != null)
            {
                var ev = parser.Parse(args);
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
            foreach(var serializer in _serializers)
            {
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

            OnRaised(name, args);
        }

        void OnRaised(string name, Attr args)
        {
            // default  listeners
            var ddlgList = new List<Action<string, Attr>>(_defaultListeners);
            foreach(var dlg in ddlgList)
            {
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
                foreach(var dlg in dlgList)
                {
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
            foreach(var listener in cdlgList)
            {
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