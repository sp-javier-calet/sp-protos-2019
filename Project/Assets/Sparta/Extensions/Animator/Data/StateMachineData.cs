using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public class StateMachineData : INetworkShareable
    {
        public string Name;
        public string DefaultState;
        public StateData[] States;
        public TransitionData[] AnyStateTransitions;

        public void Serialize(IWriter writer)
        {
            writer.Write(Name);
            writer.Write(DefaultState);
            writer.WriteArray<StateData>(States);
            writer.WriteArray<TransitionData>(AnyStateTransitions);
        }

        public void Deserialize(IReader reader)
        {
            Name = reader.ReadString();
            DefaultState = reader.ReadString();
            States = reader.ReadArray<StateData>();
            AnyStateTransitions = reader.ReadArray<TransitionData>();
        }
    }
}
