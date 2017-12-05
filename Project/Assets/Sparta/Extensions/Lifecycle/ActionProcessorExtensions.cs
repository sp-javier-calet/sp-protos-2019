using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public partial class ActionProcessor<S, T, R>
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

        public void RegisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new ValidatedActionHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
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

        public bool UnregisterHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
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

        public bool UnregisterHandler<K>(IActionHandler<K> handler) where K : T
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

        public bool UnregisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateActionHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(IActionValidator<K> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new ActionValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(Func<K, bool> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateActionValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new StateValidatedActionHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
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

        public bool UnregisterHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
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

        public bool UnregisterHandler<K>(IStateActionHandler<S, K> handler) where K : T
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

        public bool UnregisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            if(handler != null)
            {
                DoRegisterHandler<K>(handler, new GeneralDelegateStateActionHandlerWrapper<K, E>(handler));
            }
        }

        public bool UnregisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R
        {
            return DoUnregisterHandler<K>(handler);
        }

        public void RegisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(IStateActionValidator<S, K> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new StateActionValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K>(validator));
            }
        }

        public bool UnregisterValidator<K>(Func<S, K, bool> validator) where K : T
        {
            return DoUnregisterValidator<K>(validator);
        }

        public void RegisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            if(validator != null)
            {
                DoRegisterValidator<K>(validator, new DelegateStateActionValidatorWrapper<K, E>(validator));
            }
        }

        public bool UnregisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R
        {
            return DoUnregisterValidator<K>(validator);
        }
    }
}
