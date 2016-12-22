using System;
using SocialPoint.IO;

namespace SocialPoint.Geometry
{
    public class VectorSerializer : IDiffWriteSerializer<Vector>
    {
        public static readonly VectorSerializer Instance = new VectorSerializer();

        VectorSerializer()
        {
        }

        public void Compare(Vector newObj, Vector oldObj, Bitset dirty)
        {
            dirty.Set(Math.Abs(newObj.X - oldObj.X) > float.Epsilon);
            dirty.Set(Math.Abs(newObj.Y - oldObj.Y) > float.Epsilon);
            dirty.Set(Math.Abs(newObj.Z - oldObj.Z) > float.Epsilon);
        }

        public void Serialize(Vector newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
        }

        public void Serialize(Vector newObj, Vector oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.X);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.Y);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.Z);
            }
        }
    }

    public class VectorParser : IDiffReadParser<Vector>
    {
        public static readonly VectorParser Instance = new VectorParser();

        VectorParser()
        {
        }

        public Vector Parse(IReader reader)
        {
            Vector obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Vector obj)
        {
            return 3;
        }

        public Vector Parse(Vector obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.X = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Y = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Z = reader.ReadSingle();
            }
            return obj;
        }
    }
}
