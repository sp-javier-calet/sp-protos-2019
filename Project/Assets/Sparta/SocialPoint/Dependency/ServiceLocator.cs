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
        public ResolveException(Type t, string tag=null):
        base("Could not resolve type "+t+" tag "+tag+".")
        {
        }
    }

    public class Binding<F>
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

        public void ToSingle()
        {
            ToSingle<F>();
        }

        public void ToSingle<T>() where T : F
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
            _method = method;
            _toType = ToType.Method;
        }

        public void ToGetter<T>(Func<T,F> method)
        {
            _type = typeof(T);
            _getter = (t) => method((T)t);
            _toType = ToType.Method;
        }

        public F Resolve()
        {
            F val;
            if(!TryResolve(out val))
            {
                throw new ResolveException(typeof(F));
            }
            return val;
        }

        public bool TryResolve(out F val)
        {
            if(_instance != null)
            {
                val = _instance;
                return true;
            }
            if(_toType == ToType.Single)
            {
                _instance = (F)_container.Create(_type);
                val = _instance;
                return true;
            }
            else if(_toType == ToType.Lookup)
            {
                _instance = (F)_container.Resolve(_type);
                val = _instance;
                return true;
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
                val = _instance;
                return true;
            }
            val = default(F);
            return false;
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

    public class ServiceLocator : MonoBehaviourSingleton<ServiceLocator>
    {
        [SerializeField]
        IInstaller[] _installers;

        List<IInstaller> _installedInstallers = new List<IInstaller>();

        Dictionary<BindingKey, List<object>> _bindings = new Dictionary<BindingKey, List<object>>();

        void AddBinding(object binding, Type type, string tag=null)
        {
            List<object> list;
            var key = new BindingKey( type, tag );
            if(!_bindings.TryGetValue(key, out list))
            {
                list = new List<object>();
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

        public bool TryResolve<T>(out object val)
        {
            return TryResolve(typeof(T), out val);
        }

        public bool TryResolve(Type type, out object val)
        {
            return TryResolve(type, null, out val);
        }

        public T Resolve<T>(string tag=null)
        {
            return (T)Resolve(typeof(T), tag);
        }

        public object Resolve(Type type, string tag=null)
        {
            object obj;
            if(!TryResolve(type, tag, out obj))
            {
                throw new ResolveException(type, tag);
            }
            return obj;
        }

        public T OptResolve<T>(string tag=null, T def=default(T))
        {
            return (T)OptResolve(typeof(T), tag, def);
        }

        public object OptResolve(Type t, string tag=null, object def=null)
        {
            object obj;
            if(!TryResolve(t, tag, out obj))
            {
                return def;
            }
            return obj;
        }

        public bool TryResolve<T>(string tag, out object val)
        {
            return TryResolve(typeof(T), tag, out val);
        }

        public bool TryResolve(Type type, string tag, out object val)
        {
            List<object> objs;
            if(_bindings.TryGetValue(new BindingKey(type, tag), out objs))
            {
                if(objs.Count > 0)
                {
                    val = objs[0];
                    return true;
                }
            }
            val = null;
            return false;
        }

        public List<object> ResolveList(Type type, string tag=null)
        {
            var objs = new List<object>();
            if(_bindings.TryGetValue(new BindingKey(type, tag), out objs))
            {
                return objs;
            }
            return objs;
        }

        public T[] ResolveList<T>(string tag=null)
        {
            var objs = new List<object>();
            var type = typeof(T);
            if(_bindings.TryGetValue(new BindingKey(type, tag), out objs))
            {
                var arr = new T[objs.Count];
                for(var i = 0; i < arr.Length; i++)
                {
                    arr[i] = (T)objs[i];
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
            object obj;
            if(TryResolve(type, out obj))
            {
                return obj;
            }
            return type.GetConstructor(new Type[]{}).Invoke(new object[]{});
        }

        const string GlobalInstallersResource = "GlobalInstallers";

        public void Start()
        {
            var globalConfig = Resources.Load<GlobalInstallerConfig>(GlobalInstallersResource);
            for(var i = 0; i < globalConfig.Installers.Length; i++)
            {
                Install(globalConfig.Installers[i]);
            }
            for(var i = 0; i < _installers.Length; i++)
            {
                Install(_installers[i]);
            }
        }
    }

    public static class ServiceLocatorExtensions
    {

        public static void Install<T>(this ServiceLocator locator) where T : IInstaller
        {
            locator.Install(default(T));
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
