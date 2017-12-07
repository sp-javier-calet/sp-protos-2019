using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public partial class EventProcessor<S, T, R> : IEventProcessor<S, T, R>, IEventHandler<T>, IStateEventHandler<S, T>, IDisposable
    {
        interface ITypeValidator
        {
            void Register(object key, object obj);
            bool Unregister(object key);
            bool Validate(S state, T ev, out R result);
        }

        class TypeValidator<K> : ITypeValidator where K : T
        {
            readonly Dictionary<object, IStateEventValidator<S, K, R>> _validators = new Dictionary<object, IStateEventValidator<S, K, R>>();
            readonly Dictionary<object, IStateEventValidator<S, K, R>> _tempValidators = new Dictionary<object, IStateEventValidator<S, K, R>>();
            int _depth;

            public void Register(object key, object obj)
            {
                var validator = obj as IStateEventValidator<S, K, R>;
                if(validator != null)
                {
                    _validators.Add(key, validator);
                }
            }

            public bool Unregister(object obj)
            {
                return _validators.Remove(obj);
            }

            public bool Validate(S state, T ev, out R result)
            {
                result = default(R);
                if(!(ev is K))
                {
                    return true;
                }
                if(_validators.Count == 0)
                {
                    return true;
                }
                if(_depth == 0)
                {
                    _tempValidators.Clear();
                    _tempValidators.Merge(_validators);
                }
                _depth++;
                var kev = (K)ev;
                var success = true;
                var itr = _tempValidators.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(!itr.Current.Value.Validate(state, kev, out result))
                    {
                        success = false;
                        break;
                    }
                }
                itr.Dispose();
                _depth--;
                return success;
            }
        }

        interface ITypeHandler
        {
            void Register(object key, object obj);
            bool Unregister(object key);
            bool Handle(S state, T action, bool success, R result);
        }

        class TypeHandler<K> : ITypeHandler where K : T
        {
            readonly Dictionary<object, IStateValidatedEventHandler<S, K, R>> _handlers = new Dictionary<object, IStateValidatedEventHandler<S, K, R>>();
            readonly Dictionary<object, IStateValidatedEventHandler<S, K, R>> _tempHandlers = new Dictionary<object, IStateValidatedEventHandler<S, K, R>>();
            int _depth;

            public void Register(object key, object obj)
            {
                var handler = obj as IStateValidatedEventHandler<S, K, R>;
                if(handler != null)
                {
                    _handlers.Add(key, handler);
                }
            }

            public bool Unregister(object key)
            {
                return _handlers.Remove(key);
            }

            public bool Handle(S state, T ev, bool success, R result)
            {
                if(!(ev is K))
                {
                    return false;
                }
                if(_depth == 0)
                {
                    _tempHandlers.Clear();
                    _tempHandlers.Merge(_handlers);
                }
                if(_tempHandlers.Count == 0)
                {
                    return false;
                }
                _depth++;
                var kev = (K)ev;
                var itr = _tempHandlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.Value.Handle(state, kev, success, result);
                }
                itr.Dispose();
                _depth--;
                return true;
            }
        }

        Dictionary<Type, ITypeValidator> _validators;
        Dictionary<Type, ITypeHandler> _handlers;

        public bool DerivedEventSupport = false;

        public EventProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _handlers = new Dictionary<Type, ITypeHandler>();
        }

        public void Dispose()
        {
            _validators.Clear();
            _handlers.Clear();
        }

        void IEventHandler<T>.Handle(T ev)
        {
            Process(ev);
        }

        void IStateEventHandler<S, T>.Handle(S state, T ev)
        {
            Process(state, ev);
        }

        public bool Process(T ev)
        {
            R result;
            return Process(ev, out result);
        }

        public bool Process(S state, T ev)
        {
            R result;
            return Process(state, ev, out result);
        }

        public bool Process(T ev, out R result)
        {
            var state = default(S);
            return Process(state, ev, out result);
        }

        public bool Process(S state, T ev, out R result)
        {
            var success = Validate(state, ev, out result);
            var handled = false;
            if(DerivedEventSupport)
            {
                var itr = _handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Key.IsAssignableFrom(ev.GetType()))
                    {
                        if(itr.Current.Value.Handle(state, ev, success, result))
                        {
                            handled = true;
                        }
                    }
                }
                itr.Dispose();
            }
            else
            {
                ITypeHandler handler;
                if(_handlers.TryGetValue(ev.GetType(), out handler))
                {
                    handled = handler.Handle(state, ev, success, result);
                }
            }
            return handled;
        }

        bool Validate(S state, T ev, out R result)
        {
            result = default(R);
            var success = true;
            if(DerivedEventSupport)
            {
                var itr = _validators.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Key.IsAssignableFrom(ev.GetType()))
                    {
                        if(!itr.Current.Value.Validate(state, ev, out result))
                        {
                            success = false;
                            break;
                        }
                    }
                }
                itr.Dispose();
            }
            else
            {
                ITypeValidator validator;
                if(_validators.TryGetValue(ev.GetType(), out validator))
                {
                    success = validator.Validate(state, ev, out result);
                }
            }
            return success;
        }

        public void DoRegisterHandler<K>(object key, IStateValidatedEventHandler<S, K, R> obj) where K : T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(!_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                _handlers[type] = typeHandler;
            }
            typeHandler.Register(key, obj);
        }

        public bool DoUnregisterHandler<K>(object key) where K : T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                return typeHandler.Unregister(key);
            }
            return false;
        }

        public void DoRegisterValidator<K>(object key, IStateEventValidator<S, K, R> validator) where K : T
        {
            var type = typeof(K);
            ITypeValidator typeValidator;
            if(!_validators.TryGetValue(type, out typeValidator))
            {
                typeValidator = new TypeValidator<K>();
                _validators[type] = typeValidator;
            }
            typeValidator.Register(key, validator);
        }

        public bool DoUnregisterValidator<K>(object key) where K : T
        {
            var type = typeof(K);
            ITypeValidator typeValidator;
            if(_validators.TryGetValue(type, out typeValidator))
            {
                return typeValidator.Unregister(key);
            }
            return false;
        }

        public void Unregister<K>() where K : T
        {
            UnregisterHandlers<K>();
            UnregisterValidators<K>();
        }

        public void UnregisterHandlers<K>() where K : T
        {
            _handlers.Remove(typeof(K));
        }

        public void UnregisterValidators<K>() where K : T
        {
            _validators.Remove(typeof(K));
        }
    }

    public class EventProcessor<K> : EventProcessor<object, K, object>
    {
    }

    public class StateEventProcessor<S> : EventProcessor<S, object, object>
    {
    }

    public class EventProcessor : EventProcessor<object>
    {
    }

}
