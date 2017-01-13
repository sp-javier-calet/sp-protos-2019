using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class SocialPlayer
    {
        public interface IComponent
        {

        }

        public string Uid;
        public string Name;
        public int Level;
        public int Score;

        readonly Dictionary<RuntimeTypeHandle, IComponent> _components;

        public SocialPlayer()
        {
            _components = new Dictionary<RuntimeTypeHandle, IComponent>();
        }

        public void AddComponent(IComponent component)
        {
            DebugUtils.Assert(!_components.ContainsKey(Type.GetTypeHandle(component)), "Adding a Component to SocialPlayer that already exists");
            _components[Type.GetTypeHandle(component)] = component;
        }

        public bool HasComponent<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T).TypeHandle);
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            IComponent component = null;
            _components.TryGetValue(typeof(T).TypeHandle, out component);
            DebugUtils.Assert(component != null, "SocialPlayer should have this component");
            return component as T;
        }
    }
}