#if !ZEN_NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    public class GameObjectTransientProvider : ProviderBase
    {
        readonly string _name;
        readonly Type _componentType;

        public GameObjectTransientProvider(
            Type componentType, string name)
        {
            Assert.That(componentType.DerivesFrom<Component>());
            _componentType = componentType;
            _name = name;
        }

        public override Type GetInstanceType()
        {
            return _componentType;
        }

        public override object GetInstance(InjectContext context)
        {
            Assert.That(_componentType.DerivesFromOrEqual(context.MemberType));

            var name = string.IsNullOrEmpty(_name) ? _componentType.Name() : _name;

            return context.Container.InstantiateComponentOnNewGameObjectExplicit(
                _componentType, name, new List<TypeValuePair>(), context);
        }

        public override IEnumerable<ZenjectResolveException> ValidateBinding(InjectContext context)
        {
            return context.Container.ValidateObjectGraph(_componentType, context);
        }
    }
}

#endif

