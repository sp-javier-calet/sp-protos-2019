using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public static class JitterMatrixExtension
    {
        /// <summary>
        /// Gets or sets the first row in the matrix; that is M11, M12, M13, and M14.
        /// </summary>
        public static JVector Row1(this JMatrix matrix)
        {
            return new JVector(matrix.M11, matrix.M12, matrix.M13);
        }

        /// <summary>
        /// Gets or sets the second row in the matrix; that is M21, M22, M23, and M24.
        /// </summary>
        public static JVector Row2(this JMatrix matrix)
        {
            return new JVector(matrix.M21, matrix.M22, matrix.M23);
        }

        /// <summary>
        /// Gets or sets the third row in the matrix; that is M31, M32, M33, and M34.
        /// </summary>
        public static JVector Row3(this JMatrix matrix)
        {
            return new JVector(matrix.M31, matrix.M32, matrix.M33);
        }

        /// <summary>
        /// Gets or sets the first column in the matrix; that is M11, M21, M31, and M41.
        /// </summary>
        public static JVector Column1(this JMatrix matrix)
        {
            return new JVector(matrix.M11, matrix.M21, matrix.M31);
        }

        /// <summary>
        /// Gets or sets the second column in the matrix; that is M12, M22, M32, and M42.
        /// </summary>
        public static JVector Column2(this JMatrix matrix)
        {
            return new JVector(matrix.M12, matrix.M22, matrix.M32);
        }

        /// <summary>
        /// Gets or sets the third column in the matrix; that is M13, M23, M33, and M43.
        /// </summary>
        public static JVector Column3(this JMatrix matrix)
        {
            return new JVector(matrix.M13, matrix.M23, matrix.M33);
        }
    }

    public class MatrixSerializer : IWriteSerializer<JMatrix>
    {
        public static readonly MatrixSerializer Instance = new MatrixSerializer();

        public void Compare(JMatrix newObj, JMatrix oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Row1() != oldObj.Row1());
            dirty.Set(newObj.Row2() != oldObj.Row2());
            dirty.Set(newObj.Row3() != oldObj.Row3());
            //dirty.Set(newObj.Row4 != oldObj.Row4);
        }

        public void Serialize(JMatrix newObj, IWriter writer)
        {
            //var vs = Vector3Parser.Instance;
            //vs.Serialize(newObj.Row1(), writer);
            //vs.Serialize(newObj.Row2(), writer);
            //vs.Serialize(newObj.Row3(), writer);
            //vs.Serialize(newObj.Ro()w4, writer);
        }

        public void Serialize(JMatrix newObj, JMatrix oldObj, IWriter writer, Bitset dirty)
        {
            //var vs = Vector3Parser.Instance;
            /*if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row1(), oldObj.Row1(), writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row2(), oldObj.Row2(), writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row3(), oldObj.Row3(), writer);
            }*/
            /*if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Row4, oldObj.Row4, writer);
            }*/
        }
    }

    public class MatrixParser : IReadParser<JMatrix>
    {
        public static readonly MatrixParser Instance = new MatrixParser();

        public JMatrix Parse(IReader reader)
        {
            //var vp = Vector3Parser.Instance;
            var obj = new JMatrix();
            /*obj.Row1 = vp.Parse(reader);
            obj.Row2 = vp.Parse(reader);
            obj.Row3 = vp.Parse(reader);*/
            //obj.Row4 = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(JMatrix obj)
        {
            return 4;
        }

        public JMatrix Parse(JMatrix obj, IReader reader, Bitset dirty)
        {
            //var vp = Vector3Parser.Instance;
            /*if(Bitset.NullOrGet(dirty))
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
            }*/
            /*if(Bitset.NullOrGet(dirty))
            {
                obj.Row4 = vp.Parse(obj.Row4, reader);
            }*/
            return obj;
        }
    }
}