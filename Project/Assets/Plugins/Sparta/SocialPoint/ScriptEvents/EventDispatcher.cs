using System;
using System.Collections.Generic;

namespace SocialPoint.ScriptEvents
{
    public interface IEventDispatcher : IDisposable
    {
        void AddDefaultListener(Action<object> listener);

        bool RemoveDefaultListener(Action<object> listener);

        void AddBridge(IEventsBridge bridge);

        void Raise(object e);

        Dictionary<Type, List<Delegate>> Listeners { get; }
    }

    public static class EventDispatcherExtensions
    {
        public static void AddListener<T>(this IEventDispatcher dispatcher, Action<T> action)
        {
            var ttype = typeof(T);
            List<Delegate> d;
            if(!dispatcher.Listeners.TryGetValue(ttype, out d))
            {
                d = new List<Delegate>();
                dispatcher.Listeners[ttype] = d;
            }
            if(!d.Contains(action))
            {
                d.Add(action);
            }
        }

        public static bool RemoveListener<T>(this IEventDispatcher dispatcher, Action<T> action)
        {
            List<Delegate> d;
            return dispatcher.Listeners.TryGetValue(typeof(T), out d) && d.Remove(action);
        }

        public static Action<F> Connect<F,T>(this IEventDispatcher dispatcher, Func<F, T> conversion = null)
        {
            Action<F> action = from => {
                if(conversion == null)
                {
                    dispatcher.Raise(default(T));
                }
                else
                {
                    dispatcher.Raise(conversion(from));
                }
            };
            dispatcher.AddListener<F>(action);
            return action;
        }
    }

    public interface IEventsBridge : IDisposable
    {
        void Load(IEventDispatcher dispatcher);
    }

    public sealed class EventDispatcher : IEventDispatcher
    {
        readonly Dictionary<Type, List<Delegate>> _listeners = new Dictionary<Type, List<Delegate>>();
        readonly List<IEventDispatcher> _dispatchers = new List<IEventDispatcher>();
        readonly List<IEventsBridge> _bridges = new List<IEventsBridge>();
        readonly List<Action<object>> _defaultListeners = new List<Action<object>>();

        public event Action<Exception> ExceptionThrown;

        public Dictionary<Type, List<Delegate>> Listeners
        {
            get
            {
                return _listeners;
            }
        }

        public void Dispose()
        {
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
            _dispatchers.Clear();
            _bridges.Clear();
            _defaultListeners.Clear();
        }

        public void AddBridges(IEnumerable<IEventsBridge> bridges)
        {
            var itr = bridges.GetEnumerator();
            while(itr.MoveNext())
            {
                var bridge = itr.Current;
                AddBridge(bridge);
            }
            itr.Dispose();
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

        public void AddDefaultListener(Action<object> listener)
        {
            if(!_defaultListeners.Contains(listener))
            {
                _defaultListeners.Add(listener);
            }
        }

        public bool RemoveDefaultListener(Action<object> listener)
        {
            return _defaultListeners.Remove(listener);
        }

        public void Raise(object ev)
        {
            if(ev == null)
            {
                throw new ArgumentNullException("e");
            }

            // default listeners
            var ddlgList = new List<Action<object>>(_defaultListeners);
            for(int i = 0, ddlgListCount = ddlgList.Count; i < ddlgListCount; i++)
            {
                var action = ddlgList[i];
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

            // event listeners
            List<Delegate> dlgList;
            if(_listeners.TryGetValue(evType, out dlgList))
            {
                // You need to create a copy of the delegates because TryGetValue returns a reference to the internal list
                // of delegates, so if an event listener modifies the delegate list while you are iterating it you'll run
                // into problems. This can happen, for example, if the event listener unregisters itself
                dlgList = new List<Delegate>(dlgList);

                for(int i = 0, dlgListCount = dlgList.Count; i < dlgListCount; i++)
                {
                    var dlg = dlgList[i];
                    if(dlg != null)
                    {
                        try
                        {
                            // TODO: find solution that does not use reflection
                            var method = dlg.GetType().GetMethod("Invoke");
                            method.Invoke(dlg, new[] {
                                ev
                            });
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

            for(int i = 0, _dispatchersCount = _dispatchers.Count; i < _dispatchersCount; i++)
            {
                var dispatcher = _dispatchers[i];
                dispatcher.Raise(ev);
            }
        }

    }
}