using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public class InstantiateNetworkGameObjectEvent
    {
        public const byte MessageType = 2;

        public int ObjectId;
        public string PrefabName;
        public Transform Transform;
    }

    public class InstantiateNetworkGameObjectEventSerializer : SimpleSerializer<InstantiateNetworkGameObjectEvent>
    {
        TransformSerializer _trans = new TransformSerializer();

        public override void Serialize(InstantiateNetworkGameObjectEvent newObj, IWriter writer)
        {
            writer.Write(newObj.ObjectId);
            writer.Write(newObj.PrefabName);
            _trans.Serialize(newObj.Transform, writer);
        }
    }

    public class InstantiateNetworkGameObjectEventParser : SimpleParser<InstantiateNetworkGameObjectEvent>
    {
        TransformParser _trans = new TransformParser();

        public override InstantiateNetworkGameObjectEvent Parse(IReader reader)
        {
            return new InstantiateNetworkGameObjectEvent {
                ObjectId = reader.ReadInt32(),
                PrefabName = reader.ReadString(),
                Transform = _trans.Parse(reader)
            };
        }
    }

}
