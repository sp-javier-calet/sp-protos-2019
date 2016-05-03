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
    }

    public class UnityComponentBinding<F> : IBinding where F : Component
    {
        ServiceLocator _container;

        public UnityComponentBinding(ServiceLocator container)
        {
            _container = container;
        }

        public object Resolve()
        {
            return _container.gameObject.AddComponent<F>();
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
        Func<F> _method;
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

        public void ToSingleInstance<T>(T instance) where T : F
        {
            _toType = ToType.Single;
            _instance = instance;
        }

        public void ToLookup<T>() where T : F
        {
            _toType = ToType.Lookup;
            _type = typeof(T);
        }

        public void ToSingleMethod<T>(Func<T> method) where T : F
        {
            _type = typeof(T);
            _method = () => method();
            _toType = ToType.Method;
        }

        public void ToGetter<T>(Func<T,F> method)
        {
            _type = typeof(T);
            _getter = (t) => method((T)t);
            _toType = ToType.Method;
        }

        public object Resolve()
        {
            if(_instance != null)
            {
            }
            else if(_toType == ToType.Single)
            {
                _instance = (F)_container.Create(_type);
            }
            else if(_toType == ToType.Lookup)
            {
                _instance = (F)_container.Resolve(_type);

            }
            else if(_toType == ToType.Method)
            {
                if(_method != null)
                {
                    _instance = (F)_method();
                }
                else if(_getter != null)
                {
                    var param = _container.Resolve(_type);
                    _instance = (F)_getter(param);
                }
            }
            return _instance;
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
        //List<IInitializable> _initializedInitializables = new List<IInitializable>();
        Dictionary<BindingKey, List<IBinding>> _bindings = new Dictionary<BindingKey, List<IBinding>>();

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
            /*
            var inits = ResolveArray<IInitializable>();
            for(var i = 0; i < inits.Length; i++)
            {
                var init = inits[i];
                if(!_initializedInitializables.Contains(init))
                {
                    init.Initialize();
                    _initializedInitializables.Add(init);
                }
            }*/
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

        public object Resolve(Type type, string tag=null, object def=null)
        {
            List<IBinding> bindings;
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                if(bindings.Count > 0)
                {
                    var binding = bindings[0];
                    Debug.Log("resolve " + binding);
                    return binding.Resolve();
                }
            }
            return def;
        }

        public List<T> ResolveList<T>(string tag=null)
        {
            return new List<T>(ResolveArray<T>(tag));
        }

        public T[] ResolveArray<T>(string tag=null)
        {
            var objs = new List<IBinding>();
            var type = typeof(T);
            if(_bindings.TryGetValue(new BindingKey(type, tag), out objs))
            {
                var arr = new T[objs.Count];
                for(var i = 0; i < arr.Length; i++)
                {
                    var binding = objs[i];
                    Debug.Log("resolve array " + binding);
                    arr[i] = (T)binding.Resolve();
                }
                return arr;
            }
            return null;
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        public object Create(Type type)
        {
            /*
            object obj;
            if(TryResolve(type, out obj))
            {
                return obj;
            }*/
            var construct = type.GetConstructor(new Type[]{ });
            if(construct == null)
            {
                throw new ResolveException("Type " + type + " does not have a default constructor");
            }
            return construct.Invoke(new object[]{});
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
            locator.Bind<T>(tag).ToSingleInstance(instance);
        }
    }
}
