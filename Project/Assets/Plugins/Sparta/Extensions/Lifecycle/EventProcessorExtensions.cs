using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public partial class EventProcessor<S, T, R>
    {
        class ValidatorWrapper<K, E> : IStateEventValidator<S, K, R> where K : T where E : R
        {
            IEventValidator<K, E> _validator;

            public ValidatorWrapper(IEventValidator<K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                E eresult;
                var success = _validator.Validate(ev, out eresult);
                result = eresult;
                return success;
            }
        }

        class ValidatorWrapper<K> : IStateEventValidator<S, K, R> where K : T
        {
            IEventValidator<K> _validator;

            public ValidatorWrapper(IEventValidator<K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                result = default(R);
                return _validator.Validate(ev);
            }
        }

        class DelegateValidatorWrapper<K> : IStateEventValidator<S, K, R> where K : T
        {
            Func<K, bool> _validator;

            public DelegateValidatorWrapper(Func<K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                result = default(R);
                return _validator(ev);
            }
        }

        class DelegateValidatorWrapper<K, E> : IStateEventValidator<S, K, R> where K : T where E : R
        {
            EventValidatorFunc<K, E> _validator;

            public DelegateValidatorWrapper(EventValidatorFunc<K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                E eresult;
                var success = _validator(ev, out eresult);
                result = eresult;
                return success;
            }
        }

        class HandlerWrapper<K> : IStateValidatedEventHandler<S, K, R> where K : T
        {
            IEventHandler<K> _handler;
            bool _successFilter;

            public HandlerWrapper(IEventHandler<K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(ev);
                }
            }
        }

        class ValidatedHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            IValidatedEventHandler<K, E> _handler;

            public ValidatedHandlerWrapper(IValidatedEventHandler<K, E> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(result is E)
                {
                    _handler.Handle(ev, success, (E)result);
                }
            }
        }

        class ResultHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            IResultEventHandler<K, E> _handler;
            bool _successFilter;

            public ResultHandlerWrapper(IResultEventHandler<K, E> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _handler.Handle(ev, (E)result);
                }
            }
        }

        class DelegateHandlerWrapper<K> : IStateValidatedEventHandler<S, K, R> where K : T
        {
            Action<K> _delegate;
            bool _successFilter;

            public DelegateHandlerWrapper(Action<K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _delegate(ev);
                }
            }
        }

        class DelegateHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            Action<K, E> _delegate;
            bool _successFilter;

            public DelegateHandlerWrapper(Action<K, E> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _delegate(ev, (E)result);
                }
            }
        }

        class GeneralDelegateHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            Action<K, bool, E> _delegate;

            public GeneralDelegateHandlerWrapper(Action<K, bool, E> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(result is E)
                {
                    _delegate(ev, success, (E)result);
                }
            }
        }

        class StateValidatorWrapper<K, E> : IStateEventValidator<S, K, R> where K : T where E : R
        {
            IStateEventValidator<S, K, E> _validator;

            public StateValidatorWrapper(IStateEventValidator<S, K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                E eresult;
                var success = _validator.Validate(state, ev, out eresult);
                result = eresult;
                return success;
            }
        }

        class StateValidatorWrapper<K> : IStateEventValidator<S, K, R> where K : T
        {
            IStateEventValidator<S, K> _validator;

            public StateValidatorWrapper(IStateEventValidator<S, K> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                result = default(R);
                return _validator.Validate(state, ev);
            }
        }

        class DelegateStateValidatorWrapper<K> : IStateEventValidator<S, K, R> where K : T
        {
            Func<S, K, bool> _validator;

            public DelegateStateValidatorWrapper(Func<S, K, bool> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                result = default(R);
                return _validator(state, ev);
            }
        }

        class DelegateStateValidatorWrapper<K, E> : IStateEventValidator<S, K, R> where K : T where E : R
        {
            EventValidatorFunc<S, K, E> _validator;

            public DelegateStateValidatorWrapper(EventValidatorFunc<S, K, E> validator)
            {
                _validator = validator;
            }

            public bool Validate(S state, K ev, out R result)
            {
                E eresult;
                var success = _validator(state, ev, out eresult);
                result = eresult;
                return success;
            }
        }

        class StateHandlerWrapper<K> : IStateValidatedEventHandler<S, K, R> where K : T
        {
            IStateEventHandler<S, K> _handler;
            bool _successFilter;

            public StateHandlerWrapper(IStateEventHandler<S, K> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _handler.Handle(state, ev);
                }
            }
        }

        class StateValidatedHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            IStateValidatedEventHandler<S, K, E> _handler;

            public StateValidatedHandlerWrapper(IStateValidatedEventHandler<S, K, E> handler)
            {
                _handler = handler;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(result is E)
                {
                    _handler.Handle(state, ev, success, (E)result);
                }
            }
        }

        class StateResultHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            IStateResultEventHandler<S, K, E> _handler;
            bool _successFilter;

            public StateResultHandlerWrapper(IStateResultEventHandler<S, K, E> handler, bool successFilter)
            {
                _handler = handler;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _handler.Handle(state, ev, (E)result);
                }
            }
        }

        class DelegateStateHandlerWrapper<K> : IStateValidatedEventHandler<S, K, R> where K : T
        {
            Action<S, K> _delegate;
            bool _successFilter;

            public DelegateStateHandlerWrapper(Action<S, K> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success)
                {
                    _delegate(state, ev);
                }
            }
        }

        class DelegateStateHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            Action<S, K, E> _delegate;
            bool _successFilter;

            public DelegateStateHandlerWrapper(Action<S, K, E> dlg, bool successFilter)
            {
                _delegate = dlg;
                _successFilter = successFilter;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(_successFilter == success && result is E)
                {
                    _delegate(state, ev, (E)result);
                }
            }
        }

        class GeneralDelegateStateHandlerWrapper<K, E> : IStateValidatedEventHandler<S, K, R> where K : T where E : R
        {
            Action<S, K, bool, E> _delegate;

            public GeneralDelegateStateHandlerWrapper(Action<S, K, bool, E> dlg)
            {
                _delegate = dlg;
            }

            public void Handle(S state, K ev, bool success, R result)
            {
                if(result is E)
                {
                    _delegate(state, ev, success, (E)result);
                }
            }
        }

        public void RegisterHandler<K, E>(IValidatedEventHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ValidatedHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(IValidatedEventHandler<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ResultHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ResultHandlerWrapper<K, E>(handler, false));
            }
        }

        public bool UnregisterHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IEventHandler<K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IEventHandler<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new HandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(IEventHandler<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new HandlerWrapper<K>(handler, false));
            }
        }

        public bool UnregisterHandler<K>(IEventHandler<K> handler) where K : T
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(Action<K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(Action<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(Action<K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateHandlerWrapper<K>(handler, false));
            }
        }

        public bool UnregisterHandler<K>(Action<K> handler) where K : T
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            RegisterSuccessResultHandler<K, E>(handler);
        }

        public void RegisterSuccessResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateHandlerWrapper<K, E>(handler, false));
            }
        }

        public bool UnregisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IEventValidator<K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(IEventValidator<K> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IEventValidator<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(IEventValidator<K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(Func<K, bool> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(EventValidatorFunc<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(EventValidatorFunc<K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterHandler<K, E>(IStateValidatedEventHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateValidatedHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(IStateValidatedEventHandler<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateResultHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateResultHandlerWrapper<K, E>(handler, false));
            }
        }

        public bool UnregisterHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K>(IStateEventHandler<S, K> handler) where K : T
        {
            RegisterSuccessHandler<K>(handler);
        }

        public void RegisterSuccessHandler<K>(IStateEventHandler<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureHandler<K>(IStateEventHandler<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateHandlerWrapper<K>(handler, false));
            }
        }

        public bool UnregisterHandler<K>(IStateEventHandler<S, K> handler) where K : T
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterStateHandler<K>(Action<S, K> handler) where K : T
        {
            RegisterSuccessStateHandler<K>(handler);
        }

        public void RegisterSuccessStateHandler<K>(Action<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateHandlerWrapper<K>(handler, true));
            }
        }

        public void RegisterFailureStateHandler<K>(Action<S, K> handler) where K : T
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateHandlerWrapper<K>(handler, false));
            }
        }

        public bool UnregisterStateHandler<K>(Action<S, K> handler) where K : T
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            RegisterSuccessHandler<K, E>(handler);
        }

        public void RegisterSuccessHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateHandlerWrapper<K, E>(handler, true));
            }
        }

        public void RegisterFailureHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new DelegateStateHandlerWrapper<K, E>(handler, false));
            }
        }

        public bool UnregisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateStateHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IStateEventValidator<S, K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(IStateEventValidator<S, K> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IStateEventValidator<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(IStateEventValidator<S, K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(EventValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(EventValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }
    }
}
