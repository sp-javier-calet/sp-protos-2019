using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Dependency.Graph;

namespace SocialPoint.Dependency
{
    public sealed class Binding<F> : IBinding
    {
        enum ToType
        {
            Single,
            Lookup,
            Method
        }

        F _instance;
        bool _validInstance;
        ToType _toType;
        Type _type;
        string _tag;
        Func<F> _method;
        Action<object> _setup;
        Func<object, F> _getter;
        DependencyContainer _container;

        public BindingKey Key
        {
            get
            {
                return new BindingKey(typeof(F), _tag);
            }
        }

        public bool Resolved { get; private set; }

        public Binding(DependencyContainer container)
        {
            _container = container;
        }

        public Binding<F> ToSingle<T>() where T : F, new()
        {
            DependencyGraphBuilder.Bind(typeof(F), typeof(T), _tag);
            _toType = ToType.Single;
            _type = typeof(T);
            return this;
        }

        public Binding<F> ToInstance<T>(T instance) where T : F
        {
            DependencyGraphBuilder.Bind(typeof(F), typeof(T), _tag);
            _toType = ToType.Single;
            _instance = instance;
            _validInstance = true;
            return this;
        }

        public Binding<F> ToLookup<T>(string tag = null) where T : F
        {
            DependencyGraphBuilder.Alias(typeof(F), _tag, typeof(T), tag);
            _toType = ToType.Lookup;
            _type = typeof(T);
            _tag = tag;
            _container.AddLookup(this, _type, _tag);
            return this;
        }

        public Binding<F> ToMethod<T>(Func<T> method, Action<T> setup = null) where T : F
        {
            DependencyGraphBuilder.Bind(typeof(F), typeof(T), _tag);
            _type = typeof(T);
            _method = () => method();
            _toType = ToType.Method;

            _setup = null;
            if(setup != null)
            {
                DebugUtils.Assert(!_type.IsValueType, "Setup methods is not supported for value types");
                _setup = result => setup((T)result);
            }
            return this;
        }

        public Binding<F> ToGetter<T>(Func<T,F> method, string tag = null)
        {
            DependencyGraphBuilder.Bind(typeof(F), typeof(T), tag);
            _type = typeof(T);
            _getter = t => method((T)t);
            _toType = ToType.Method;
            _tag = null;
            _container.AddLookup(this, _type, _tag);
            return this;
        }

        public object Resolve()
        {
            if(!_validInstance)
            {
                DependencyGraphBuilder.StartCreation(typeof(F), _tag);
                if(_toType == ToType.Single)
                {
                    DependencyGraphBuilder.StartCreation(_type, _tag);
                    if(!_type.IsValueType)
                    {
                        var construct = _type.GetConstructor(new Type[]{ });
                        _instance = (F)construct.Invoke(new object[]{ });
                    }
                    DependencyGraphBuilder.Finalize(_type, _instance);
                }
                else if(_toType == ToType.Lookup)
                {
                    _instance = (F)_container.Resolve(_type, _tag, null);
                }
                else if(_toType == ToType.Method)
                {
                    if(_method != null)
                    {
                        DependencyGraphBuilder.StartCreation(_type, _tag);
                        _instance = _method();
                        DependencyGraphBuilder.Finalize(_type, _instance);
                    }
                    else if(_getter != null)
                    {
                        var param = _container.Resolve(_type, _tag, null);
                        _instance = _getter(param);
                    }

                }
                _validInstance = true;
                DependencyGraphBuilder.Finalize(typeof(F), _instance);
            }

            return _instance;
        }

        public void OnResolutionFinished()
        {
            Resolved = true;

            if(_setup != null && _instance != null)
            {
                // Execute a copy to avoid recursive calls in circular dependencies
                var setup = _setup;
                _setup = null;
                DependencyGraphBuilder.StartSetup(_type, _tag);
                setup(_instance);
            }
        }

        public override string ToString()
        {
            return string.Format("[Binding {0} -> {1} {2}]", typeof(F), _toType, _type);
        }
    }

    public struct BindingKey
    {
        public Type Type;
        public string Tag;

        public BindingKey(Type type, string tag)
        {
            Type = type;
            Tag = tag;
        }
    }
}