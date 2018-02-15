using System;
using System.Collections.Generic;
using System.Linq;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Dependency.Graph;

namespace SocialPoint.Dependency
{
    public sealed class DependencyContainer : IDisposable
    {
        public const int DefaultBindingPriority = 0;
        public const int NormalBindingPriority = 1;

        const string Tag = "DependencyContainer";

        List<IInstaller> _installed;
        Dictionary<BindingKey, List<IBinding>> _bindings;
        HashSet<IBinding> _resolving;
        List<IBinding> _resolved;
        Dictionary<IBinding, HashSet<object>> _instances;
        Dictionary<BindingKey, List<IBinding>> _lookups;
        Dictionary<BindingKey, List<IListener>> _listeners;


        public DependencyContainer()
        {
            _installed = new List<IInstaller>();
            _resolving = new HashSet<IBinding>();
            _resolved = new List<IBinding>();
            _instances = new Dictionary<IBinding, HashSet<object>>();
            var keyComparer = new BindingKeyComparer();
            _bindings = new Dictionary<BindingKey, List<IBinding>>(keyComparer);
            _lookups = new Dictionary<BindingKey, List<IBinding>>(keyComparer);
            _listeners = new Dictionary<BindingKey, List<IListener>>(keyComparer);
        }

        public void AddBindingWithInstance<T>(IBinding binding, Type type, T instance, string tag = null)
        {
            AddBinding(binding, type, tag);
            AddInstance(binding, instance);
        }

        static void AddBindingSorted(IList<IBinding> list, IBinding binding)
        {
            for(int i = 0; i < list.Count; ++i)
            {
                var current = list[i];
                if(current.Priority <= binding.Priority)
                {
                    list.Insert(i, binding);
                    return;
                }
            }
            list.Add(binding);
        }

        public void AddBinding(IBinding binding, Type type, string tag = null)
        {
            List<IBinding> list;
            var key = new BindingKey(type, tag);
            if(!_bindings.TryGetValue(key, out list))
            {
                list = new List<IBinding>();
                _bindings.Add(key, list);
            }

            AddBindingSorted(list, binding);
            Log.v(Tag, string.Format("Added binding <{0}> for type `{1}`", tag, type.Name));
        }

        public void AddLookup(IBinding binding, Type type, string tag = null)
        {
            var key = new BindingKey(type, tag);

            // Add Lookup
            List<IBinding> lookupsList;
            if(!_lookups.TryGetValue(key, out lookupsList))
            {
                lookupsList = new List<IBinding>();
                _lookups.Add(key, lookupsList);
            }
            lookupsList.Add(binding);

            Log.v(Tag, string.Format("Added lookup <{0}> for type `{1}`", tag, type.Name));
        }

        public void AddListener(IListener listener, BindingKey[] bindingKeys)
        {
            for(var i = 0; i < bindingKeys.Length; i++)
            {
                List<IListener> list;
                var key = bindingKeys[i];
                if(!_listeners.TryGetValue(key, out list))
                {
                    list = new List<IListener>();
                    _listeners.Add(key, list);
                }
                if(!list.Contains(listener))
                {
                    list.Add(listener);
                }
                Log.v(Tag, string.Format("Added listener <{0}> for binding `{1}`", listener, key));
            }
        }

        public bool HasBinding<T>(string tag = null)
        {
            return _bindings.ContainsKey(new BindingKey(typeof(T), tag));
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
            if(installer == null)
            {
                return;
            }
            installer.Container = this;
            installer.InstallBindings();
            _installed.Add(installer);
        }

        public List<T> ResolveList<T>(string tag = null)
        {
            List<IBinding> bindings;
            var type = typeof(T);
            var list = new List<T>();
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                IBinding firstValidBinding = null;
                for(var i = 0; i < bindings.Count; i++)
                {
                    var currentBinding = bindings[i];
                    if(firstValidBinding != null && firstValidBinding.Priority != currentBinding.Priority)
                    {
                        //Exit if we found a valid binding and the next one is of less priority
                        break;
                    }
                    object result;
                    if(TryResolve(currentBinding, out result))
                    {
                        if(firstValidBinding == null)
                        {
                            firstValidBinding = currentBinding;
                        }
                        list.Add((T)result);
                    }
                }
            }

            Log.v(Tag, string.Format("Resolved List <{0}> for type `{1}`. Size {2}", tag, typeof(T).Name, list.Count));
            return list;
        }

        bool TrySearchAndResolve(Type type, string tag, out object result)
        {
            List<IBinding> bindings;
            if(_bindings.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                for(var i = 0; i < bindings.Count; i++)
                {
                    var currentBinding = bindings[i];
                    object resolved;
                    if(TryResolve(currentBinding, out resolved))
                    {
                        result = resolved;
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }


        public T Resolve<T>(string tag = null, T def = default(T))
        {
            return (T)Resolve(typeof(T), tag, def);
        }

        public object Resolve(Type type, string tag = null, object def = null)
        {
            object result;
            var found = TrySearchAndResolve(type, tag, out result);

            if(!found)
            {
                Log.w(Tag, string.Format("Resolved instance <{0}> for type `{1}`. Default is {2}", tag, type.Name, def));
                return def;
            }

            Log.v(Tag, string.Format("Resolved instance <{0}> for type `{1}`. Instance is {2}", tag, type.Name, result));
            return result;
        }

        void AddInstance(IBinding binding, object instance)
        {
            HashSet<object> instances;
            if(!_instances.TryGetValue(binding, out instances))
            {
                instances = new HashSet<object>();
                _instances[binding] = instances;
            }
            instances.Add(instance);
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
            AddInstance(binding, result);

            _resolving.Remove(binding);
            _resolved.Add(binding);

            if(_resolving.Count == 0)
            {
                var resolved = _resolved.ToArray();
                _resolved.Clear();
                for(var i = 0; i < resolved.Length; i++)
                {
                    var resolvedBinding = resolved[i];
                    if(!binding.Resolved)
                    {
                        NotifyResolutionFinished(resolvedBinding);
                    }
                }
            }
            return true;
        }

        void NotifyResolutionFinished(IBinding binding)
        {
            binding.OnResolved();

            var listeners = FindListeners(binding);
            if(listeners.Count > 0)
            {
                var instance = binding.Resolve();
                var itr = listeners.GetEnumerator();
                while(itr.MoveNext())
                {
                    var keyListeners = itr.Current.Value;
                    var keyBinding = itr.Current.Key;
                    for(var j = 0; j < keyListeners.Count; ++j)
                    {
                        keyListeners[j].OnResolved(keyBinding, instance);
                    }
                }
                itr.Dispose();
            }
        }

        public void Clear()
        {
            Dispose();
            _bindings.Clear();
            _installed.Clear();
            _resolved.Clear();
            _resolving.Clear();
            _lookups.Clear();
            _listeners.Clear();
            Log.v(Tag, "Depencency Container Cleared");
        }

        public enum InstallationPhase
        {
            Global,
            Install,
            Initialization
        }

        public void OnPhaseStart(InstallationPhase phase)
        {
            switch(phase)
            {
            case InstallationPhase.Global:
                DependencyGraphBuilder.StartGlobalInstall();
                break;
            case InstallationPhase.Install:
                DependencyGraphBuilder.StartInstall();
                break;
            case InstallationPhase.Initialization:
                DependencyGraphBuilder.StartInitialization();
                break;
            }
        }

        public void OnPhaseEnd()
        {
            DependencyGraphBuilder.EndPhase();
        }

        Dictionary<IBinding, List<IListener>> FindListeners(IBinding binding)
        {
            var listeners = new Dictionary<IBinding, List<IListener>>();

            // Look for direct bindings
            List<IListener> keyListeners;
            if(_listeners.TryGetValue(binding.Key, out keyListeners) && keyListeners.Count > 0)
            {
                listeners.Add(binding, keyListeners);
            }

            // Look for lookup bindings
            var bindings = new Dictionary<BindingKey, List<IBinding>>();
            FindLookups(binding, bindings);
            var itr = bindings.GetEnumerator();
            while(itr.MoveNext())
            {
                var keyBindings = itr.Current.Value;
                for(var i = 0; i < keyBindings.Count; i++)
                {
                    var keyBinding = keyBindings[i];
                    if(!listeners.ContainsKey(keyBinding) && _listeners.TryGetValue(keyBinding.Key, out keyListeners) && keyListeners.Count > 0)
                    {
                        listeners.Add(keyBinding, keyListeners);
                    }
                }
            }
            itr.Dispose();
            return listeners;
        }

        BindingKey FindBindingKey(IBinding binding)
        {
            var itr = _bindings.GetEnumerator();
            while(itr.MoveNext())
            {
                if(itr.Current.Value.Contains(binding))
                {
                    itr.Dispose();
                    return itr.Current.Key;
                }
            }
            itr.Dispose();
            return new BindingKey();
        }

        bool IsLookup(BindingKey from, BindingKey to)
        {
            if(from.Equals(to))
            {
                return true;
            }
            List<IBinding> bindings;
            var key = to;
            if(_lookups.TryGetValue(key, out bindings))
            {
                for(var i = 0; i < bindings.Count; i++)
                {
                    var key2 = FindBindingKey(bindings[i]);
                    if(IsLookup(from, key2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void FindLookups(IBinding binding, Dictionary<BindingKey, List<IBinding>> bindings)
        {
            if(bindings.ContainsKey(binding.Key))
            {
                return;
            }
            List<IBinding> list;
            if(_lookups.TryGetValue(binding.Key, out list))
            {
                bindings.Add(binding.Key, list);
                for(var i = 0; i < list.Count; i++)
                {
                    FindLookups(list[i], bindings);
                }
            }
        }

        HashSet<object> FindInstances(BindingKey fromKey, BindingKey filterKey, bool remove = false)
        {
            var instances = new HashSet<object>();
            var itr = _bindings.GetEnumerator();
            while(itr.MoveNext())
            {
                HashSet<object> bindingInstances;
                var bindings = itr.Current.Value;
                var key = itr.Current.Key;
                for(var i = 0; i < bindings.Count; i++)
                {
                    if(filterKey.Type != null && !filterKey.Equals(key))
                    {
                        continue;
                    }
                    var binding = bindings[i];
                    var bindingKey = binding.Key;
                    bool isInstanceBinding = fromKey.Equals(bindingKey);

                    if(isInstanceBinding || IsLookup(fromKey, key))
                    {
                        if(_instances.TryGetValue(binding, out bindingInstances))
                        {
                            var itr2 = bindingInstances.GetEnumerator();
                            while(itr2.MoveNext())
                            {
                                instances.Add(itr2.Current);
                            }
                            itr2.Dispose();
                            if(remove)
                            {
                                _instances.Remove(binding);
                            }
                        }
                    }
                }
            }
            itr.Dispose();
            return instances;
        }

        public void Dispose()
        {
            DisposeInstances(new BindingKey(null, null));
            _instances.Clear();
            Log.v(Tag, "Depencency Container Disposed");
        }

        void DisposeInstances(BindingKey key)
        {
            var disposables = FindInstances(new BindingKey(typeof(IDisposable), null), key, true);
            Log.v(Tag, string.Format("Disposing {0} instances", disposables.Count));
            var itr = disposables.GetEnumerator();
            while(itr.MoveNext())
            {
                Log.w("Disposing Container Instance: " + itr.Current.GetType().Name);
                var disposable = itr.Current as IDisposable;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
                else
                {
                    Log.e(string.Format("Type {0} does not implement IDisposable", itr.Current.GetType().Name));
                }
            }
            itr.Dispose();
        }
    }
}
