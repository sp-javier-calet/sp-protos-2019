using System;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public interface IInitializable
    {
        void Initialize();
    }

    public class ResolveException : InvalidOperationException
    {
        public ResolveException(string msg):base(msg)
        {
        }
    }

    public interface IBinding
    {
        object Resolve();
        void OnResolutionFinished();
    }

    public class UnityComponentBinding<F> : IBinding where F : Component
    {
        ServiceLocator _container;
        Action<F> _setup;
        F _instance;

        public UnityComponentBinding(ServiceLocator container)
        {
            _container = container;
        }
            
        public UnityComponentBinding<F> WithSetup<T>(Action<F> setup)
        {
            _setup = setup;
            return this;
        }

        public object Resolve()
        {
            if(_instance == null)
            {
                _instance = _container.gameObject.AddComponent<F>();
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
            return string.Format("[UnityComponentBinding {0}]", typeof(F));
        }
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
        ServiceLocator _container;

        public Binding(ServiceLocator container)
        {
            _container = container;
        }

        public void ToSingle<T>() where T : F, new()
        {
            _toType = ToType.Single;
            _type = typeof(T);
        }

        public void ToInstance<T>(T instance) where T : F
        {
            _toType = ToType.Single;
            _instance = instance;
        }

        public void ToLookup<T>(string tag=null) where T : F
        {
            _toType = ToType.Lookup;
            _type = typeof(T);
            _tag = tag;
        }

        public void ToMethod<T>(Func<T> method, Action<T> setup=null) where T : F
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
        }

        public void ToGetter<T>(Func<T,F> method, string tag=null)
        {
            _type = typeof(T);
            _getter = (t) => method((T)t);
            _tag = tag;
            _toType = ToType.Method;
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
                Debug.Log("OnResolutionFinished " + this);
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

    public sealed class ServiceLocator : MonoBehaviourSingleton<ServiceLocator>
    {
        List<IInstaller> _installedInstallers = new List<IInstaller>();
        List<IInitializable> _initializedInitializables = new List<IInitializable>();
        Dictionary<BindingKey, List<IBinding>> _bindings = new Dictionary<BindingKey, List<IBinding>>();
        HashSet<IBinding> _resolving = new HashSet<IBinding>();
        List<IBinding> _resolved = new List<IBinding>();

        void AddBinding(IBinding binding, Type type, string tag=null)
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

        public Binding<T> Bind<T>(string tag = null)
        {
            var bind = new Binding<T>(this);
            AddBinding(bind, typeof(T), tag);
            return bind;
        }

        public void BindUnityComponent<T>(string tag = null) where T : Component
        {
            var bind = new UnityComponentBinding<T>(this);
            AddBinding(bind, typeof(T), tag);
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
            for(int i = 0; i < _installedInstallers.Count; i++)
            {
                if(_installedInstallers[i].GetType() == type)
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
            _installedInstallers.Add(installer);
        }

        public void Install(IInstaller[] installers)
        {
            for(var i = 0; i < installers.Length; i++)
            {
                Install(installers[i]);
            }
        }

        public void Initialize()
        {
            var inits = ResolveArray<IInitializable>();
            for(var i = 0; i < inits.Length; i++)
            {
                var init = inits[i];
                if(!_initializedInitializables.Contains(init))
                {
                    init.Initialize();
                    _initializedInitializables.Add(init);
                }
            }
        }

        void AddBindedComponents()
        {
            List<IBinding> bindings;
            var type = typeof(MonoBehaviour);
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {

            }
        }
            
        public T Resolve<T>(string tag=null, T def=default(T))
        {
            return (T)Resolve(typeof(T), tag, def);
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
            Debug.Log("Resolve " + type + " " + tag);
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
            
        public T[] ResolveArray<T>(string tag=null)
        {
            return ResolveList<T>(tag).ToArray();
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
        }

        const string GlobalInstallersResource = "GlobalInstallers";

        override protected void SingletonAwakened()
        {
            var globalConfig = Resources.Load<GlobalDependencyConfigurer>(GlobalInstallersResource);
            if(globalConfig != null)
            {
                Install(globalConfig.Installers);
            }
        }
    }

    public static class ServiceLocatorExtensions
    {
        public static void Install<T>(this ServiceLocator locator) where T : IInstaller, new()
        {
            locator.Install(new T());
        }

        public static Binding<T> Rebind<T>(this ServiceLocator locator, string tag=null)
        {
            locator.Remove<T>(tag);
            return locator.Bind<T>(tag);
        }

        public static void BindInstance<T>(this ServiceLocator locator, string tag, T instance)
        {
            locator.Bind<T>(tag).ToInstance(instance);
        }
    }
}
