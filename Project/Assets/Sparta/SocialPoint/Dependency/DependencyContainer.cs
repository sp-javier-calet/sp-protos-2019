using System;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public interface IBinding
    {
        object Resolve();
        void OnResolutionFinished();
    }

    public class Binding<F> : IBinding
    {
        enum ToType
        {
            Single,
            Lookup,
            Method
        }

        F _instance;
        ToType _toType;
        Type _type;
        string _tag;
        Func<F> _method;
        Action<object> _setup;
        Func<object, F> _getter;
        DependencyContainer _container;

        public Binding(DependencyContainer container)
        {
            _container = container;
        }

        public Binding<F> ToSingle<T>() where T : F, new()
        {
            _toType = ToType.Single;
            _type = typeof(T);
            return this;
        }

        public Binding<F> ToInstance<T>(T instance) where T : F
        {
            _toType = ToType.Single;
            _instance = instance;
            return this;
        }

        public Binding<F> ToLookup<T>(string tag=null) where T : F
        {
            _toType = ToType.Lookup;
            _type = typeof(T);
            _tag = tag;
            return this;
        }

        public Binding<F> ToMethod<T>(Func<T> method, Action<T> setup=null) where T : F
        {
            _type = typeof(T);
            _method = () => method();
            _toType = ToType.Method;

            _setup = null;
            if(setup != null)
            {
                _setup = (result) => {
                    setup((T)result);
                };
            }
            return this;
        }

        public Binding<F> ToGetter<T>(Func<T,F> method, string tag=null)
        {
            _type = typeof(T);
            _getter = (t) => method((T)t);
            _toType = ToType.Method;
            _tag = null;
            return this;
        }

        public object Resolve()
        {
            if(_instance != null)
            {
            }
            else if(_toType == ToType.Single)
            {
                var construct = _type.GetConstructor(new Type[]{ });
                _instance = (F) construct.Invoke(new object[]{});

            }
            else if(_toType == ToType.Lookup)
            {
                _instance = (F)_container.Resolve(_type, _tag, null);
            }
            else if(_toType == ToType.Method)
            {
                if(_method != null)
                {
                    _instance = (F)_method();
                }
                else if(_getter != null)
                {
                    var param = _container.Resolve(_type, _tag, null);
                    _instance = (F)_getter(param);
                }
            }
            return _instance;
        }

        public void OnResolutionFinished()
        {
            if(_setup != null && _instance != null)
            {
                _setup(_instance);
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

    public class DependencyContainer
    {
        List<IInstaller> _installed = new List<IInstaller>();
        Dictionary<BindingKey, List<IBinding>> _bindings = new Dictionary<BindingKey, List<IBinding>>();
        HashSet<IBinding> _resolving = new HashSet<IBinding>();
        List<IBinding> _resolved = new List<IBinding>();

        public void AddBinding(IBinding binding, Type type, string tag=null)
        {
            List<IBinding> list;
            var key = new BindingKey( type, tag );
            if(!_bindings.TryGetValue(key, out list))
            {
                list = new List<IBinding>();
                _bindings[key] = list;
            }
            list.Add(binding);
        }

        public bool Remove<T>(string tag = null)
        {
            return _bindings.Remove(new BindingKey( typeof(T), tag ));
        }

        public bool HasBinding<T>(string tag = null)
        {
            return _bindings.ContainsKey(new BindingKey( typeof(T), tag ));
        }

        public bool HasInstalled<T>() where T : IInstaller
        {
            var type = typeof(T);
            for(int i = 0; i < _installed.Count; i++)
            {
                if(_installed[i].GetType() == type)
                {
                    return true;
                }
            }
            return false;
        }

        public void Install(IInstaller installer)
        {
            installer.Container = this;
            installer.InstallBindings();
            _installed.Add(installer);
        }

        public List<T> ResolveList<T>(string tag=null)
        {
            var bindings = new List<IBinding>();
            var type = typeof(T);
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                var list = new List<T>();
                for(var i = 0; i < bindings.Count; i++)
                {
                    object result;
                    if(TryResolve(bindings[i], out result))
                    {
                        list.Add((T)result);
                    }
                }
                return list;
            }
            return new List<T>();
        }

        public object Resolve(Type type, string tag=null, object def=null)
        {
            List<IBinding> bindings;
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                for(var i = 0; i < bindings.Count; i++)
                {
                    object result;
                    if(TryResolve(bindings[i], out result))
                    {
                        return result;
                    }
                }
            }
            return def;
        }

        bool TryResolve(IBinding binding, out object result)
        {
            if(_resolving.Contains(binding))
            {
                result = null;
                return false;
            }

            _resolving.Add(binding);
            result = binding.Resolve();
            _resolving.Remove(binding);
            _resolved.Add(binding);
            if(_resolving.Count == 0)
            {
                var resolved = _resolved.ToArray();
                _resolved.Clear();
                for(var i = 0; i < resolved.Length; i++)
                {
                    resolved[i].OnResolutionFinished();
                }
            }
            return true;
        }

        public void Clear()
        {
            _bindings.Clear();
            _installed.Clear();
            _resolved.Clear();
            _resolving.Clear();
        }
    }

    public static class DependencyContainerExtensions
    {
        public static void Install<T>(this DependencyContainer container) where T : IInstaller, new()
        {
            container.Install(new T());
        }

        public static Binding<T> Rebind<T>(this DependencyContainer container, string tag=null)
        {
            container.Remove<T>(tag);
            return container.Bind<T>(tag);
        }


        public static Binding<T> Bind<T>(this DependencyContainer container, string tag = null)
        {
            var bind = new Binding<T>(container);
            container.AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public static void BindInstance<T>(this DependencyContainer container, string tag, T instance)
        {
            container.Bind<T>(tag).ToInstance(instance);
        }
            
        public static void Install(this DependencyContainer container, IInstaller[] installers)
        {
            for(var i = 0; i < installers.Length; i++)
            {
                container.Install(installers[i]);
            }
        }
            
        public static T Resolve<T>(this DependencyContainer container, string tag=null, T def=default(T))
        {
            return (T)container.Resolve(typeof(T), tag, def);
        }
    }
}
