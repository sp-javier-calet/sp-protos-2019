#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Zenject
{
    // NOTE: we need the provider seperate from the creator because
    // if we return the same provider multiple times then the condition
    // will get over-written
    internal class PrefabSingletonProvider : ProviderBase
    {
        PrefabSingletonLazyCreator _creator;
        DiContainer _container;
        Type _instanceType;

        public PrefabSingletonProvider(
            DiContainer container, Type instanceType, PrefabSingletonLazyCreator creator)
        {
            _creator = creator;
            _container = container;
            _instanceType = instanceType;
        }

        public override void Dispose()
        {
            _creator.DecRefCount();
        }

        public override Type GetInstanceType()
        {
            return _instanceType;
        }

        public override object GetInstance(InjectContext context)
        {
            return _creator.GetComponent(_instanceType, context);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            if (!_creator.ContainsComponent(_instanceType))
            {
                yield return new ZenjectResolveException(
                    "Could not find component of type '{0}' in prefab with name '{1}' \nObject graph:\n{2}"
                    .Fmt(_instanceType.Name(), _creator.Prefab.name, context.GetObjectGraphString()));
                yield break;
            }

            // In most cases _instanceType will be a MonoBehaviour but we also want to allow interfaces
            // And in that case we can't validate it
            if (!_instanceType.IsAbstract)
            {
                // Note that we always want to cache _container instead of using context.Container
                // since for singletons, the container they are accessed from should not determine
                // the container they are instantiated with
                // Transients can do that but not singletons
                foreach (var err in _container.ValidateObjectGraph(_instanceType, context))
                {
                    yield return err;
                }
            }
        }
    }
}

#endif
