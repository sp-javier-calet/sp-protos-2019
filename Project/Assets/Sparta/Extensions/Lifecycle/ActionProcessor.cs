using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public partial class ActionProcessor<S, T, R> : IActionProcessor<S, T, R>, IActionHandler<T>, IStateActionHandler<S, T>, IDisposable
    {
        interface ITypeValidator
        {
            void Register(object key, object obj);
            void Unregister(object key);
            bool Validate(S state, T action, out R result);
        }

        class TypeValidator<K> : ITypeValidator where K : T
        {
            Dictionary<object, IStateActionValidator<S, K, R>> _validators = new Dictionary<object, IStateActionValidator<S, K, R>>();

            public void Register(object key, object obj)
            {
                var validator = obj as IStateActionValidator<S, K, R>;
                if(validator != null)
                {
                    _validators.Add(key, validator);
                }
            }

            public void Unregister(object obj)
            {
                _validators.Remove(obj);
            }

            public bool Validate(S state, T action, out R result)
            {
                result = default(R);
                if(!(action is K))
                {
                    return true;
                }
                if(_validators.Count == 0)
                {
                    return true;
                }
                var kaction = (K)action;
                var itr = _validators.GetEnumerator();
                var success = true;
                while(itr.MoveNext())
                {
                    if(!itr.Current.Value.Validate(state, kaction, out result))
                    {
                        success = false;
                        break;
                    }
                }
                itr.Dispose();
                return success;
            }
        }

        interface ITypeHandler
        {
            void Register(object key, object obj);
            void Unregister(object key);
            bool Handle(S state, T action, bool success, R result);
        }

        class TypeHandler<K> : ITypeHandler where K : T
        {
            Dictionary<object, IStateValidatedActionHandler<S, K, R>> _handlers = new Dictionary<object, IStateValidatedActionHandler<S, K, R>>();

            public void Register(object key, object obj)
            {
                var handler = obj as IStateValidatedActionHandler<S, K, R>;
                if(handler != null)
                {
                    _handlers.Add(key, handler);
                }
            }

            public void Unregister(object key)
            {
                _handlers.Remove(key);
            }

            public bool Handle(S state, T action, bool success, R result)
            {
                if(!(action is K))
                {
                    return false;
                }
                if(_handlers.Count == 0)
                {
                    return false;
                }
                var kaction = (K)action;
                var itr = _handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.Value.Handle(state, kaction, success, result);
                }
                itr.Dispose();
                return true;
            }
        }

        Dictionary<Type, ITypeValidator> _validators;
        Dictionary<Type, ITypeHandler> _handlers;

        public bool DerivedActionSupport = false;

        public ActionProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _handlers = new Dictionary<Type, ITypeHandler>();
        }

        public void Dispose()
        {
            _validators.Clear();
            _handlers.Clear();
        }

        void IActionHandler<T>.Handle(T action)
        {
            Process(action);
        }

        void IStateActionHandler<S, T>.Handle(S state, T action)
        {
            Process(state, action);
        }

        public bool Process(T action)
        {
            R result;
            return Process(action, out result);
        }

        public bool Process(S state, T action)
        {
            R result;
            return Process(state, action, out result);
        }

        public bool Process(T action, out R result)
        {
            var state = default(S);
            return Process(state, action, out result);
        }

        public bool Validate(S state, T action, out R result)
        {
            result = default(R);
            var success = true;
            if(DerivedActionSupport)
            {
                var itr = _validators.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Key.IsAssignableFrom(action.GetType()))
                    {
                        if(!itr.Current.Value.Validate(state, action, out result))
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
                if(_validators.TryGetValue(action.GetType(), out validator))
                {
                    success = validator.Validate(state, action, out result);
                }
            }
            return success;
        }

        public bool Process(S state, T action, out R result)
        {
            var success = Validate(state, action, out result);
            var handled = false;
            if(DerivedActionSupport)
            {
                var itr = _handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Key.IsAssignableFrom(action.GetType()))
                    {
                        if(itr.Current.Value.Handle(state, action, success, result))
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
                if(_handlers.TryGetValue(action.GetType(), out handler))
                {
                    handled = handler.Handle(state, action, success, result);
                }
            }
            return handled;
        }

        void DoRegisterHandler<K>(object key, object obj) where K : T
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

        void DoUnregisterHandler<K>(object key) where K : T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.Unregister(key);
            }
        }

        void DoRegisterValidator<K>(object key, object validator) where K : T
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

        void DoUnregisterValidator<K>(object key) where K : T
        {
            var type = typeof(K);
            ITypeValidator typeValidator;
            if(_validators.TryGetValue(type, out typeValidator))
            {
                typeValidator.Unregister(key);
            }
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

    public class ActionProcessor<K> : ActionProcessor<object, K, object>
    {
    }

    public class StateActionProcessor<S> : ActionProcessor<S, object, object>
    {
    }

    public class ActionProcessor : ActionProcessor<object>
    {
    }

}
