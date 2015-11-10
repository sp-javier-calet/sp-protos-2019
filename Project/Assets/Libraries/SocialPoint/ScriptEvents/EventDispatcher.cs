using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    public interface IEventDispatcher : IDisposable
    {
        void AddListener<T>(Action<T> listener);        
        bool RemoveListener<T>(Action<T> listener);
        void Raise<T>(T e);
    }

    public interface IEventsBridge : IDisposable
    {
        void Load(IEventDispatcher dispatcher);
    }

    public class ScriptEventConfiguration
    {
        public Type Type { get; private set; }
        public string Name { get; private set; }
        public object Serializer { get; private set; }

        ScriptEventConfiguration(Type type, string name, object serializer)
        {
            Type = type;
            Name = name;
            Serializer = serializer;
        }

        public static ScriptEventConfiguration Create<T>(string name, ISerializer<T> serializer)
        {
            return new ScriptEventConfiguration(typeof(T), name, serializer);
        }
    }

    public class EventDispatcher : IEventDispatcher
    {
        readonly Dictionary<Type, List<Delegate>> _delegates = new Dictionary<Type, List<Delegate>>();
        readonly List<EventDispatcher> _dispatchers = new List<EventDispatcher>();
        readonly List<IEventsBridge> _bridges = new List<IEventsBridge>();
        readonly List<Action<object>> _defaultDelegates = new List<Action<object>>();
        readonly Dictionary<string, List<Action<Attr>>> _scriptDelegates = new Dictionary<string, List<Action<Attr>>>();
        readonly List<Action<string, Attr>> _defaultScriptDelegates = new List<Action<string, Attr>>();
        readonly Dictionary<Type, ScriptEventConfiguration> _eventSerializers = new Dictionary<Type, ScriptEventConfiguration>();

        public event Action<Exception> ExceptionThrown;

        public void Dispose()
        {
            foreach(var bridge in _bridges)
            {
                bridge.Dispose();
            }
            Clear();
        }

        public void Clear()
        {
            _delegates.Clear();
            _defaultDelegates.Clear();
            _dispatchers.Clear();
            _scriptDelegates.Clear();
            _defaultScriptDelegates.Clear();
            _eventSerializers.Clear();
        }

        public void AddBridge(IEventsBridge bridge)
        {
            if(!_bridges.Contains(bridge))
            {
                bridge.Load(this);
                _bridges.Add(bridge);
            }
        }
        
        public void AddDispatcher(EventDispatcher dispatcher)
        {
            _dispatchers.Add(dispatcher);
        }
        
        public bool RemoveDispatcher(EventDispatcher dispatcher)
        {
            return _dispatchers.Remove(dispatcher);
        }
        
        public void AddListener<T>(Action<T> listener)
        {
            List<Delegate> d;
            var ttype = typeof(T);
            if(!_delegates.TryGetValue(ttype, out d))
            {
                d = new List<Delegate>();
                _delegates[ttype] = d;
            }
            if(!d.Contains(listener))
            {
                d.Add(listener);
            }
        }
        
        public bool RemoveListener<T>(Action<T> listener)
        {
            List<Delegate> d;
            if(_delegates.TryGetValue(typeof(T), out d))
            {
                d.Remove(listener);
                return true;
            }
            return false;
        }
        
        public void AddDefaultListener(Action<object> listener)
        {
            if(!_defaultDelegates.Contains(listener))
            {
                _defaultDelegates.Add(listener);
            }
        }
        
        public void RemoveDefaultListener(Action<object> listener)
        {
            _defaultDelegates.Remove(listener);
        }

        public void AddScriptListener(string name, Action<Attr> listener)
        {
            List<Action<Attr>> d;
            if(!_scriptDelegates.TryGetValue(name, out d))
            {
                d = new List<Action<Attr>>();
                _scriptDelegates[name] = d;
            }
            d.Add(listener);
        }

        public void AddDefaultScriptListener(Action<string, Attr> listener)
        {
            if(!_defaultScriptDelegates.Contains(listener))
            {
                _defaultScriptDelegates.Add(listener);
            }
        }

        public bool RemoveScriptListener(Action<Attr> listener)
        {
            bool found = false;
            foreach(var kvp in _scriptDelegates)
            {
                if(kvp.Value.Remove(listener))
                {
                    found = true;
                }
            }
            return found;
        }
        
        public void RemoveDefaultScriptListener(Action<string, Attr> listener)
        {
            _defaultScriptDelegates.Remove(listener);
        }

        public void AddSerializer(ScriptEventConfiguration config)
        {
            _eventSerializers[config.Type] = config;
        }

        public void Raise<T>(T e)
        {
            if(e == null)
            {
                throw new ArgumentNullException("e");
            }

            // default delegates
            var ddlgList = new List<Action<object>>(_defaultDelegates);
            foreach(var action in ddlgList)
            {
                if(action != null)
                {
                    try
                    {
                        action(e);
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

            Type ttype = typeof(T);

            // event delegates
            List<Delegate> dlgList;
            if(_delegates.TryGetValue(ttype, out dlgList))
            {
                // You need to create a copy of the delegates because TryGetValue returns a reference to the internal list
                // of delegates, so if an event listener modifies the delegate list while you are iterating it you'll run
                // into problems. This can happen, for example, if the event listener unregisters itself
                dlgList = new List<Delegate>(dlgList);
                foreach(var dlg in dlgList)
                {
                    var action = dlg as Action<T>;                    
                    if(action != null)
                    {
                        try
                        {
                            action(e);
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
            
            ScriptEventConfiguration config;
            if(_eventSerializers.TryGetValue(ttype, out config))
            {
                var serializer = config.Serializer as ISerializer<T>;
                Attr data = null;
                if(serializer != null)
                {
                    data = serializer.Serialize(e);
                }
                // default script delegates
                var sddlgList = new List<Action<string, Attr>>(_defaultScriptDelegates);
                foreach(var dlg in sddlgList)
                {
                    if(dlg != null)
                    {
                        try
                        {
                           dlg(config.Name, data);
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
                // script delegates
                List<Action<Attr>> sdlgList;
                if(_scriptDelegates.TryGetValue(config.Name, out sdlgList))
                {
                    foreach(var dlg in sdlgList)
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
            }
            
            foreach(var dispatcher in _dispatchers)
            {
                if(dispatcher != null)
                {
                    dispatcher.Raise<T>(e);
                }
            }
        }

    }
}