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

    public class Binding<F>
    {
        public Binding(ServiceLocator container)
        {
        }

        public void ToSingle()
        {
        }

        public void ToSingle<T>() where T : F
        {
        }

        public void ToSingleInstance<T>(T instance) where T : F
        {
        }

        public void ToLookup<T>() where T : F
        {
        }

        public void ToSingleMethod<T>(Func<T> method) where T : F
        {
        }

        public void ToGetter<T>(Func<T,F> method) where T : F
        {
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
            return false;
        }

        public void Install(IInstaller installer)
        {
            installer.Container = this;
            installer.InstallBindings();
            _installedInstallers.Add(installer);
        }

        public T Resolve<T>()
        {
            return default(T);
        }

        public List<T> ResolveList<T>()
        {
            return new List<T>();
        }

        public T TryResolve<T>()
        {
            return default(T);
        }

        public T Resolve<T>(string tag)
        {
            return default(T);
        }

        public T TryResolve<T>(string tag, T def=default(T))
        {
            return default(T);
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
