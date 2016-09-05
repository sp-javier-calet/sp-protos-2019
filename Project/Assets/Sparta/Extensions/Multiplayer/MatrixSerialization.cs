using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class MatrixSerializer : IWriteSerializer<Matrix>
    {
        public static readonly MatrixSerializer Instance = new MatrixSerializer();

        public void Compare(Matrix newObj, Matrix oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Row1 != oldObj.Row1);
            dirty.Set(newObj.Row2 != oldObj.Row2);
            dirty.Set(newObj.Row3 != oldObj.Row3);
            dirty.Set(newObj.Row4 != oldObj.Row4);
        }

        public void Serialize(Matrix newObj, IWriter writer)
        {
            var vs = Vector4Serializer.Instance;
            vs.Serialize(newObj.Row1, writer);
            vs.Serialize(newObj.Row2, writer);
            vs.Serialize(newObj.Row3, writer);
            vs.Serialize(newObj.Row4, writer);
        }

        public void Serialize(Matrix newObj, Matrix oldObj, IWriter writer, Bitset dirty)
        {
            var vs = Vector4Serializer.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row1, oldObj.Row1, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row2, oldObj.Row2, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row3, oldObj.Row3, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row4, oldObj.Row4, writer);
            }
        }
    }

    public class MatrixParser : IReadParser<Matrix>
    {
        public static readonly MatrixParser Instance = new MatrixParser();

        public Matrix Parse(IReader reader)
        {
            var vp = Vector4Parser.Instance;
            var obj = new Matrix();
            obj.Row1 = vp.Parse(reader);
            obj.Row2 = vp.Parse(reader);
            obj.Row3 = vp.Parse(reader);
            obj.Row4 = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Matrix obj)
        {
            return 4;
        }

        public Matrix Parse(Matrix obj, IReader reader, Bitset dirty)
        {
            var vp = Vector4Parser.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                obj.Row1 = vp.Parse(obj.Row1, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Row2 = vp.Parse(obj.Row2, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Row3 = vp.Parse(obj.Row3, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Row4 = vp.Parse(obj.Row4, reader);
            }
            return obj;
        }
    }
}