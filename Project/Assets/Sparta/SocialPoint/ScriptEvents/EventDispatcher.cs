using System;
using System.Collections.Generic;
using SocialPoint.Lifecycle;

namespace SocialPoint.ScriptEvents
{
    public interface IEventDispatcher : IDisposable
    {
        void AddDefaultListener(Action<object> listener);

        bool RemoveDefaultListener(Action<object> listener);

        void AddBridge(IEventsBridge bridge);

        void Raise(object e);

        void AddListener<T>(Action<T> action);

        bool RemoveListener<T>(Action<T> action);

        Action<F> Connect<F,T>(Func<F, T> conversion = null);
    }

    public interface IEventsBridge : IDisposable
    {
        void Load(IEventDispatcher dispatcher);
    }

    public sealed class EventDispatcher : IEventDispatcher
    {
        class DefaultListenerValidator : IStateEventValidator<object, object, object>
        {
            Action<object> _action;

            public DefaultListenerValidator(Action<object> action)
            {
                _action = action;
            }

            public bool Validate(object state, object ev, out object result)
            {
                result = null;
                if(_action != null)
                {
                    _action(ev);
                }
                return true;
            }
        }

        readonly EventProcessor _processor;
        readonly List<IEventsBridge> _bridges;

        public EventDispatcher()
        {
            _processor = new EventProcessor();
            _processor.DerivedEventSupport = true;
            _bridges = new List<IEventsBridge>();
        }

        public void AddDefaultListener(Action<object> listener)
        {
            _processor.DoRegisterValidator(listener, new DefaultListenerValidator(listener));
        }

        public bool RemoveDefaultListener(Action<object> listener)
        {
            return _processor.DoUnregisterValidator<object>(listener);
        }

        public void AddBridge(IEventsBridge bridge)
        {
            if(bridge != null && !_bridges.Contains(bridge))
            {
                bridge.Load(this);
                _bridges.Add(bridge);
            }
        }

        public void Dispose()
        {
            for(int i = 0, _bridgesCount = _bridges.Count; i < _bridgesCount; i++)
            {
                var bridge = _bridges[i];
                bridge.Dispose();
            }
            _bridges.Clear();
        }

        public void Raise(object e)
        {
            _processor.Process(e);
        }

        public void AddListener<T>(Action<T> action)
        {
            _processor.RegisterHandler(action);
        }

        public bool RemoveListener<T>(Action<T> action)
        {
            return _processor.UnregisterHandler(action);
        }

        public Action<F> Connect<F,T>(Func<F, T> conversion = null)
        {
            Action<F> action = from => {
                if(conversion == null)
                {
                    Raise(default(T));
                }
                else
                {
                    Raise(conversion(from));
                }
            };
            AddListener<F>(action);
            return action;
        }
    }
}