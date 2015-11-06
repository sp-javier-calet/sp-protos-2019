using System;
using System.Collections.Generic;

namespace SocialPoint.ScriptEvents
{
    public interface IEventDispatcher
    {
        void AddListener<T>(Action<T> listener);        
        bool RemoveListener<T>(Action<T> listener);
        void Raise<T>(T e);
    }

    public class EventDispatcher : IEventDispatcher
    {
        readonly Dictionary<Type, List<Delegate>> _delegates = new Dictionary<Type, List<Delegate>>();
        
        List<Delegate> _defaultDelegate = new List<Delegate>();
        
        readonly List<EventDispatcher> _dispatchers = new List<EventDispatcher>();

        public event Action<Exception> ExceptionThrown;
        
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
            if(!_delegates.TryGetValue(typeof(T), out d))
            {
                d = new List<Delegate>();
                _delegates[typeof(T)] = d;
            }
            if(!d.Contains(listener))
                d.Add(listener);
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
            if(!_defaultDelegate.Contains(listener))
            {
                _defaultDelegate.Add(listener);
            }
        }
        
        public void RemoveDefaultListener(Action<object> listener)
        {
            _defaultDelegate.Remove(listener);
        }
        
        public void Raise<T>(T e)
        {
            if(e == null)
            {
                throw new ArgumentNullException("e");
            }
            for(int i = 0; i < _defaultDelegate.Count; ++i)
            {
                try
                {
                    (_defaultDelegate[i] as Action<object>)(e);
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
            
            List<Delegate> dlgList = GetDelegateListForEventType(e);
            
            for(int i = 0; i < dlgList.Count; ++i)
            {
                var callback = dlgList[i] as Action<T>;
                
                if(callback != null)
                {
                    try
                    {
                        callback(e);
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
            
            for(int k = 0; k < _dispatchers.Count; k++)
            {
                EventDispatcher dispatcher = _dispatchers[k];
                dispatcher.Raise<T>(e);
            }
        }
        
        public void UnregisterAll()
        {
            _delegates.Clear();
            _defaultDelegate.Clear();
            _dispatchers.Clear();
        }
        
        /// <summary>
        ///     Gets a COPY of the list of delegates registered for a given event type
        /// </summary>
        /// <remarks>
        ///     You need to create a copy of the delegates because TryGetValue returns a reference to the internal list
        ///     of delegates, so if an event listener modifies the delegate list while you are iterating it you'll run
        ///     into problems
        ///     This can happen, for example, if the event listener unregisters itself
        /// </remarks>
        List<Delegate> GetDelegateListForEventType<T>(T e)
        {
            List<Delegate> list;
            if(_delegates.TryGetValue(typeof(T), out list))
            {
                return new List<Delegate>(list);
            }
            else
            {
                return new List<Delegate>();
            }
        }
    }
}