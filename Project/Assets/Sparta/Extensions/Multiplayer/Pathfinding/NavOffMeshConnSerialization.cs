using System;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavOffMeshConnSerializer : SimpleWriteSerializer<OffMeshConnection>
    {
        public static readonly NavOffMeshConnSerializer Instance = new NavOffMeshConnSerializer();

        public override void Serialize(OffMeshConnection value, IWriter writer)
        {
            NavVector3Serializer.Instance.Serialize(value.Pos0, writer);
            NavVector3Serializer.Instance.Serialize(value.Pos1, writer);
            writer.Write(value.Radius);
            writer.Write(value.Poly);
            writer.Write((byte)value.Flags);
            writer.Write((byte)value.Side);
            //TODO: Serialize OffMeshConnection.Tag if used
        }
    }

    public class NavOffMeshConnParser : SimpleReadParser<OffMeshConnection>
    {
        public static readonly NavOffMeshConnParser Instance = new NavOffMeshConnParser();

        public override OffMeshConnection Parse(IReader reader)
        {
            var offConn = new OffMeshConnection();
            offConn.Pos0 = NavVector3Parser.Instance.Parse(reader);
            offConn.Pos1 = NavVector3Parser.Instance.Parse(reader);
            offConn.Radius = reader.ReadSingle();
            offConn.Poly = reader.ReadInt32();
            offConn.Flags = (OffMeshConnectionFlags)reader.ReadByte();
            offConn.Side = (BoundarySide)reader.ReadByte();
            //TODO: Parse OffMeshConnection.Tag if used
            return offConn;
        }
    }
}


