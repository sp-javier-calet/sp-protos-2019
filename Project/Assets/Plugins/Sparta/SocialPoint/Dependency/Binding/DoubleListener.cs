﻿using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Dependency
{
    public sealed class DoubleListener<F, T> : IListener
    {
        event Action<F, T> _callback;
        readonly BindingKey _fromKey;
        readonly BindingKey _toKey;

        List<F> _fromInstances;
        List<T> _toInstances;

        public DoubleListener(BindingKey fromKey, BindingKey toKey)
        {
            _fromKey = fromKey;
            _toKey = toKey;
            _fromInstances = new List<F>();
            _toInstances = new List<T>();
        }

        public DoubleListener<F, T> WhenResolved(Action<F, T> callback)
        {
            _callback = callback;
            return this;
        }

        void IListener.OnResolved(IBinding binding, object instance)
        {
            if(binding.Key.Type == _fromKey.Type && binding.Key.Tag == _fromKey.Tag)
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
            if(binding.Key.Type == _toKey.Type && binding.Key.Tag == _toKey.Tag)
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
        }
    }
}