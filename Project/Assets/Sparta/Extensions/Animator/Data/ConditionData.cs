using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public enum ConditionDataType : byte
    {
        Invalid = 0,
        If,
        IfNot,
        Greater,
        Less,
        Equals,
        NotEqual
    }

    public class ConditionData : INetworkShareable
    {
        public string Paramater;
        public ConditionDataType Type;
        public float Threshold;

        public void Serialize(IWriter writer)
        {
            writer.Write(Paramater);
            writer.Write((byte)Type);
            writer.Write(Threshold);
        }

        public void Deserialize(IReader reader)
        {
            Paramater = reader.ReadString();
            Type = (ConditionDataType)reader.ReadByte();
            Threshold = reader.ReadSingle();
        }
    }
}
