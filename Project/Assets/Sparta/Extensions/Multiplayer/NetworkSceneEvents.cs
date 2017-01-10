using SocialPoint.IO;
using SocialPoint.Geometry;

namespace SocialPoint.Multiplayer
{
    public static class SceneMsgType
    {
        public const byte UpdateSceneEvent = 1;
        public const byte InstantiateObjectEvent = 2;
        public const byte DestroyObjectEvent = 3;
        public const byte Highest = 3;
    }

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

    public class DestroyNetworkGameObjectEvent : INetworkShareable
    {
        public int ObjectId;

        public void Deserialize(IReader reader)
        {
            ObjectId = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ObjectId);
        }
    }
}
