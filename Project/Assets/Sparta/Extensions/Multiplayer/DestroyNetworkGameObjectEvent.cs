using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public class DestroyNetworkGameObjectEvent
    {
        public const byte MessageType = 3;

        public int ObjectId;
    }

    public class DestroyNetworkGameObjectEventSerializer : SimpleSerializer<DestroyNetworkGameObjectEvent>
    {
        public override void Serialize(DestroyNetworkGameObjectEvent newObj, IWriter writer)
        {
            writer.Write(newObj.ObjectId);
  
        }
    }

    public class DestroyNetworkGameObjectEventParser : SimpleParser<DestroyNetworkGameObjectEvent>
    {
        public override DestroyNetworkGameObjectEvent Parse(IReader reader)
        {
            return new DestroyNetworkGameObjectEvent {
                ObjectId = reader.ReadInt32()
            };
        }
    }

}
