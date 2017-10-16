using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public class StateData : INetworkShareable
    {
        public int NameHash;
        public string Name;
        public float Speed;
        public string SpeedParameter;
        public bool SpeedParameterActive;
        public MotionData Motion;
        public TransitionData[] Transitions;

        public bool Loop
        {
            get
            {
                if(Motion == null)
                {
                    return false;
                }
                return Motion.Loop;
            }
        }

        public float AnimationDuration
        {
            get
            {
                if(Motion == null)
                {
                    return 0.0f;
                }
                return Motion.AnimationDuration;
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(NameHash);
            writer.Write(Name);
            writer.Write(Speed);
            writer.Write(SpeedParameter);
            writer.Write(SpeedParameterActive);
            Motion.Serialize(writer);
            writer.WriteArray<TransitionData>(Transitions);
        }

        public void Deserialize(IReader reader)
        {
            NameHash = reader.ReadInt32();
            Name = reader.ReadString();
            Speed = reader.ReadSingle();
            SpeedParameter = reader.ReadString();
            SpeedParameterActive = reader.ReadBoolean();
            Motion = reader.Read<MotionData>();
            Transitions = reader.ReadArray<TransitionData>();
        }

        public override string ToString()
        {
            return string.Format("[StateData \"{0}\" speed={1} motion={2} SpeedParameter={3} SpeedParameterActive={4}]", Name, Speed, Motion, SpeedParameter, SpeedParameterActive);
        }
    }
}