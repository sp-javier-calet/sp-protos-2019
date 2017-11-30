using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Components
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

    public class ActionProcessor<T, S> : IActionHandler<T>, IStateActionHandler<S, T>
    {
        class ActionValidatorWrapper<K, R> : IStateActionValidator<S, K, object> where K : T
        {
            IActionValidator<K, R> _validator;

            public ActionValidatorWrapper(IActionValidator<K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                R rresult;
                var success = _validator.Validate(action, out rresult);
                result = rresult;
                return success;
            }
        }

        class ActionValidatorWrapper<K> : IStateActionValidator<S, K, object> where K : T
        {
            IActionValidator<K> _validator;

            public ActionValidatorWrapper(IActionValidator<K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                result = null;
                return _validator.Validate(action);
            }
        }

        class DelegateActionValidatorWrapper<K> : IStateActionValidator<S, K, object> where K : T
        {
            Func<K, bool> _validator;

            public DelegateActionValidatorWrapper(Func<K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                result = null;
                return _validator(action);
            }
        }

        class DelegateActionValidatorWrapper<K, R> : IStateActionValidator<S, K, object> where K : T
        {
            ActionValidatorFunc<K, R> _validator;

            public DelegateActionValidatorWrapper(ActionValidatorFunc<K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                R rresult;
                var success = _validator(action, out rresult);
                result = rresult;
                return success;
            }
        }

        class ActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IActionHandler<K> _handler;
            bool _successFilter;

            public ActionHandlerWrapper(IActionHandler<K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(action);
                }
            }
        }

        class ValidatedActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IValidatedActionHandler<K, R> _handler;

            public ValidatedActionHandlerWrapper(IValidatedActionHandler<K, R> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(result is R)
                {
                    _handler.Handle(action, success, (R)result);
                }
            }
        }

        class ResultActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IResultActionHandler<K, R> _handler;
            bool _successFilter;

            public ResultActionHandlerWrapper(IResultActionHandler<K, R> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success && result is R)
                {
                    _handler.Handle(action, (R)result);
                }
            }
        }

        class DelegateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<K> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success)
                {
                    _delegate(action);
                }
            }
        }

        class DelegateActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<K, R> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K, R> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success && result is R)
                {
                    _delegate(action, (R)result);
                }
            }
        }

        class GeneralDelegateActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<K, bool, R> _delegate;

            public GeneralDelegateActionHandlerWrapper(Action<K, bool, R> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(result is R)
                {
                    _delegate(action, success, (R)result);
                }
            }
        }

        class StateActionValidatorWrapper<K, R> : IStateActionValidator<S, K, object> where K : T
        {
            IStateActionValidator<S, K, R> _validator;

            public StateActionValidatorWrapper(IStateActionValidator<S, K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                R rresult;
                var success = _validator.Validate(state, action, out rresult);
                result = rresult;
                return success;
            }
        }

        class StateActionValidatorWrapper<K> : IStateActionValidator<S, K, object> where K : T
        {
            IStateActionValidator<S, K> _validator;

            public StateActionValidatorWrapper(IStateActionValidator<S, K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                result = null;
                return _validator.Validate(state, action);
            }
        }

        class DelegateStateActionValidatorWrapper<K> : IStateActionValidator<S, K, object> where K : T
        {
            Func<S, K, bool> _validator;

            public DelegateStateActionValidatorWrapper(Func<S, K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                result = null;
                return _validator(state, action);
            }
        }

        class DelegateStateActionValidatorWrapper<K, R> : IStateActionValidator<S, K, object> where K : T
        {
            ActionValidatorFunc<S, K, R> _validator;

            public DelegateStateActionValidatorWrapper(ActionValidatorFunc<S, K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K action, out object result)
            {
                R rresult;
                var success = _validator(state, action, out rresult);
                result = rresult;
                return success;
            }
        }

        class StateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IStateActionHandler<S, K> _handler;
            bool _successFilter;

            public StateActionHandlerWrapper(IStateActionHandler<S, K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(state, action);
                }
            }
        }

        class StateValidatedActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IStateValidatedActionHandler<S, K, R> _handler;

            public StateValidatedActionHandlerWrapper(IStateValidatedActionHandler<S, K, R> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(result is R)
                {
                    _handler.Handle(state, action, success, (R)result);
                }
            }
        }

        class StateResultActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            IStateResultActionHandler<S, K, R> _handler;
            bool _successFilter;

            public StateResultActionHandlerWrapper(IStateResultActionHandler<S, K, R> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success && result is R)
                {
                    _handler.Handle(state, action, (R)result);
                }
            }
        }

        class DelegateStateActionHandlerWrapper<K> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<S, K> _delegate;
            bool _successFilter;

            public DelegateStateActionHandlerWrapper(Action<S, K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success)
                {
                    _delegate(state, action);
                }
            }
        }

        class DelegateStateActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<S, K, R> _delegate;
            bool _successFilter;

            public DelegateStateActionHandlerWrapper(Action<S, K, R> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(_successFilter == success && result is R)
                {
                    _delegate(state, action, (R)result);
                }
            }
        }

        class GeneralDelegateStateActionHandlerWrapper<K, R> : IStateValidatedActionHandler<S, K, object> where K : T
        {
            Action<S, K, bool, R> _delegate;

            public GeneralDelegateStateActionHandlerWrapper(Action<S, K, bool, R> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K action, bool success, object result)
            {
                if(result is R)
                {
                    _delegate(state, action, success, (R)result);
                }
            }
        }

        interface ITypeValidator
        {
            void Register(object key, object obj);
            void Unregister(object key);
            bool Validate(S state, object action, out object result);
        }

        class TypeValidator<K> : ITypeValidator
        {
            Dictionary<object, IStateActionValidator<S, K, object>> _validators = new Dictionary<object, IStateActionValidator<S, K, object>>();

            public void Register(object key, object obj)
            {
                var validatork = obj as IStateActionValidator<S, K, object>;
                if(validatork != null)
                {
                    _validators.Add(key, validatork);
                }
            }

            public void Unregister(object obj)
            {
                _validators.Remove(obj);
            }

            public bool Validate(S state, object action, out object result)
            {
                result = null;
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
            bool Handle(S state, object action, bool success, object result);
        }

        class TypeHandler<K> : ITypeHandler
        {
            Dictionary<object, IStateValidatedActionHandler<S, K, object>> _handlers = new Dictionary<object, IStateValidatedActionHandler<S, K, object>>();

            public void Register(object key, object obj)
            {
                var handlerk = obj as IStateValidatedActionHandler<S, K, object>;
                if(handlerk != null)
                {
                    _handlers.Add(key, handlerk);
                }
            }

            public void Unregister(object key)
            {
                _handlers.Remove(key);
            }

            public bool Handle(S state, object action, bool success, object result)
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

        public ActionProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _handlers = new Dictionary<Type, ITypeHandler>();
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
            object result;
            return Process(action, out result);
        }

        public bool Process(S state, T action)
        {
            object result;
            return Process(state, action, out result);
        }

        public bool Process(T action, out object result)
        {
            var state = default(S);
            return Process(state, action, out result);
        }

        public bool Process(S state, T action, out object result)
        {
            result = null;
            var success = true;
            {
                var itr = _validators.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(!itr.Current.Value.Validate(state, action, out result))
                    {
                        success = false;
                        break;
                    }
                }
                itr.Dispose();
            }
            var handled = false;
            {
                var itr = _handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Value.Handle(state, action, success, result))
                    {
                        handled = true;
                    }
                }
                itr.Dispose();
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

        public void RegisterHandler<K, R>(IValidatedActionHandler<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ValidatedActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterHandler<K, R>(IValidatedActionHandler<K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, R>(IResultActionHandler<K, R> handler) where K : T
        {
            RegisterSuccessHandler<K, R>(handler);
        }

        public void RegisterSuccessHandler<K, R>(IResultActionHandler<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ResultActionHandlerWrapper<K, R>(handler, true));
        }

        public void RegisterFailureHandler<K, R>(IResultActionHandler<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ResultActionHandlerWrapper<K, R>(handler, false));
        }

        public void UnregisterHandler<K, R>(IResultActionHandler<K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IActionHandler<K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IActionHandler<K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ActionHandlerWrapper<K>(handler, true));
        }

        public void RegisterFailureHandler<K>(IActionHandler<K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ActionHandlerWrapper<K>(handler, false));
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
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K>(handler, true));
        }

        public void RegisterFailureHandler<K>(Action<K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K>(handler, false));
        }

        public void UnregisterHandler<K>(Action<K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, R>(Action<K, R> handler) where K : T
        {
            RegisterSuccessResultHandler<K, R>(handler);
        }

        public void RegisterSuccessResultHandler<K, R>(Action<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, R>(handler, true));
        }

        public void RegisterFailureResultHandler<K, R>(Action<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, R>(handler, false));
        }

        public void UnregisterResultHandler<K, R>(Action<K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, R>(Action<K, bool, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new GeneralDelegateActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterResultHandler<K, R>(Action<K, bool, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K>(validator));
        }

        public void UnregisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, R>(IActionValidator<K, R> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K, R>(validator));
        }

        public void UnregisterValidator<K, R>(IActionValidator<K, R> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<K, bool> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K>(validator));
        }

        public void UnregisterValidator<K>(Func<K, bool> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, R>(ActionValidatorFunc<K, R> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K, R>(validator));
        }

        public void UnregisterValidator<K, R>(ActionValidatorFunc<K, R> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterHandler<K, R>(IStateValidatedActionHandler<S, K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new StateValidatedActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterHandler<K, R>(IStateValidatedActionHandler<S, K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, R>(IStateResultActionHandler<S, K, R> handler) where K : T
        {
            RegisterSuccessHandler<K, R>(handler);
        }

        public void RegisterSuccessHandler<K, R>(IStateResultActionHandler<S, K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new StateResultActionHandlerWrapper<K, R>(handler, true));
        }

        public void RegisterFailureHandler<K, R>(IStateResultActionHandler<S, K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new StateResultActionHandlerWrapper<K, R>(handler, false));
        }

        public void UnregisterHandler<K, R>(IStateResultActionHandler<S, K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new StateActionHandlerWrapper<K>(handler, true));
        }

        public void RegisterFailureHandler<K>(IStateActionHandler<S, K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new StateActionHandlerWrapper<K>(handler, false));
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
            DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K>(handler, true));
        }

        public void RegisterFailureStateHandler<K>(Action<S, K> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K>(handler, false));
        }

        public void UnregisterStateHandler<K>(Action<S, K> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, R>(Action<S, K, R> handler) where K : T
        {
            RegisterSuccessHandler<K, R>(handler);
        }

        public void RegisterSuccessHandler<K, R>(Action<S, K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K, R>(handler, true));
        }

        public void RegisterFailureHandler<K, R>(Action<S, K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateStateActionHandlerWrapper<K, R>(handler, false));
        }

        public void UnregisterHandler<K, R>(Action<S, K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, R>(Action<S, K, bool, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new GeneralDelegateStateActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterHandler<K, R>(Action<S, K, bool, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K>(validator));
        }

        public void UnregisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, R>(IStateActionValidator<S, K, R> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K, R>(validator));
        }

        public void UnregisterValidator<K, R>(IStateActionValidator<S, K, R> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K>(validator));
        }

        public void UnregisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, R>(ActionValidatorFunc<S, K, R> validator) where K : T
        {
            DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K, R>(validator));
        }

        public void UnregisterValidator<K, R>(ActionValidatorFunc<S, K, R> validator) where K : T
        {
            DoUnregisterValidator<K>(validator);
        }

    }

    public class ActionProcessor<K> : ActionProcessor<K, object>
    {
    }

    public class StateActionProcessor<S> : ActionProcessor<object, S>
    {
    }

    public class ActionProcessor : ActionProcessor<object>
    {
    }

}
