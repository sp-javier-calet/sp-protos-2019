﻿using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class JVectorSerializer : IWriteSerializer<JVector>
    {
        public static readonly JVectorSerializer Instance = new JVectorSerializer();

        public void Compare(JVector newObj, JVector oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
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

    public class JVectorParser : IReadParser<JVector>
    {
        public static readonly JVectorParser Instance = new JVectorParser();

        public JVector Parse(IReader reader)
        {
            JVector obj;
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
}