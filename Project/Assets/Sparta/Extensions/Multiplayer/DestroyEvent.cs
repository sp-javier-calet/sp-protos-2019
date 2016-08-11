using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public class DestroyEvent
    {
        public const byte MessageType = 3;

        public int ObjectId;
    }

    public class DestroyEventSerializer : SimpleSerializer<DestroyEvent>
    {
        public override void Serialize(DestroyEvent newObj, IWriter writer)
        {
            writer.Write(newObj.ObjectId);
  
        }
    }

    public class DestroyEventParser : SimpleParser<DestroyEvent>
    {
        public override DestroyEvent Parse(IReader reader)
        {
            return new DestroyEvent {
                ObjectId = reader.ReadInt32()
            };
        }
    }

}
