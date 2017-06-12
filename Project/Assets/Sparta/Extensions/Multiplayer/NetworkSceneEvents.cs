using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public static class SceneMsgType
    {
        public const byte ConnectEvent = 0;
        public const byte UpdateSceneEvent = 1;
        public const byte UpdateSceneAckEvent = 2;
        public const byte Highest = 3;
    }

    public struct ConnectEvent : INetworkShareable
    {
        public float Timestamp;

        public void Deserialize(IReader reader)
        {
            Timestamp = reader.ReadSingle();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Timestamp);
        }
    }

    public struct UpdateSceneEvent : INetworkShareable
    {
        public float Timestamp;
        public int LastAction;

        public void Deserialize(IReader reader)
        {
            Timestamp = reader.ReadSingle();
            LastAction = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Timestamp);
            writer.Write(LastAction);
        }
    }

    public struct UpdateSceneAckEvent : INetworkShareable
    {
        public float Timestamp;

        public void Deserialize(IReader reader)
        {
            Timestamp = reader.ReadSingle();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Timestamp);
        }
    }
}