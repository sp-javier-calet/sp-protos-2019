using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Components
{
    public interface IValidatedActionHandler<T, R>
    {
        void Handle(T action, bool success, R result);
    }

    public interface IResultActionHandler<T, R>
    {
        void Handle(T action, R result);
    }

    public interface IActionValidator<T>
    {
        bool Validate(T action);
    }

    public interface IActionValidator<T, R>
    {
        bool Validate(T action, out R result);
    }

    public delegate bool ActionValidatorFunc<T, R>(T action, out R result);

    public class ValidateActionProcessor<T> : IActionHandler<T>
    {
        class ActionValidatorWrapper<K, R> : IActionValidator<T, object> where K : T
        {
            IActionValidator<K, R> _validator;

            public ActionValidatorWrapper(IActionValidator<K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(T action, out object result)
            {
                result = null;
                if(_validator != null)
                {
                    if(action is K)
                    {
                        R rresult;
                        var success = _validator.Validate((K)action, out rresult);
                        result = rresult;
                        return success;
                    }
                }
                return true;
            }
        }

        class ActionValidatorWrapper<K> : IActionValidator<T, object> where K : T
        {
            IActionValidator<K> _validator;

            public ActionValidatorWrapper(IActionValidator<K> validator)
            {
                _validator = validator;
            }

            public bool Validate(T action, out object result)
            {
                result = null;
                if(_validator != null)
                {
                    if(action is K)
                    {
                        return _validator.Validate((K)action);
                    }
                }
                return true;
            }
        }

        class DelegateActionValidatorWrapper<K> : IActionValidator<T, object> where K : T
        {
            Func<K, bool> _validator;

            public DelegateActionValidatorWrapper(Func<K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(T action, out object result)
            {
                result = null;
                if(_validator != null)
                {
                    if(action is K)
                    {
                        return _validator((K)action);
                    }
                }
                return true;
            }
        }

        class DelegateActionValidatorWrapper<K, R> : IActionValidator<T, object> where K : T
        {
            ActionValidatorFunc<K, R> _validator;

            public DelegateActionValidatorWrapper(ActionValidatorFunc<K, R> validator)
            {
                _validator = validator;
            }

            public bool Validate(T action, out object result)
            {
                result = null;
                if(_validator != null)
                {
                    if(action is K)
                    {
                        R rresult;
                        var success = _validator((K)action, out rresult);
                        result = rresult;
                        return success;
                    }
                }
                return true;
            }
        }

        class ActionHandlerWrapper<K> : IValidatedActionHandler<T, object> where K : T
        {
            IActionHandler<K> _handler;
            bool _successFilter;

            public ActionHandlerWrapper(IActionHandler<K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_successFilter == success && _handler != null)
                {
                    if(action is K)
                    {
                        _handler.Handle((K)action);
                    }
                }
            }
        }

        class ValidatedActionHandlerWrapper<K, R> : IValidatedActionHandler<T, object> where K : T
        {
            IValidatedActionHandler<K, R> _handler;

            public ValidatedActionHandlerWrapper(IValidatedActionHandler<K, R> handler)
            {
                _handler = handler;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_handler != null)
                {
                    if(action is K && result is R)
                    {
                        _handler.Handle((K)action, success, (R)result);
                    }
                }
            }
        }

        class ResultActionHandlerWrapper<K, R> : IValidatedActionHandler<T, object> where K : T
        {
            IResultActionHandler<K, R> _handler;
            bool _successFilter;

            public ResultActionHandlerWrapper(IResultActionHandler<K, R> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_successFilter == success && _handler != null)
                {
                    if(action is K && result is R)
                    {
                        _handler.Handle((K)action, (R)result);
                    }
                }
            }
        }

        class DelegateActionHandlerWrapper<K> : IValidatedActionHandler<T, object> where K : T
        {
            Action<K> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_successFilter == success && _delegate != null)
                {
                    if(action is K)
                    {
                        _delegate((K)action);
                    }
                }
            }
        }

        class DelegateActionHandlerWrapper<K, R> : IValidatedActionHandler<T, object> where K : T
        {
            Action<K, R> _delegate;
            bool _successFilter;

            public DelegateActionHandlerWrapper(Action<K, R> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_successFilter == success && _delegate != null)
                {
                    if(action is K && result is R)
                    {
                        _delegate((K)action, (R)result);
                    }
                }
            }
        }

        class GeneralDelegateActionHandlerWrapper<K, R> : IValidatedActionHandler<T, object> where K : T
        {
            Action<K, bool, R> _delegate;

            public GeneralDelegateActionHandlerWrapper(Action<K, bool, R> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(T action, bool success, object result)
            {
                if(_delegate != null)
                {
                    if(action is K && result is R)
                    {
                        _delegate((K)action, success, (R)result);
                    }
                }
            }
        }

        interface ITypeValidator
        {
            void Register(object key, object obj);
            void Unregister(object key);
            bool Validate(object action, out object result);
        }

        class TypeValidator<K> : ITypeValidator
        {
            Dictionary<object, IActionValidator<K, object>> _validators = new Dictionary<object, IActionValidator<K, object>>();

            public void Register(object key, object obj)
            {
                var validatork = obj as IActionValidator<K, object>;
                if(validatork != null)
                {
                    _validators.Add(key, validatork);
                }
            }

            public void Unregister(object obj)
            {
                _validators.Remove(obj);
            }

            public bool Validate(object action, out object result)
            {
                result = null;
                if(!(action is K))
                {
                    return false;
                }
                if(_validators.Count == 0)
                {
                    return false;
                }
                var kaction = (K)action;
                var itr = _validators.GetEnumerator();
                var success = true;
                while(itr.MoveNext())
                {
                    if(!itr.Current.Value.Validate(kaction, out result))
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
            bool Handle(object action, bool success, object result);
        }

        class TypeHandler<K> : ITypeHandler
        {
            Dictionary<object, IValidatedActionHandler<K, object>> _handlers = new Dictionary<object, IValidatedActionHandler<K, object>>();

            public void Register(object key, object obj)
            {
                var handlerk = obj as IValidatedActionHandler<K, object>;
                if(handlerk != null)
                {
                    _handlers.Add(key, handlerk);
                }
            }

            public void Unregister(object key)
            {
                _handlers.Remove(key);
            }

            public bool Handle(object action, bool success, object result)
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
                    itr.Current.Value.Handle(kaction, success, result);
                }
                itr.Dispose();
                return true;
            }
        }

        Dictionary<Type, ITypeValidator> _validators;
        Dictionary<Type, ITypeHandler> _handlers;

        public ValidateActionProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _handlers = new Dictionary<Type, ITypeHandler>();
        }

        void IActionHandler<T>.Handle(T action)
        {
            Process(action);
        }

        public bool Process(T action)
        {
            object result;
            return Process(action, out result);
        }

        public bool Process(T action, out object result)
        {
            result = null;
            var success = true;
            {
                var itr = _validators.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(!itr.Current.Value.Validate(action, out result))
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
                    if(itr.Current.Value.Handle(action, success, result))
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

        public void RegisterHandler<K, R>(IValidatedActionHandler<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new ValidatedActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterHandler<K, R>(IValidatedActionHandler<K, R> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
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

        public void RegisterSuccessHandler<K, R>(Action<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, R>(handler, true));
        }

        public void RegisterFailureHandler<K, R>(Action<K, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new DelegateActionHandlerWrapper<K, R>(handler, false));
        }

        public void UnregisterHandler<K>(Action<K, T> handler) where K : T
        {
            DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, R>(Action<K, bool, R> handler) where K : T
        {
            DoRegisterHandler<K>(handler, new GeneralDelegateActionHandlerWrapper<K, R>(handler));
        }

        public void UnregisterHandler<K>(Action<K, bool, T> handler) where K : T
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
    }

    public class ValidateActionProcessor : ValidateActionProcessor<object>
    {
    }

}
