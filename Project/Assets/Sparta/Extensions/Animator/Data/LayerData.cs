using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public class LayerData : INetworkShareable
    {
        public StateMachineData StateMachine;

        public void Serialize(IWriter writer)
        {
            StateMachine.Serialize(writer);
        }

        public void Deserialize(IReader reader)
        {
            StateMachine = reader.Read<StateMachineData>();
        }
    }
}
