using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Dependency
{
    public sealed class SingleListener<T> : BaseListener
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

        protected override void OnResolved(IBinding binding, object instance)
        {
            base.OnResolved(binding, instance);

            if(_callback != null)
            {
                _callback((T)instance);
            }
        }
    }
}
