using System;
using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public sealed class DoubleListener<F, T> : BaseListener, IListener
    {
        event Action<F, T> _callback;
        readonly BindingKey _fromKey;
        readonly BindingKey _toKey;

        List<F> _fromInstances;
        List<T> _toInstances;

        public DoubleListener(DependencyContainer container, BindingKey fromKey, BindingKey toKey) : base(container)
        {
            _fromKey = fromKey;
            _toKey = toKey;
            _fromInstances = new List<F>();
            _toInstances = new List<T>();
        }

        public void Then(Action<F, T> callback)
        {
            _callback = callback;
        }

        void IListener.OnResolved(IBinding binding, object instance)
        {
            if(binding.Key.Equals(_fromKey))
            {
                var finstance = (F) instance;
                _fromInstances.Add(finstance);
                if(_callback != null)
                {
                    for(var i = 0; i < _toInstances.Count; i++)
                    {
                        _callback(finstance, _toInstances[i]);
                    }
                }
            }
            if(binding.Key.Equals(_toKey))
            {
                var tinstance = (T) instance;
                _toInstances.Add(tinstance);
                if(_callback != null)
                {
                    for(var i = 0; i < _fromInstances.Count; i++)
                    {
                        _callback(_fromInstances[i], tinstance);
                    }
                }
            }
            if(_fromInstances.Count > 0 && _toInstances.Count > 0)
            {
                base.Trigger();
            }
        }
    }
}