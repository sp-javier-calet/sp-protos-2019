using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Physics
{
    public class JMatrixSerializer : IDiffWriteSerializer<JMatrix>
    {
        public static readonly JMatrixSerializer Instance = new JMatrixSerializer();

        public void Compare(JMatrix newObj, JMatrix oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Row1 != oldObj.Row1);
            dirty.Set(newObj.Row2 != oldObj.Row2);
            dirty.Set(newObj.Row3 != oldObj.Row3);
        }

        public void Serialize(JMatrix newObj, IWriter writer)
        {
            var vs = JVectorSerializer.Instance;
            vs.Serialize(newObj.Row1, writer);
            vs.Serialize(newObj.Row2, writer);
            vs.Serialize(newObj.Row3, writer);
        }

        public void Serialize(JMatrix newObj, JMatrix oldObj, IWriter writer, Bitset dirty)
        {
            var vs = JVectorSerializer.Instance;
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

    public class JMatrixParser : IDiffReadParser<JMatrix>
    {
        public static readonly JMatrixParser Instance = new JMatrixParser();

        public JMatrix Parse(IReader reader)
        {
            var vp = JVectorParser.Instance;
            var obj = new JMatrix();
            obj.Row1 = vp.Parse(reader);
            obj.Row2 = vp.Parse(reader);
            obj.Row3 = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(JMatrix obj)
        {
            return 3;
        }

        public JMatrix Parse(JMatrix obj, IReader reader, Bitset dirty)
        {
            var vp = JVectorParser.Instance;
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
