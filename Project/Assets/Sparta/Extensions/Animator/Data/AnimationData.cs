using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public class AnimationData : INetworkShareable
    {
        public string Name;
        public float Length;
        public bool Loop;
        public AnimationEventData[] Events;

        public void Serialize(IWriter writer)
        {
            writer.Write(Name);
            writer.Write(Length);
            writer.Write(Loop);
            writer.WriteArray<AnimationEventData>(Events);
        }

        public void Deserialize(IReader reader)
        {
            Name = reader.ReadString();
            Length = reader.ReadSingle();
            Loop = reader.ReadBoolean();
            Events = reader.ReadArray<AnimationEventData>();
        }
    }
}
