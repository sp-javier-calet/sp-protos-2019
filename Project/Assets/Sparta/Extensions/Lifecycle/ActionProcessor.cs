using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public interface IActionHandler<T>
    {
        void Handle(T action);
    }

    public interface IValidatedActionHandler<T, R>
    {
        void Handle(T action, bool success, R result);
    }

    public interface IStateValidatedActionHandler<S, T, R>
    {
        void Handle(S state, T action, bool success, R result);
    }

    public interface IStateResultActionHandler<S, T, R>
    {
        void Handle(S state, T action, R result);
    }

    public interface IResultActionHandler<T, R>
    {
        void Handle(T action, R result);
    }

    public interface IStateActionHandler<S, T>
    {
        void Handle(S state, T action);
    }

    public interface IStateActionHandler<S> : IStateActionHandler<S, object>
    {
    }

    public interface IStateAppliable<S>
    {
        void Apply(S state);
    }

    public class AppliableStateActionHandler<S> : IStateActionHandler<S, IStateAppliable<S>>
    {
        public void Handle(S state, IStateAppliable<S> action)
        {
            if(action != null)
            {
                action.Apply(state);
            }
        }
    }

    public interface IActionValidator<T>
    {
        bool Validate(T action);
    }

    public interface IActionValidator<T, R>
    {
        bool Validate(T action, out R result);
    }

    public interface IStateActionValidator<S, T, R>
    {
        bool Validate(S state, T action, out R result);
    }

    public interface IStateActionValidator<S, T>
    {
        bool Validate(S state, T action);
    }

    public delegate bool ActionValidatorFunc<T, R>(T action, out R result);

    public delegate bool ActionValidatorFunc<S, T, R>(S state, T action, out R result);

    public class ActionProcessor<T, S, R> : IActionHandler<T>, IStateActionHandler<S, T>, IDisposable
    {
        class ActionValidatorWrapper<K, E> : IStateActionValidator<S, K, R> where K : T where E : R
        {
            IActionValidator<K, E> _validator;

            public ActionValidatorWrapper(IActionValidator<K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                E eresult;
                var success = _validator.Validate(action, out eresult);
                result = eresult;
                return success;
            }
        }

        class ActionValidatorWrapper<K> : IStateActionValidator<S, K, R> where K : T
        {
            IActionValidator<K> _validator;

            public ActionValidatorWrapper(IActionValidator<K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                result = default(R);
                return _validator.Validate(action);
            }
        }

        class DelegateActionValidatorWrapper<K> : IStateActionValidator<S, K, R> where K : T
        {
            Func<K, bool> _validator;

            public DelegateActionValidatorWrapper(Func<K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                result = default(R);
                return _validator(action);
            }
        }

        class DelegateActionValidatorWrapper<K, E> : IStateActionValidator<S, K, R> where K : T where E : R
        {
            ActionValidatorFunc<K, E> _validator;

            public DelegateActionValidatorWrapper(ActionValidatorFunc<K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                E eresult;
                var success = _validator(action, out eresult);
                result = eresult;
                return success;
            }
        }

        class ActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, R> where K : T
        {
            IActionHandler<K> _handler;
            bool _successFilter;

            public ActionHandlerWrapper(IActionHandler<K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(action);
                }
            }
        }

        class ValidatedActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            IValidatedActionHandler<K, E> _handler;

            public ValidatedActionHandlerWrapper(IValidatedActionHandler<K, E> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(result is E)
                {
                    _handler.Handle(action, success, (E)result);
                }
            }
        }

        class ResultActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            IResultActionHandler<K, E> _handler;
            bool _successFilter;

            public ResultActionHandlerWrapper(IResultActionHandler<K, E> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _handler.Handle(action, (E)result);
                }
            }
        }

        class DelegateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, R> where K : T
        {
            Action<K> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _delegate(action);
                }
            }
        }

        class DelegateActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            Action<K, E> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K, E> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _delegate(action, (E)result);
                }
            }
        }

        class GeneralDelegateActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            Action<K, bool, E> _delegate;

            public GeneralDelegateActionHandlerWrapper(Action<K, bool, E> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(result is E)
                {
                    _delegate(action, success, (E)result);
                }
            }
        }

        class StateActionValidatorWrapper<K, E> : IStateActionValidator<S, K, R> where K : T where E : R
        {
            IStateActionValidator<S, K, E> _validator;

            public StateActionValidatorWrapper(IStateActionValidator<S, K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                E eresult;
                var success = _validator.Validate(state, action, out eresult);
                result = eresult;
                return success;
            }
        }

        class StateActionValidatorWrapper<K> : IStateActionValidator<S, K, R> where K : T
        {
            IStateActionValidator<S, K> _validator;

            public StateActionValidatorWrapper(IStateActionValidator<S, K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                result = default(R);
                return _validator.Validate(state, action);
            }
        }

        class DelegateStateActionValidatorWrapper<K> : IStateActionValidator<S, K, R> where K : T
        {
            Func<S, K, bool> _validator;

            public DelegateStateActionValidatorWrapper(Func<S, K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                result = default(R);
                return _validator(state, action);
            }
        }

        class DelegateStateActionValidatorWrapper<K, E> : IStateActionValidator<S, K, R> where K : T where E : R
        {
            ActionValidatorFunc<S, K, E> _validator;

            public DelegateStateActionValidatorWrapper(ActionValidatorFunc<S, K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out R result)
            {
                E eresult;
                var success = _validator(state, action, out eresult);
                result = eresult;
                return success;
            }
        }

        class StateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, R> where K : T
        {
            IStateActionHandler<S, K> _handler;
            bool _successFilter;

            public StateActionHandlerWrapper(IStateActionHandler<S, K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(state, action);
                }
            }
        }

        class StateValidatedActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            IStateValidatedActionHandler<S, K, E> _handler;

            public StateValidatedActionHandlerWrapper(IStateValidatedActionHandler<S, K, E> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(result is E)
                {
                    _handler.Handle(state, action, success, (E)result);
                }
            }
        }

        class StateResultActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            IStateResultActionHandler<S, K, E> _handler;
            bool _successFilter;

            public StateResultActionHandlerWrapper(IStateResultActionHandler<S, K, E> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _handler.Handle(state, action, (E)result);
                }
            }
        }

        class DelegateStateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, R> where K : T
        {
            Action<S, K> _delegate;
            bool _successFilter;

            public DelegateStateActionHandlerWrapper(Action<S, K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _delegate(state, action);
                }
            }
        }

        class DelegateStateActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            Action<S, K, E> _delegate;
            bool _successFilter;

            public DelegateStateActionHandlerWrapper(Action<S, K, E> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _delegate(state, action, (E)result);
                }
            }
        }

        class GeneralDelegateStateActionHandlerWrapper<K, E> : IStateValidatedActionHandler<S, K, R> where K : T where E : R
        {
            Action<S, K, bool, E> _delegate;

            public GeneralDelegateStateActionHandlerWrapper(Action<S, K, bool, E> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K action, bool success, R result)
            {
                if(result is E)
                {
                    _delegate(state, action, success, (E)result);
                }
            }
        }

        interface ITypeValidator
        {
            void Register(object key, object obj);
            void Unregister(object key);
            bool Validate(S state, object action, out R result);
        }

        class TypeValidator<K> : ITypeValidator
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

            public bool Validate(S state, object action, out R result)
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
            bool Handle(S state, object action, bool success, R result);
        }

        class TypeHandler<K> : ITypeHandler
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

            public bool Handle(S state, object action, bool success, R result)
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

        void DoRegisterHandler<K>(object key, object obj)
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

        void DoUnregisterHandler<K>(object key)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.Unregister(key);
            }
        }

        void DoRegisterValidator<K>(object key, object validator)
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

        public void DoUnregisterValidator<K>(object key)
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

        public void RegisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ValidatedActionHandlerWrapper<K, E>(handler));
            }
        }

        public void UnregisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoUnregisterHandler<K>(handler);
            }
        }

        public void RegisterHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ResultActionHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ResultActionHandlerWrapper<K, E>(handler, false));
            }
        }

        public void UnregisterHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IActionHandler<K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IActionHandler<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ActionHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(IActionHandler<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ActionHandlerWrapper<K>(handler, false));
            }
        }

        public void UnregisterHandler<K>(IActionHandler<K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(Action<K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(Action<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(Action<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K>(handler, false));
            }
        }

        public void UnregisterHandler<K>(Action<K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            RegisterSuccessResultHandler<K, E>(handler);
        }

        public void RegisterSuccessResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, E>(handler, false));
            }
        }

        public void UnregisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateActionHandlerWrapper<K, E>(handler));
            }
        }

        public void UnregisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K>(validator));
            }
        }

        public void UnregisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K, E>(validator));
            }
        }

        public void UnregisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K>(validator));
            }
        }

        public void UnregisterValidator<K>(Func<K, bool> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K, E>(validator));
            }
        }

        public void UnregisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateValidatedActionHandlerWrapper<K, E>(handler));
            }
        }

        public void UnregisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateResultActionHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateResultActionHandlerWrapper<K, E>(handler, false));
            }
        }

        public void UnregisterHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateActionHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateActionHandlerWrapper<K>(handler, false));
            }
        }

        public void UnregisterHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterStateHandler<K>(Action<S, K> handler) where K : T
        {
            RegisterSuccessStateHandler<K>(handler);
        }

        public void RegisterSuccessStateHandler<K>(Action<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureStateHandler<K>(Action<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K>(handler, false));
            }
        }

        public void UnregisterStateHandler<K>(Action<S, K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K, E>(handler, false));
            }
        }

        public void UnregisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateStateActionHandlerWrapper<K, E>(handler));
            }
        }

        public void UnregisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K>(validator));
            }
        }

        public void UnregisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K, E>(validator));
            }
        }

        public void UnregisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K>(validator));
            }
        }

        public void UnregisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K, E>(validator));
            }
        }

        public void UnregisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            DoUnregisterValidator<K>(validator);
        }

    }

    public class ActionProcessor<K> : ActionProcessor<K, object, object>
    {
    }

    public class StateActionProcessor<S> : ActionProcessor<object, S, object>
    {
    }

    public class ActionProcessor : ActionProcessor<object>
    {
    }

}
