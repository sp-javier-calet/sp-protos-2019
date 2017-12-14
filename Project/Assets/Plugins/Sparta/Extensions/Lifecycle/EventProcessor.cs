using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Lifecycle
{
    public partial class EventProcessor<S, T, R> : IEventProcessor<S, T, R>, IEventHandler<T>, IStateEventHandler<S, T>, IDisposable
    {
        interface ITypeValidator : ICloneable
        {
            void Register(object key, object obj);
            bool Unregister(object key);
            bool Validate(S state, T ev, out R result);
            void Load(ITypeValidator other);
        }

        class TypeValidator<K> : ITypeValidator where K : T
        {
            readonly Dictionary<object, IStateEventValidator<S, K, R>> _validators;

            public TypeValidator()
            {
                _validators = new Dictionary<object, IStateEventValidator<S, K, R>>();
            }

            public TypeValidator(Dictionary<object, IStateEventValidator<S, K, R>> validators)
            {
                _validators = validators;
            }

            public void Load(ITypeValidator other)
            {
                _validators.Clear();
                var kother = other as TypeValidator<K>;
                if(kother != null)
                {
                    _validators.Merge(kother._validators);
                }
            }

            public object Clone()
            {
                var other = new TypeValidator<K>(new Dictionary<object, IStateEventValidator<S, K, R>>(_validators));
                return other;
            }

            public void Register(object key, object obj)
            {
                var validator = obj as IStateEventValidator<S, K, R>;
                if(validator != null && !_validators.ContainsKey(key))
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
                var kev = (K)ev;
                var success = true;
                var itr = _validators.GetEnumerator();
                var exceptions = new List<Exception>();
                while(itr.MoveNext())
                {
                    try
                    {
                        if(!itr.Current.Value.Validate(state, kev, out result))
                        {
                            success = false;
                            break;
                        }
                    }
                    catch(Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
                itr.Dispose();
                if(exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
                return success;
            }
        }

        interface ITypeHandler : ICloneable
        {
            void Register(object key, object obj);
            bool Unregister(object key);
            bool Handle(S state, T action, bool success, R result);
            void Load(ITypeHandler other);
        }

        class TypeHandler<K> : ITypeHandler where K : T
        {
            readonly Dictionary<object, IStateValidatedEventHandler<S, K, R>> _handlers;

            public TypeHandler()
            {
                _handlers = new Dictionary<object, IStateValidatedEventHandler<S, K, R>>();
            }

            public TypeHandler(Dictionary<object, IStateValidatedEventHandler<S, K, R>> handlers)
            {
                _handlers = handlers;
            }

            public void Load(ITypeHandler other)
            {
                _handlers.Clear();
                var kother = other as TypeHandler<K>;
                if(kother != null)
                {
                    _handlers.Merge(kother._handlers);
                }
            }

            public object Clone()
            {
                var other = new TypeHandler<K>(new Dictionary<object, IStateValidatedEventHandler<S, K, R>>(_handlers));
                return other;
            }

            public void Register(object key, object obj)
            {
                var handler = obj as IStateValidatedEventHandler<S, K, R>;
                if(handler != null && !_handlers.ContainsKey(key))
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
                if(_handlers.Count == 0)
                {
                    return false;
                }
                var kev = (K)ev;
                var itr = _handlers.GetEnumerator();
                var exceptions = new List<Exception>();
                while(itr.MoveNext())
                {
                    try
                    {
                        itr.Current.Value.Handle(state, kev, success, result);
                    }
                    catch(Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
                itr.Dispose();
                if(exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
                return true;
            }
        }

        Dictionary<Type, ITypeValidator> _validators;
        Dictionary<Type, ITypeHandler> _handlers;
        Dictionary<Type, ITypeValidator> _iterValidators;
        Dictionary<Type, ITypeHandler> _iterHandlers;
        int _depth;

        public bool DerivedEventSupport = false;

        public EventProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _handlers = new Dictionary<Type, ITypeHandler>();
            _iterValidators = new Dictionary<Type, ITypeValidator>();
            _iterHandlers = new Dictionary<Type, ITypeHandler>();
        }

        public void Dispose()
        {
            _validators.Clear();
            _handlers.Clear();
            _iterValidators.Clear();
            _iterHandlers.Clear();
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
            if(_depth == 0)
            {
                UpdateContainers();
            }
            _depth++;
            var handled = false;
            try
            {
                var success = Validate(state, ev, out result);
                handled = DoProcess(state, ev, success, result);
            }
            finally
            {
                _depth--;
            }
            return handled;
        }

        void UpdateContainers()
        {
            {
                var itr = _validators.GetEnumerator();
                while(itr.MoveNext())
                {
                    ITypeValidator validator;
                    if(!_iterValidators.TryGetValue(itr.Current.Key, out validator))
                    {
                        validator = (ITypeValidator)itr.Current.Value.Clone();
                        _iterValidators.Add(itr.Current.Key, validator);
                    }
                    else
                    {
                        validator.Load(itr.Current.Value);
                    }
                }
                itr.Dispose();
            }
            {
                var itr = _handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    ITypeHandler handler;
                    if(!_iterHandlers.TryGetValue(itr.Current.Key, out handler))
                    {
                        handler = (ITypeHandler)itr.Current.Value.Clone();
                        _iterHandlers.Add(itr.Current.Key, handler);
                    }
                    else
                    {
                        handler.Load(itr.Current.Value);
                    }
                }
                itr.Dispose();
            }
        }

        bool DoProcess(S state, T ev, bool success, R result)
        {
            var handled = false;
            if(DerivedEventSupport)
            {
                var itr = _iterHandlers.GetEnumerator();
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
                if(_iterHandlers.TryGetValue(ev.GetType(), out handler))
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
                var itr = _iterValidators.GetEnumerator();
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

    public class EventProcessor<K> : EventProcessor<object, K, object>, IEventProcessor<K>
    {
    }

    public class StateEventProcessor<S> : EventProcessor<S, object, object>, IStateEventProcessor<S>
    {
    }

    public class EventProcessor : EventProcessor<object>, IEventProcessor
    {
    }

}
