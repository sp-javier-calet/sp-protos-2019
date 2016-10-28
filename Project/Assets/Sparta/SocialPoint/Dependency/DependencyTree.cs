using System;
using System.Collections.Generic;

#define SPARTA_COLLECT_DEPENDENCIES

namespace SocialPoint.Dependency
{
    public static class DependencyTree
    {
        const string CollectDependenciesFlag = "SPARTA_COLLECT_DEPENDENCIES";

        enum BindActions
        {
            Bind,
            Alias,
            Remove,
            Resolve
        }
        
        class BindingInfo
        {
            public readonly string Type;
            public readonly string Tag;
            public readonly List<BindActions> History;
            public readonly List<BindingInfo> AliasesAs;
            public readonly List<BindingInfo> AliasTo;

            public BindingInfo(string name, string tag)
            {
                Type = name;
                Tag = tag;
                History = new List<BindActions>();
                AliasesAs = new List<BindingInfo>();
                AliasTo = new List<BindingInfo>();
            }
        }

        static readonly Dictionary<Type, Dictionary<string, BindingInfo>> _bindings = new Dictionary<Type, Dictionary<string, BindingInfo>>();

        static BindingInfo Get(Type type, string tag)
        {
            Dictionary<string, BindingInfo> typeBinding;
            BindingInfo info;
            if(_bindings.TryGetValue(type, out typeBinding))
            {
                if(!typeBinding.TryGetValue(tag, info))
                {
                    info = new BindingInfo(type.Name, tag);
                    typeBinding.Add(tag, info);
                }   
            }
            else
            {
                info = new BindingInfo(type.Name, tag);
                typeBinding = new Dictionary<string, BindingInfo>();
                typeBinding.Add(tag, info);
                _bindings.Add(type, typeBinding);
            }

            return info;
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void AddBinding(Type fromType, Type toType, string tag)
        {
            var binding = Get(fromType, tag);
            var binded = Get(toType, tag);
            binding.History.Add(BindActions.Bind);
            binding.AliasTo.Add(binded);
            binded.AliasesAs.Add(binding);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void AddLookup(Type fromType, string fromTag, Type toType, string toTag)
        {
            var alias = Get(fromType, fromTag);
            var aliased = Get(toType, toTag);
            alias.History.Add(BindActions.Bind);
            alias.AliasTo.Add(aliased);
            aliased.AliasesAs.Add(alias);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Remove(Type type, string tag)
        {
            Get(type, tag).History.Add(BindActions.Remove);
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void Reset()
        {
            _bindings.Clear();
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void OnInstall(Type installer)
        {
        }

        [System.Diagnostics.Conditional(CollectDependenciesFlag)]
        public static void OnResolve(Type type, string tag)
        {
        }
    }
}