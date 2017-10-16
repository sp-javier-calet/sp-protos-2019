using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Animations
{
    public enum ParameterDataType : byte
    {
        Invalid = 0,
        Int,
        Float,
        Bool,
        Trigger,
    }

    public class ParameterData : INetworkShareable
    {
        public string Name;
        public ParameterDataType Type;
        public int DefaultInt;
        public float DefaultFloat;
        public bool DefaultBool;

        public void Serialize(IWriter writer)
        {
            writer.Write(Name);
            writer.Write((byte)Type);
            writer.Write(DefaultInt);
            writer.Write(DefaultFloat);
            writer.Write(DefaultBool);
        }

        public void Deserialize(IReader reader)
        {
            Name = reader.ReadString();
            Type = (ParameterDataType)reader.ReadByte();
            DefaultInt = reader.ReadInt32();
            DefaultFloat = reader.ReadSingle();
            DefaultBool = reader.ReadBoolean();
        }
    }
}
