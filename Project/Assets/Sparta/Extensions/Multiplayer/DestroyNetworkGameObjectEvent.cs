using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
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
