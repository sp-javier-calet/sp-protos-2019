using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public enum MotionDataType : byte
    {
        Invalid = 0,
        Clip,
        BlendTree,
    }

    public class MotionData : INetworkShareable
    {
        public MotionDataType Type;
        public AnimationData Animation;

        public float AnimationDuration
        {
            get
            {
                if(Animation != null && Type == MotionDataType.Clip)
                {
                    return Animation.Length;

                }
                return 0.0f;
            }
        }

        public bool Loop
        {
            get
            {
                if(Animation != null && Type == MotionDataType.Clip)
                {
                    return Animation.Loop;

                }
                return false;
            }
        }

        public AnimationEventData[] Events
        {
            get
            {
                if(Animation != null)
                {
                    return Animation.Events;
                }
                return null;
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)Type);
            bool writeClip = (Type == MotionDataType.Clip);
            writer.Write(writeClip);
            if(writeClip)
            {
                Animation.Serialize(writer);
            }
        }

        public void Deserialize(IReader reader)
        {
            Type = (MotionDataType)reader.ReadByte();
            bool readClip = reader.ReadBoolean();
            if(readClip)
            {
                Animation = reader.Read<AnimationData>();
            }
        }

        public override string ToString()
        {
            return string.Format("[MotionData: duration={0} loop={1}]", AnimationDuration, Loop);
        }
    }
}
