using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Physics
{
    public class JVectorSerializer : IDiffWriteSerializer<JVector>
    {
        const float _epsilon = 1e-5f;
        public static readonly JVectorSerializer Instance = new JVectorSerializer();

        public void Compare(JVector newObj, JVector oldObj, Bitset dirty)
        {
            dirty.Set(Math.Abs(newObj.X - oldObj.X) > _epsilon);
            dirty.Set(Math.Abs(newObj.Y - oldObj.Y) > _epsilon);
            dirty.Set(Math.Abs(newObj.Z - oldObj.Z) > _epsilon);
        }

        public void Serialize(JVector newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
        }

        public void Serialize(JVector newObj, JVector oldObj, IWriter writer, Bitset dirty)
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

    public class JVectorParser : IDiffReadParser<JVector>
    {
        public static readonly JVectorParser Instance = new JVectorParser();

        public JVector Parse(IReader reader)
        {
            JVector obj = new JVector();
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(JVector obj)
        {
            return 3;
        }

        public JVector Parse(JVector obj, IReader reader, Bitset dirty)
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

    public class JVectorShortSerializer : IDiffWriteSerializer<JVector>
    {
        const float _epsilon = 1e-5f;
        public static readonly JVectorShortSerializer Instance = new JVectorShortSerializer();

        public void Compare(JVector newObj, JVector oldObj, Bitset dirty)
        {
            dirty.Set(Math.Abs(newObj.X - oldObj.X) > _epsilon);
            dirty.Set(Math.Abs(newObj.Y - oldObj.Y) > _epsilon);
            dirty.Set(Math.Abs(newObj.Z - oldObj.Z) > _epsilon);
        }

        public void Serialize(JVector newObj, IWriter writer)
        {
            writer.WriteShortFloat(newObj.X);
            writer.WriteShortFloat(newObj.Y);
            writer.WriteShortFloat(newObj.Z);
        }

        public void Serialize(JVector newObj, JVector oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.WriteShortFloat(newObj.X);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.WriteShortFloat(newObj.Y);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.WriteShortFloat(newObj.Z);
            }
        }
    }

    public class JVectorShortParser : IDiffReadParser<JVector>
    {
        public static readonly JVectorShortParser Instance = new JVectorShortParser();

        public JVector Parse(IReader reader)
        {
            JVector obj = new JVector();
            obj.X = reader.ReadShortFloat();
            obj.Y = reader.ReadShortFloat();
            obj.Z = reader.ReadShortFloat();
            return obj;
        }

        public int GetDirtyBitsSize(JVector obj)
        {
            return 3;
        }

        public JVector Parse(JVector obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.X = reader.ReadShortFloat();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Y = reader.ReadShortFloat();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Z = reader.ReadShortFloat();
            }
            return obj;
        }
    }
}
