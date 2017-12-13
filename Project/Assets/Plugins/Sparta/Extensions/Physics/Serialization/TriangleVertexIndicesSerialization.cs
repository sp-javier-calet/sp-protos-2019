using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using Jitter.Collision;

namespace SocialPoint.Physics
{
    public class TriangleVertexIndicesSerializer : IDiffWriteSerializer<TriangleVertexIndices>
    {
        public static readonly TriangleVertexIndicesSerializer Instance = new TriangleVertexIndicesSerializer();

        public void Compare(TriangleVertexIndices newObj, TriangleVertexIndices oldObj, Bitset dirty)
        {
            dirty.Set(newObj.I0 != oldObj.I0);
            dirty.Set(newObj.I1 != oldObj.I1);
            dirty.Set(newObj.I2 != oldObj.I2);
        }

        public void Serialize(TriangleVertexIndices newObj, IWriter writer)
        {
            writer.Write(newObj.I0);
            writer.Write(newObj.I1);
            writer.Write(newObj.I2);
        }

        public void Serialize(TriangleVertexIndices newObj, TriangleVertexIndices oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.I0);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.I1);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.I2);
            }
        }
    }

    public class TriangleVertexIndicesParser : IDiffReadParser<TriangleVertexIndices>
    {
        public static readonly TriangleVertexIndicesParser Instance = new TriangleVertexIndicesParser();

        public TriangleVertexIndices Parse(IReader reader)
        {
            return new TriangleVertexIndices(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }

        public int GetDirtyBitsSize(TriangleVertexIndices obj)
        {
            return 3;
        }

        public TriangleVertexIndices Parse(TriangleVertexIndices obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.I0 = reader.ReadInt32();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.I1 = reader.ReadInt32();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.I2 = reader.ReadInt32();
            }
            return obj;
        }
    }
}
