using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    public interface IEventHandler<T>
    {
        void Handle(T ev);
    }

    public interface IValidatedEventHandler<T, R>
    {
        void Handle(T ev, bool success, R result);
    }

    public interface IStateValidatedEventHandler<S, T, R>
    {
        void Handle(S state, T ev, bool success, R result);
    }

    public interface IStateValidatedEventHandler<T> : IStateValidatedEventHandler<object, T, object>
    {
    }

    public interface IStateResultEventHandler<S, T, R>
    {
        void Handle(S state, T ev, R result);
    }

    public interface IResultEventHandler<T, R>
    {
        void Handle(T ev, R result);
    }

    public interface IStateEventHandler<S, T>
    {
        void Handle(S state, T ev);
    }

    public interface IStateEventHandler<S> : IStateEventHandler<S, object>
    {
    }

    public interface IEventValidator<T>
    {
        bool Validate(T ev);
    }

    public interface IEventValidator<T, R>
    {
        bool Validate(T ev, out R result);
    }

    public interface IStateEventValidator<S, T, R>
    {
        bool Validate(S state, T ev, out R result);
    }

    public interface IStateEventValidator<S, T>
    {
        bool Validate(S state, T ev);
    }

    public delegate bool EventValidatorFunc<T, R>(T ev, out R result);

    public delegate bool EventValidatorFunc<S, T, R>(S state, T ev, out R result);

    public interface IEventProcessor<S, T, R>
    {
        bool Process(T action);
        bool Process(S state, T action);
        bool Process(S state, T action, out R result);

        void Unregister<K>() where K : T;
        void UnregisterHandlers<K>() where K : T;
        void UnregisterValidators<K>() where K : T;

        void DoRegisterHandler<K>(object key, IStateValidatedEventHandler<S, K, R> obj) where K : T;
        bool DoUnregisterHandler<K>(object key) where K : T;
        void DoRegisterValidator<K>(object key, IStateEventValidator<S, K, R> validator) where K : T;
        bool DoUnregisterValidator<K>(object key) where K : T;

        // extension methods

        void RegisterHandler<K, E>(IValidatedEventHandler<K, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(IValidatedEventHandler<K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(IResultEventHandler<K, E> handler) where K : T where E : R;
        void RegisterHandler<K>(IEventHandler<K> handler) where K : T;
        void RegisterSuccessHandler<K>(IEventHandler<K> handler) where K : T;
        void RegisterFailureHandler<K>(IEventHandler<K> handler) where K : T;
        bool UnregisterHandler<K>(IEventHandler<K> handler) where K : T;
        void RegisterHandler<K>(Action<K> handler) where K : T;
        void RegisterSuccessHandler<K>(Action<K> handler) where K : T;
        void RegisterFailureHandler<K>(Action<K> handler) where K : T;
        bool UnregisterHandler<K>(Action<K> handler) where K : T;
        void RegisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterSuccessResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterFailureResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        bool UnregisterResultHandler<K, E>(Action<K, E> handler) where K : T where E : R;
        void RegisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R;
        bool UnregisterResultHandler<K, E>(Action<K, bool, E> handler) where K : T where E : R;
        void RegisterValidator<K>(IEventValidator<K> validator) where K : T;
        bool UnregisterValidator<K>(IEventValidator<K> validator) where K : T;
        void RegisterValidator<K, E>(IEventValidator<K, E> validator) where K : T where E : R;
        bool UnregisterValidator<K, E>(IEventValidator<K, E> validator) where K : T where E : R;
        void RegisterValidator<K>(Func<K, bool> validator) where K : T;
        bool UnregisterValidator<K>(Func<K, bool> validator) where K : T;
        void RegisterValidator<K, E>(EventValidatorFunc<K, E> validator) where K : T where E : R;
        bool UnregisterValidator<K, E>(EventValidatorFunc<K, E> validator) where K : T where E : R;
        void RegisterHandler<K, E>(IStateValidatedEventHandler<S, K, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(IStateValidatedEventHandler<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(IStateResultEventHandler<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K>(IStateEventHandler<S, K> handler) where K : T;
        void RegisterSuccessHandler<K>(IStateEventHandler<S, K> handler) where K : T;
        void RegisterFailureHandler<K>(IStateEventHandler<S, K> handler) where K : T;
        bool UnregisterHandler<K>(IStateEventHandler<S, K> handler) where K : T;
        void RegisterStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterSuccessStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterFailureStateHandler<K>(Action<S, K> handler) where K : T;
        bool UnregisterStateHandler<K>(Action<S, K> handler) where K : T;
        void RegisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterSuccessHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterFailureHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(Action<S, K, E> handler) where K : T where E : R;
        void RegisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R;
        bool UnregisterHandler<K, E>(Action<S, K, bool, E> handler) where K : T where E : R;
        void RegisterValidator<K>(IStateEventValidator<S, K> validator) where K : T;
        bool UnregisterValidator<K>(IStateEventValidator<S, K> validator) where K : T;
        void RegisterValidator<K, E>(IStateEventValidator<S, K, E> validator) where K : T where E : R;
        bool UnregisterValidator<K, E>(IStateEventValidator<S, K, E> validator) where K : T where E : R;
        void RegisterValidator<K>(Func<S, K, bool> validator) where K : T;
        bool UnregisterValidator<K>(Func<S, K, bool> validator) where K : T;
        void RegisterValidator<K, E>(EventValidatorFunc<S, K, E> validator) where K : T where E : R;
        bool UnregisterValidator<K, E>(EventValidatorFunc<S, K, E> validator) where K : T where E : R;
    }

    public interface IEventProcessor<K> : IEventProcessor<object, K, object>
    {
    }

    public interface IStateEventProcessor<S> : IEventProcessor<S, object, object>
    {
    }

    public interface IEventProcessor : IEventProcessor<object>
    {
    }
}
