using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public sealed class SocialPlayer
    {
        public interface IComponent
        {

        }

        public sealed class BasicData : IComponent
        {
            public string Uid;
            public string Name;
            public int Level;
            public int Score;

            public override string ToString()
            {
                var builder = StringUtils.StartBuilder();
                builder
                    .AppendLine("BasicData:")
                    .Append("\tId: ").AppendLine(Uid)
                    .Append("\tName: ").AppendLine(Name)
                    .Append("\tLevel: ").AppendLine(Level.ToString())
                    .Append("\tScore: ").AppendLine(Score.ToString());
                return StringUtils.FinishBuilder(builder);
            }
        }

        public string Uid
        { 
            get
            { 
                return GetComponent<BasicData>().Uid;
            } 
        }

        public string Name
        { 
            get
            { 
                return GetComponent<BasicData>().Name;
            } 
        }

        public int Level
        { 
            get
            { 
                return GetComponent<BasicData>().Level;
            } 
        }

        public int Score
        { 
            get
            { 
                return GetComponent<BasicData>().Score;
            } 
        }

        readonly Dictionary<RuntimeTypeHandle, IComponent> _components;

        public SocialPlayer()
        {
            _components = new Dictionary<RuntimeTypeHandle, IComponent>();
        }

        public void AddComponent(IComponent component)
        {
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

        public override string ToString()
        {
            var builder = StringUtils.StartBuilder();
            var itr = _components.GetEnumerator();
            while(itr.MoveNext())
            {
                builder.Append(itr.Current.Value.ToString());
            }
            return StringUtils.FinishBuilder(builder);
        }
    }
}