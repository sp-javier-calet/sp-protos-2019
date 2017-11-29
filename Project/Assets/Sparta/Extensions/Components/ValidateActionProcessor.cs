using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Components
{
    public interface IValidatedActionHandler<T, R>
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
        interface ITypeValidator
        {
            void Register(object obj);
            void Unregister(object dlg);
            bool Validate(object action, out object result);
        }

        class TypeValidator<K> : ITypeValidator
        {
            List<IActionValidator<K, object>> _validators = new List<IActionValidator<K, object>>();

            public void Register(object obj)
            {
                var validatork = obj as IActionValidator<K, object>;
                if(validatork != null)
                {
                    _validators.Add(validatork);
                }
            }

            public void Unregister(object obj)
            {
                var validatork = obj as IActionValidator<K, object>;
                if(validatork != null)
                {
                    _validators.Remove(validatork);
                }
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
                for(var i = 0; i < _validators.Count; i++)
                {
                    if(!_validators[i].Validate(kaction, out result))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        interface ITypeHandler
        {
            void Register(object obj);
            void Unregister(object obj);
            bool Handle(object action, object result);
        }

        class TypeHandler<K> : ITypeHandler
        {
            List<IValidatedActionHandler<K, object>> _handlers = new List<IValidatedActionHandler<K, object>>();

            public void Register(object obj)
            {
                var handlerk = obj as IValidatedActionHandler<K, object>;
                if(handlerk != null)
                {
                    _handlers.Add(handlerk);
                }
            }

            public void Unregister(object obj)
            {
                var handlerk = obj as IValidatedActionHandler<K, object>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
            }

            public bool Handle(object action, object result)
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
                for(var i = 0; i < _handlers.Count; i++)
                {
                    _handlers[i].Handle(kaction, result);
                }
                return true;
            }
        }

        Dictionary<Type, ITypeValidator> _validators;
        Dictionary<Type, ITypeHandler> _successHandlers;
        Dictionary<Type, ITypeHandler> _failureHandlers;

        public ValidateActionProcessor()
        {
            _validators = new Dictionary<Type, ITypeValidator>();
            _successHandlers = new Dictionary<Type, ITypeHandler>();
            _failureHandlers = new Dictionary<Type, ITypeHandler>();
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
            var handlers = success ? _successHandlers : _failureHandlers;
            {
                var itr = handlers.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Value.Handle(action, result))
                    {
                        handled = true;
                    }
                }
                itr.Dispose();
            }
            return handled;
        }

        void DoRegister<K>(object obj, Dictionary<Type, ITypeHandler> handlers)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(!handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                handlers[type] = typeHandler;
            }
            typeHandler.Register(obj);
        }

        void DoUnregister<K>(object obj, Dictionary<Type, ITypeHandler> handlers)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.Unregister(obj);
            }
        }

        public void RegisterSuccess<K>(IValidatedActionHandler<K, object> handler)
        {
            DoRegister<K>(handler, _successHandlers);
        }

        public void RegisterFailure<K>(IValidatedActionHandler<K, object> handler)
        {
            DoRegister<K>(handler, _failureHandlers);
        }

        public void Unregister<K>(IValidatedActionHandler<K, object> handler)
        {
            DoUnregister<K>(handler, _successHandlers);
            DoUnregister<K>(handler, _failureHandlers);
        }

        public void Register<K>(IActionValidator<K, object> validator)
        {
            var type = typeof(K);
            ITypeValidator typeValidator;
            if(!_validators.TryGetValue(type, out typeValidator))
            {
                typeValidator = new TypeValidator<K>();
                _validators[type] = typeValidator;
            }
            typeValidator.Register(validator);
        }

        public void Unregister<K>(IActionValidator<K, object> validator)
        {
            var type = typeof(K);
            ITypeValidator typeValidator;
            if(_validators.TryGetValue(type, out typeValidator))
            {
                typeValidator.Unregister(validator);
            }
        }

        public void RegisterSuccess<K, R>(IValidatedActionHandler<K, R> handler)
        {
        }

        public void RegisterFailure<K, R>(IValidatedActionHandler<K, R> handler)
        {
        }

        public void RegisterSuccess<K>(IActionHandler<K> handler)
        {
        }

        public void RegisterFailure<K>(IActionHandler<K> handler)
        {
        }

        public void Unregister<K>(IActionHandler<K> handler)
        {
        }

        public void Register<K>(IActionValidator<K> validator)
        {
        }

        public void Unregister<K>(IActionValidator<K> validator)
        {
        }

        public void Register<K, R>(IActionValidator<K, R> validator)
        {
        }

        public void Unregister<K, R>(IActionValidator<K, R> validator)
        {
        }
    }

    public class ValidateActionProcessor : ValidateActionProcessor<object>
    {
    }

}
