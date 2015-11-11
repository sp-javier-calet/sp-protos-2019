using System;
using System.Reflection;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    public interface IEventDispatcher : IDisposable
    {
        void AddListener<T>(Action<T> listener);
        void AddDefaultListener(Action<object> listener);
        bool RemoveListener<T>(Action<T> listener);
        void RemoveDefaultListener(Action<object> listener);
        void Raise(object e);
    }

    public interface IEventsBridge : IDisposable
    {
        void Load(IEventDispatcher dispatcher);
    }

    public class EventDispatcher : IEventDispatcher
    {
        readonly Dictionary<Type, List<Delegate>> _delegates = new Dictionary<Type, List<Delegate>>();
        readonly List<IEventDispatcher> _dispatchers = new List<IEventDispatcher>();
        readonly List<IEventsBridge> _bridges = new List<IEventsBridge>();
        readonly List<Action<object>> _defaultDelegates = new List<Action<object>>();

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
            _dispatchers.Clear();
            _bridges.Clear();
            _defaultDelegates.Clear();
        }

        public void AddBridges(IEnumerable<IEventsBridge> bridges)
        {
            foreach(var bridge in bridges)
            {
                AddBridge(bridge);
            }
        }

        public void AddBridge(IEventsBridge bridge)
        {
            if(bridge != null && !_bridges.Contains(bridge))
            {
                bridge.Load(this);
                _bridges.Add(bridge);
            }
        }
        
        public void AddDispatcher(IEventDispatcher dispatcher)
        {
            if(dispatcher != null && !_dispatchers.Contains(dispatcher))
            {
                _dispatchers.Add(dispatcher);
            }
        }
        
        public bool RemoveDispatcher(IEventDispatcher dispatcher)
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

        public void Raise(object ev)
        {
            if(ev == null)
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
                        action(ev);
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

            var evType = ev.GetType();

            // event delegates
            List<Delegate> dlgList;
            if(_delegates.TryGetValue(evType, out dlgList))
            {
                // You need to create a copy of the delegates because TryGetValue returns a reference to the internal list
                // of delegates, so if an event listener modifies the delegate list while you are iterating it you'll run
                // into problems. This can happen, for example, if the event listener unregisters itself
                dlgList = new List<Delegate>(dlgList);

                foreach(var dlg in dlgList)
                {
                    if(dlg != null)
                    {
                        try
                        {
                            // TODO: find solution that does not use reflection
                            var method = dlg.GetType().GetMethod("Invoke");
                            method.Invoke(dlg, new object[]{ ev });
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

            foreach(var dispatcher in _dispatchers)
            {
                dispatcher.Raise(ev);
            }
        }

    }
}