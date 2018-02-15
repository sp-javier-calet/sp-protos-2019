using System;

namespace SocialPoint.Dependency
{
    public sealed class SingleListener<T> : BaseListener, IListener
    {
        Action<T> _callback;
        BindingKey _resolve;

        public SingleListener(DependencyContainer container) : base(container)
        {
        }

        public void Then(Action<T> callback)
        {
            _callback = callback;
        }

        void IListener.OnResolved(IBinding binding, object instance)
        {
            if(_callback != null)
            {
                _callback((T)instance);
            }

            base.Trigger();
        }
    }
}
