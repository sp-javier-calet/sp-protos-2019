using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public class InstantiateEvent
    {
        public const byte MessageType = 2;

        public int ObjectId;
        public string PrefabName;
        public Transform Transform;
    }

    public class InstantiateEventSerializer : SimpleSerializer<InstantiateEvent>
    {
        TransformSerializer _trans = new TransformSerializer();

        public override void Serialize(InstantiateEvent newObj, IWriter writer)
        {
            writer.Write(newObj.ObjectId);
            writer.Write(newObj.PrefabName);
            _trans.Serialize(newObj.Transform, writer);
        }
    }

    public class InstantiateEventParser : SimpleParser<InstantiateEvent>
    {
        TransformParser _trans = new TransformParser();

        public override InstantiateEvent Parse(IReader reader)
        {
            return new InstantiateEvent {
                ObjectId = reader.ReadInt32(),
                PrefabName = reader.ReadString(),
                Transform = _trans.Parse(reader)
            };
        }
    }

}
