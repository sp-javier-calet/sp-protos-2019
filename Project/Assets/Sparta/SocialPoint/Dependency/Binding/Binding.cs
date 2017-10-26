using System;
using SocialPoint.Base;
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

        public int Priority { get; private set; }

        public Binding(DependencyContainer container, int priority)
        {
            _container = container;
            Priority = priority;
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
                switch(_toType)
                {
                case ToType.Single:
                    DependencyGraphBuilder.StartCreation(_type, _tag);
                    if(!_type.IsValueType)
                    {
                        var construct = _type.GetConstructor(new Type[]{ });
                        _instance = (F)construct.Invoke(new object[]{ });
                    }
                    DependencyGraphBuilder.Finalize(_type, _instance);
                    break;

                case ToType.Lookup:
                    _instance = (F)_container.Resolve(_type, _tag, null);
                    break;

                case ToType.Method:
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
                    break;
                }
                _validInstance = true;
                DependencyGraphBuilder.Finalize(typeof(F), _instance);
            }

            return _instance;
        }

        public void OnResolved()
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

    public static class DependencyContainerExtensions
    {
        public static void Add<F, T>(this DependencyContainer container, T instance, string tag = null) where T : F
        {
            var bind = new Binding<F>(container, DependencyContainer.NormalBindingPriority);
            bind.ToInstance(instance);
            container.AddBindingWithInstance(bind, typeof(F), instance, tag);
        }

        public static void Install<T>(this DependencyContainer container) where T : IInstaller, new()
        {
            container.Install(new T());
        }

        public static Binding<T> Rebind<T>(this DependencyContainer container, string tag = null)
        {
            var bind = new Binding<T>(container, DependencyContainer.NormalBindingPriority);
            if(!container.HasBinding<T>(tag))
            {
                container.AddBinding(bind, typeof(T), tag);
            }
            else
            {
                Log.w("DependencyContainer", string.Format("Skipping binding of {0} <{1}>", typeof(T).Name, tag ?? string.Empty));
            }
            return bind;
        }

        public static Binding<T> Bind<T>(this DependencyContainer container, string tag = null)
        {
            var bind = new Binding<T>(container, DependencyContainer.NormalBindingPriority);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static Binding<T> BindDefault<T>(this DependencyContainer container, string tag = null)
        {
            var bind = new Binding<T>(container, DependencyContainer.DefaultBindingPriority);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static Listener<T> Listen<T>(this DependencyContainer container, string tag = null)
        {
            var listener = new Listener<T>();
            container.AddListener(listener, typeof(T), tag);
            return listener;
        }

        public static void Install(this DependencyContainer container, IInstaller[] installers)
        {
            for(var i = 0; i < installers.Length; i++)
            {
                container.Install(installers[i]);
            }
        }

        public static T Resolve<T>(this DependencyContainer container, string tag = null, T def = default(T))
        {
            return (T)container.Resolve(typeof(T), tag, def);
        }
    }
}
