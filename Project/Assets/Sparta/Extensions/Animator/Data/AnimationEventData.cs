using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public enum EventType
    {
        Default,
        Visual
    }

    public class AnimationEventData : INetworkShareable, IAnimationEvent
    {
        public string StringValue{ get; set; }

        public int IntValue{ get; set; }

        public float FloatValue{ get; set; }

        public float Time{ get; set; }

        public bool IsVisual { get; set; }

        public void Serialize(IWriter writer)
        {
            writer.Write(StringValue);
            writer.Write(IntValue);
            writer.Write(FloatValue);
            writer.Write(Time);
            writer.Write(IsVisual);
        }

        public void Deserialize(IReader reader)
        {
            StringValue = reader.ReadString();
            IntValue = reader.ReadInt32();
            FloatValue = reader.ReadSingle();
            Time = reader.ReadSingle();
            IsVisual = reader.ReadBoolean();
        }

        public override string ToString()
        {
            return string.Format("[AnimationEventData time={0} str={1} int={2} float={3}]", Time, StringValue, IntValue, FloatValue);
        }
    }
}
