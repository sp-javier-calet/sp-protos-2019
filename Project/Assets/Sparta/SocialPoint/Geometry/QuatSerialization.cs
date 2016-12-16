using SocialPoint.IO;

namespace SocialPoint.Geometry
{
    public class QuatSerializer : IDiffWriteSerializer<Quat>
    {
        public static readonly QuatSerializer Instance = new QuatSerializer();

        QuatSerializer()
        {
        }

        public void Compare(Quat newObj, Quat oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
            dirty.Set(newObj.W != oldObj.W);
        }

        public void Serialize(Quat newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
            writer.Write(newObj.W);
        }

        public void Serialize(Quat newObj, Quat oldObj, IWriter writer, Bitset dirty)
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
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.W);
            }
        }
    }

    public class QuatParser : IDiffReadParser<Quat>
    {
        public static readonly QuatParser Instance = new QuatParser();

        QuatParser()
        {
        }

        public Quat Parse(IReader reader)
        {
            Quat obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.W = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Quat obj)
        {
            return 4;
        }

        public Quat Parse(Quat obj, IReader reader, Bitset dirty)
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
            if(Bitset.NullOrGet(dirty))
            {
                obj.W = reader.ReadSingle();
            }
            return obj;
        }
    }
}
