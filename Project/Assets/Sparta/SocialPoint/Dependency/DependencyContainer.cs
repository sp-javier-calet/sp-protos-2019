using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Dependency.Graph;

namespace SocialPoint.Dependency
{
    public sealed class DependencyContainer : IDisposable
    {
        const string Tag = "DependencyContainer";

        List<IInstaller> _installed;
        Dictionary<BindingKey, List<IBinding>> _bindings;
        Dictionary<BindingKey, List<IBinding>> _defaultBindings;
        HashSet<IBinding> _resolving;
        List<IBinding> _resolved;
        Dictionary<IBinding, HashSet<object>> _instances;
        Dictionary<BindingKey, List<IBinding>> _lookups;
        Dictionary<BindingKey, List<BindingKey>> _aliases;
        Dictionary<BindingKey, List<IListener>> _listeners;

        public DependencyContainer()
        {
            _installed = new List<IInstaller>();
            _bindings = new Dictionary<BindingKey, List<IBinding>>(new BindingKeyComparer());
            _defaultBindings = new Dictionary<BindingKey, List<IBinding>>(new BindingKeyComparer());
            _resolving = new HashSet<IBinding>();
            _resolved = new List<IBinding>();
            var comparer = new ReferenceComparer<IBinding>();
            _instances = new Dictionary<IBinding, HashSet<object>>(comparer);
            _lookups = new Dictionary<BindingKey, List<IBinding>>(new BindingKeyComparer());
            _aliases = new Dictionary<BindingKey, List<BindingKey>>(new BindingKeyComparer());
            _listeners = new Dictionary<BindingKey, List<IListener>>(new BindingKeyComparer());
        }

        public void AddBindingWithInstance<T>(IBinding binding, Type type, T instance, string tag = null)
        {
            AddBinding(binding, type, tag);
            AddInstance(binding, instance);
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

            list.Add(binding);
            Log.v(Tag, string.Format("Added binding <{0}> for type `{1}`", tag, type.Name));
        }

        public void AddDefaultBinding(IBinding binding, Type type, string tag = null)
        {
            List<IBinding> list;
            var key = new BindingKey(type, tag);
            if(!_defaultBindings.TryGetValue(key, out list))
            {
                list = new List<IBinding>();
                _defaultBindings.Add(key, list);
            }

            list.Add(binding);
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

            // Add alias
            List<BindingKey> aliasesList;
            if(!_aliases.TryGetValue(key, out aliasesList))
            {
                aliasesList = new List<BindingKey>();
                _aliases.Add(key, aliasesList);
            }
            aliasesList.Add(binding.Key);

            Log.v(Tag, string.Format("Added lookup <{0}> for type `{1}`", tag, type.Name));
        }

        public void AddListener(IListener listener, Type type, string tag = null)
        {
            List<IListener> list;
            var key = new BindingKey(type, tag);
            if(!_listeners.TryGetValue(key, out list))
            {
                list = new List<IListener>();
                _listeners.Add(key, list);
            }
            list.Add(listener);
            Log.v(Tag, string.Format("Added binding <{0}> for type `{1}`", tag, type.Name));
        }

        public bool HasBinding<T>(string tag = null)
        {
            return _bindings.ContainsKey(new BindingKey(typeof(T), tag)) || _defaultBindings.ContainsKey(new BindingKey(typeof(T), tag));
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

        List<T> ResolveListFrom<T>(IDictionary<BindingKey, List<IBinding>> container, string tag)
        {
            List<IBinding> bindings;
            var type = typeof(T);
            var list = new List<T>();
            if(container.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                for(var i = 0; i < bindings.Count; i++)
                {
                    object result;
                    if(TryResolve(bindings[i], out result))
                    {
                        list.Add((T)result);
                    }
                }
            }
            return list;
        }

        public List<T> ResolveList<T>(string tag = null)
        {
            var list = ResolveListFrom<T>(_bindings, tag);

            if(list.Count == 0)
            {
                list = ResolveListFrom<T>(_defaultBindings, tag);
            }
            Log.v(Tag, string.Format("Resolved List <{0}> for type `{1}`. Size {2}", tag, typeof(T).Name, list.Count));
            return list;
        }

        object ResolveFrom(IDictionary<BindingKey, List<IBinding>> container, Type type, string tag, object def)
        {
            List<IBinding> bindings;
            if(container.TryGetValue(new BindingKey(type, tag), out bindings))
            {
                for(var i = 0; i < bindings.Count; i++)
                {
                    object resolved;
                    if(TryResolve(bindings[i], out resolved))
                    {
                        return resolved;
                    }
                    }
                }
            return def;
            }

        public object Resolve(Type type, string tag = null, object def = null)
        {
            var result = ResolveFrom(_bindings, type, tag, def);

            if(result == def)
            {
                result = ResolveFrom(_defaultBindings, type, tag, def);
            }

            if(result == null)
            {
                Log.w(Tag, string.Format("Resolved instance <{0}> for type `{1}`. {2}", tag, type.Name, "Default as null"));
            }
            else
            {
                Log.v(Tag, string.Format("Resolved instance <{0}> for type `{1}`. {2}", tag, type.Name, result == def ? "Default" : "Found"));
            }
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
                for(var j = 0; j < listeners.Count; ++j)
                {
                    listeners[j].OnResolved(instance);
                }
            }
        }

        public void Clear()
        {
            Dispose();
            _bindings.Clear();
            _defaultBindings.Clear();
            _installed.Clear();
            _resolved.Clear();
            _resolving.Clear();
            _lookups.Clear();
            _aliases.Clear();
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

        List<IListener> FindListeners(IBinding binding)
        {
            var listeners = new List<IListener>();

            // Look for direct bindings
            List<IListener> keyListeners;
            if(_listeners.TryGetValue(binding.Key, out keyListeners))
            {
                listeners.AddRange(keyListeners);
            }
                
            // Look for aliased bindings
            List<BindingKey> list;
            if(_aliases.TryGetValue(binding.Key, out list))
            {
                for(var i = 0; i < list.Count; i++)
                {
                    var key = list[i];
                    if(_listeners.TryGetValue(key, out keyListeners))
                    {
                        listeners.AddRange(keyListeners);
                    }
                } 
            }
            return listeners;
        }

        BindingKey FindBindingKey(IBinding binding)
        {
            using(var itr = _bindings.GetEnumerator())
            {
                while(itr.MoveNext())
                {
                    if(itr.Current.Value.Contains(binding))
                    {
                        return itr.Current.Key;
                    }
                }
            }
            using(var itr = _defaultBindings.GetEnumerator())
            {
            while(itr.MoveNext())
            {
                if(itr.Current.Value.Contains(binding))
                {
                        return itr.Current.Key;
                    }
                }
            }

            return new BindingKey();
        }

        bool IsLookup(BindingKey from, BindingKey to)
        {
            if(from.Type == to.Type && from.Tag == to.Tag)
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

        HashSet<object> FindInstancesFrom(IDictionary<BindingKey, List<IBinding>> container, BindingKey fromKey, BindingKey filterKey, bool remove)
        {
            var instances = new HashSet<object>();
            using(var itr = container.GetEnumerator())
            {
            while(itr.MoveNext())
            {
                HashSet<object> bindingInstances;
                var bindings = itr.Current.Value;
                var key = itr.Current.Key;
                for(var i = 0; i < bindings.Count; i++)
                {
                    if(filterKey.Type != null && (filterKey.Type != key.Type || filterKey.Tag != key.Tag))
                    {
                        continue;
                    }
                    var binding = bindings[i];
                    var bindingKey = binding.Key;
                    bool isInstanceBinding = fromKey.Type == bindingKey.Type && fromKey.Tag == bindingKey.Tag;

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
            }
            return instances;
        }

        HashSet<object> FindInstances(BindingKey fromKey, BindingKey filterKey, bool remove = false)
        {
            var instances = new HashSet<object>();
            instances.UnionWith(FindInstancesFrom(_bindings, fromKey, filterKey, remove));
            instances.UnionWith(FindInstancesFrom(_defaultBindings, fromKey, filterKey, remove));

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
