﻿using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Matrix4x4 : IEquatable<Matrix4x4>
    {
        Vector4 _0;
        Vector4 _1;
        Vector4 _2;
        Vector4 _3;

        public Matrix4x4(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3)
        {
            _0 = v0;
            _1 = v1;
            _2 = v2;
            _3 = v3;
        }

        public override bool Equals(System.Object obj)
        {
            return this == (Matrix4x4)obj;
        }

        public bool Equals(Matrix4x4 m)
        {             
            return this == m;
        }

        public Vector4 this[int key]
        {
            get
            {
                switch(key)
                {
                case 0:
                    return _0;
                case 1:
                    return _1;
                case 2:
                    return _2;
                case 3:
                    return _3;
                }
                throw new InvalidOperationException("Index out of range.");
            }
            set
            {
                switch(key)
                {
                case 0:
                    _0 = value;
                    break;
                case 1:
                    _1 = value;
                    break;
                case 2:
                    _2 = value;
                    break;
                case 3:
                    _3 = value;
                    break;
                default:
                    throw new InvalidOperationException("Index out of range.");
                }
            }
        }

        public override int GetHashCode()
        {
            var hash = _0.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, _1.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, _2.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, _3.GetHashCode());
            return hash;
        }

        public static bool operator ==(Matrix4x4 a, Matrix4x4 b)
        {
            return a._0 == b._0 && a._1 == b._1 && a._2 == b._2 && a._3 == b._3;
        }

        public static bool operator !=(Matrix4x4 a, Matrix4x4 b)
        {
            return !(a == b);
        } 

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", _0, _1, _2, _3);
        }
    }

    public class Matrix4x4Serializer : IWriteSerializer<Matrix4x4>
    {
        public static readonly Matrix4x4Serializer Instance = new Matrix4x4Serializer();

        public void Compare(Matrix4x4 newObj, Matrix4x4 oldObj, Bitset dirty)
        {
            dirty.Set(newObj[0] != oldObj[0]);
            dirty.Set(newObj[1] != oldObj[1]);
            dirty.Set(newObj[2] != oldObj[2]);
            dirty.Set(newObj[3] != oldObj[3]);
        }

        public void Serialize(Matrix4x4 newObj, IWriter writer)
        {
            var vs = Vector4Serializer.Instance;
            vs.Serialize(newObj[0], writer);
            vs.Serialize(newObj[1], writer);
            vs.Serialize(newObj[2], writer);
            vs.Serialize(newObj[3], writer);
        }

        public void Serialize(Matrix4x4 newObj, Matrix4x4 oldObj, IWriter writer, Bitset dirty)
        {
            var vs = Vector4Serializer.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj[0], oldObj[0], writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj[1], oldObj[1], writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj[2], oldObj[2], writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj[3], oldObj[3], writer);
            }
        }
    }

    public class Matrix4x4Parser : IReadParser<Matrix4x4>
    {
        public static readonly Matrix4x4Parser Instance = new Matrix4x4Parser();

        public Matrix4x4 Parse(IReader reader)
        {
            var vp = Vector4Parser.Instance;
            var obj = new Matrix4x4();
            obj[0] = vp.Parse(reader);
            obj[1] = vp.Parse(reader);
            obj[2] = vp.Parse(reader);
            obj[3] = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Matrix4x4 obj)
        {
            return 4;
        }

        public Matrix4x4 Parse(Matrix4x4 obj, IReader reader, Bitset dirty)
        {
            var vp = Vector4Parser.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                obj[0] = vp.Parse(obj[0], reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj[1] = vp.Parse(obj[1], reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj[2] = vp.Parse(obj[2], reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj[3] = vp.Parse(obj[3], reader);
            }
            return obj;
        }
    }
}