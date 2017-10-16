using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public enum InterruptionSourceType : byte
    {
        None = 0,
        Source,
        Destination,
        SourceThenDestination,
        DestinationThenSource
    }

    public class TransitionData : INetworkShareable
    {
        public string ToState;
        public float Duration;
        public float ExitTime;
        public bool HasFixedDuration;
        public bool HasExitTime;
        public InterruptionSourceType InterruptionSource;
        public bool OrderedInterruption;
        public ConditionData[] Conditions;

        public void Serialize(IWriter writer)
        {
            writer.Write(ToState);
            writer.Write(Duration);
            writer.Write(ExitTime);
            writer.Write(HasFixedDuration);
            writer.Write(HasExitTime);
            writer.Write((byte)InterruptionSource);
            writer.Write(OrderedInterruption);
            writer.WriteArray<ConditionData>(Conditions);
        }

        public void Deserialize(IReader reader)
        {
            ToState = reader.ReadString();
            Duration = reader.ReadSingle();
            ExitTime = reader.ReadSingle();
            HasFixedDuration = reader.ReadBoolean();
            HasExitTime = reader.ReadBoolean();
            InterruptionSource = (InterruptionSourceType)reader.ReadByte();
            OrderedInterruption = reader.ReadBoolean();
            Conditions = reader.ReadArray<ConditionData>();
        }
    }
}
