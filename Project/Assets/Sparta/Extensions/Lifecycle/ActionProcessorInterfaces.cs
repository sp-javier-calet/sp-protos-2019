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

    public interface IActionProcessor<S, T, R>
    {
        bool Process(S state, T action, out R result);

        void Unregister<K>() where K : T;
        void UnregisterHandlers<K>() where K : T;
        void UnregisterValidators<K>() where K : T;

        void RegisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(IValidatedActionHandler<K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(IResultActionHandler<K, E> handler) where K : T where E : R;
        void RegisterHandler<K>(IActionHandler<K> handler) where K : T;
        void RegisterSuccessHandler<K>(IActionHandler<K> handler) where K : T;
        void RegisterFailureHandler<K>(IActionHandler<K> handler) where K : T;
        void UnregisterHandler<K>(IActionHandler<K> handler) where K : T;
        void RegisterHandler<K>(Action<K> handler) where K : T;
        void RegisterSuccessHandler<K>(Action<K> handler) where K : T;
        void RegisterFailureHandler<K>(Action<K> handler) where K : T;
        void UnregisterHandler<K>(Action<K> handler) where K : T;
        void RegisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterSuccessResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterFailureResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void UnregisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R;
        void UnregisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R;
        void RegisterValidator<K>(IActionValidator<K> validator) where K : T;
        void UnregisterValidator<K>(IActionValidator<K> validator) where K : T;
        void RegisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R;
        void UnregisterValidator<K, E>(IActionValidator<K, E> validator) where K : T where E : R;
        void RegisterValidator<K>(Func<K, bool> validator) where K : T;
        void UnregisterValidator<K>(Func<K, bool> validator) where K : T;
        void RegisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R;
        void UnregisterValidator<K, E>(ActionValidatorFunc<K, E> validator) where K : T where E : R;
        void RegisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(IStateValidatedActionHandler<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(IStateResultActionHandler<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K>(IStateActionHandler<S, K> handler) where K : T;
        void RegisterSuccessHandler<K>(IStateActionHandler<S, K> handler) where K : T;
        void RegisterFailureHandler<K>(IStateActionHandler<S, K> handler) where K : T;
        void UnregisterHandler<K>(IStateActionHandler<S, K> handler) where K : T;
        void RegisterStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterSuccessStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterFailureStateHandler<K>(Action<S, K> handler) where K : T;
        void UnregisterStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R;
        void UnregisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R;
        void RegisterValidator<K>(IStateActionValidator<S, K> validator) where K : T;
        void UnregisterValidator<K>(IStateActionValidator<S, K> validator) where K : T;
        void RegisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R;
        void UnregisterValidator<K, E>(IStateActionValidator<S, K, E> validator) where K : T where E : R;
        void RegisterValidator<K>(Func<S, K, bool> validator) where K : T;
        void UnregisterValidator<K>(Func<S, K, bool> validator) where K : T;
        void RegisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R;
        void UnregisterValidator<K, E>(ActionValidatorFunc<S, K, E> validator) where K : T where E : R;
    }

    public interface IActionProcessor<K> : IActionProcessor<object, K, object>
    {
    }

    public interface IStateActionProcessor<S> : IActionProcessor<S, object, object>
    {
    }

    public interface IActionProcessor : IActionProcessor<object>
    {
    }
}
