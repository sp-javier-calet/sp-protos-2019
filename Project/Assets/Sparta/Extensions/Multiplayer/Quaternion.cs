﻿using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float v=0.0f)
        {
            x = v;
            y = v;
            z = v;
            w = v;
        }

        public Quaternion(float px, float py, float pz, float pw)
        {
            x = px;
            y = py;
            z = pz;
            w = pw;
        }

        public override bool Equals(System.Object obj)
        {
            return this == (Quaternion)obj;
        }

        public bool Equals(Quaternion v)
        {             
            return this == v;
        }

        public override int GetHashCode()
        {
            var hash = x.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, y.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, z.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, w.GetHashCode());
            return hash;
        }

        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return !(a == b);
        }            

        public static Quaternion Identity
        {
            get
            {
                return new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", x, y, z ,w);
        }
    }

    public class QuaternionSerializer : IWriteSerializer<Quaternion>
    {
        public static readonly QuaternionSerializer Instance = new QuaternionSerializer();

        public void Compare(Quaternion newObj, Quaternion oldObj, Bitset dirty)
        {
            dirty.Set(newObj.x != oldObj.x);
            dirty.Set(newObj.y != oldObj.y);
            dirty.Set(newObj.z != oldObj.z);
            dirty.Set(newObj.w != oldObj.w);
        }

        public void Serialize(Quaternion newObj, IWriter writer)
        {
            writer.Write(newObj.x);
            writer.Write(newObj.y);
            writer.Write(newObj.z);
            writer.Write(newObj.w);
        }

        public void Serialize(Quaternion newObj, Quaternion oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.x);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.y);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.z);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.w);
            }
        }
    }

    public class QuaternionParser : IReadParser<Quaternion>
    {
        public static readonly QuaternionParser Instance = new QuaternionParser();

        public Quaternion Parse(IReader reader)
        {
            Quaternion obj;
            obj.x = reader.ReadSingle();
            obj.y = reader.ReadSingle();
            obj.z = reader.ReadSingle();
            obj.w = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Quaternion obj)
        {
            return 4;
        }

        public Quaternion Parse(Quaternion obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.x = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.y = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.z = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.w = reader.ReadSingle();
            }
            return obj;
        }
    }
}