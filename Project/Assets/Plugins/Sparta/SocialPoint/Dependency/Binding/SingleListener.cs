using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Dependency
{
    public sealed class SingleListener<T> : IListener
    {
        event Action<T> _callback;

        public SingleListener<T> WhenResolved(Action<T> callback)
        {
            _callback = callback;
            return this;
        }

        void IListener.OnResolved(IBinding binding, object instance)
        {
            if(_callback != null)
            {
                _callback((T)instance);
            }
        }
    }
}
