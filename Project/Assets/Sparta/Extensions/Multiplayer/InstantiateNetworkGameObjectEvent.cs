using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public class InstantiateNetworkGameObjectEvent : INetworkShareable
    {
        public int ObjectId;
        public string PrefabName;
        public Transform Transform;

        public void Deserialize(IReader reader)
        {
            ObjectId = reader.ReadInt32();
            PrefabName = reader.ReadString();
            Transform = TransformParser.Instance.Parse(reader);
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ObjectId);
            writer.Write(PrefabName);
            TransformSerializer.Instance.Serialize(Transform, writer);
        }
    }
}
