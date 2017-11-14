using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Physics
{
    public class JQuaternionSerializer : IDiffWriteSerializer<JQuaternion>
    {
        public static readonly JQuaternionSerializer Instance = new JQuaternionSerializer();
        const float _epsilon = 1e-5f;

        public void Compare(JQuaternion newObj, JQuaternion oldObj, Bitset dirty)
        {
            dirty.Set(Math.Abs(newObj.X - oldObj.X) > _epsilon);
            dirty.Set(Math.Abs(newObj.Y - oldObj.Y) > _epsilon);
            dirty.Set(Math.Abs(newObj.Z - oldObj.Z) > _epsilon);
            dirty.Set(Math.Abs(newObj.W - oldObj.W) > _epsilon);
        }

        public void Serialize(JQuaternion newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
            writer.Write(newObj.W);
        }

        public void Serialize(JQuaternion newObj, JQuaternion oldObj, IWriter writer, Bitset dirty)
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

    public class JQuaternionParser : IDiffReadParser<JQuaternion>
    {
        public static readonly JQuaternionParser Instance = new JQuaternionParser();

        public JQuaternion Parse(IReader reader)
        {
            JQuaternion obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.W = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(JQuaternion obj)
        {
            return 4;
        }

        public JQuaternion Parse(JQuaternion obj, IReader reader, Bitset dirty)
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

    public class JQuaternionShortSerializer : IDiffWriteSerializer<JQuaternion>
    {
        public static readonly JQuaternionShortSerializer Instance = new JQuaternionShortSerializer();
        const float _epsilon = 1e-5f;

        public void Compare(JQuaternion newObj, JQuaternion oldObj, Bitset dirty)
        {
            dirty.Set(Math.Abs(newObj.X - oldObj.X) > _epsilon);
            dirty.Set(Math.Abs(newObj.Y - oldObj.Y) > _epsilon);
            dirty.Set(Math.Abs(newObj.Z - oldObj.Z) > _epsilon);
            dirty.Set(Math.Abs(newObj.W - oldObj.W) > _epsilon);
        }

        public void Serialize(JQuaternion newObj, IWriter writer)
        {
            writer.WriteShortFloat(newObj.X);
            writer.WriteShortFloat(newObj.Y);
            writer.WriteShortFloat(newObj.Z);
            writer.WriteShortFloat(newObj.W);
        }

        public void Serialize(JQuaternion newObj, JQuaternion oldObj, IWriter writer, Bitset dirty)
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
            if(Bitset.NullOrGet(dirty))
            {
                writer.WriteShortFloat(newObj.W);
            }
        }
    }

    public class JQuaternionShortParser : IDiffReadParser<JQuaternion>
    {
        public static readonly JQuaternionShortParser Instance = new JQuaternionShortParser();

        public JQuaternion Parse(IReader reader)
        {
            JQuaternion obj;
            obj.X = reader.ReadShortFloat();
            obj.Y = reader.ReadShortFloat();
            obj.Z = reader.ReadShortFloat();
            obj.W = reader.ReadShortFloat();
            return obj;
        }

        public int GetDirtyBitsSize(JQuaternion obj)
        {
            return 4;
        }

        public JQuaternion Parse(JQuaternion obj, IReader reader, Bitset dirty)
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
            if(Bitset.NullOrGet(dirty))
            {
                obj.W = reader.ReadShortFloat();
            }
            return obj;
        }
    }
}
