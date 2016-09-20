using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class MatrixSerializer : IWriteSerializer<JMatrix>
    {
        public static readonly MatrixSerializer Instance = new MatrixSerializer();

        public void Compare(JMatrix newObj, JMatrix oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Row1 != oldObj.Row1);
            dirty.Set(newObj.Row2 != oldObj.Row2);
            dirty.Set(newObj.Row3 != oldObj.Row3);
        }

        public void Serialize(JMatrix newObj, IWriter writer)
        {
            var vs = Vector3Serializer.Instance;
            vs.Serialize(newObj.Row1, writer);
            vs.Serialize(newObj.Row2, writer);
            vs.Serialize(newObj.Row3, writer);
        }

        public void Serialize(JMatrix newObj, JMatrix oldObj, IWriter writer, Bitset dirty)
        {
            var vs = Vector3Serializer.Instance;
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
        }
    }

    public class MatrixParser : IReadParser<JMatrix>
    {
        public static readonly MatrixParser Instance = new MatrixParser();

        public JMatrix Parse(IReader reader)
        {
            var vp = Vector3Parser.Instance;
            var obj = new JMatrix();
            obj.Row1 = vp.Parse(reader);
            obj.Row2 = vp.Parse(reader);
            obj.Row3 = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(JMatrix obj)
        {
            return 4;
        }

        public JMatrix Parse(JMatrix obj, IReader reader, Bitset dirty)
        {
            var vp = Vector3Parser.Instance;
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
            return obj;
        }
    }
}