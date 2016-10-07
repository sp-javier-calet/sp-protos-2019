﻿using System;
using System.Collections;
using SocialPoint.IO;
using SocialPoint.Utils;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public struct Ray : IEquatable<Ray>
    {
        JVector _direction;

        /// <summary>
        /// The origin point of the ray.
        /// </summary>
        public JVector Origin
        {
            get;
            set;
        }

        public int LayerIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Direction is always a normalized vector. If you assign a vector of non unit length, it will be normalized.
        /// </summary>
        public JVector Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                value.Normalize();
                _direction = value;
            }
        }

        public Ray(JVector pOrigin, JVector pDirection, int layerIdx = 0)
        {
            LayerIndex = layerIdx;

            Origin = pOrigin;

            _direction = pDirection;
            Direction = pDirection;
        }

        public override bool Equals(System.Object obj)
        {
            return this == (Ray)obj;
        }

        public bool Equals(Ray v)
        {             
            return this == v;
        }

        public override int GetHashCode()
        {
            var hash = Origin.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Direction.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, LayerIndex.GetHashCode());
            return hash;
        }

        public static bool operator ==(Ray a, Ray b)
        {
            return a.Origin == b.Origin && a.Direction == b.Direction && a.LayerIndex == b.LayerIndex;
        }

        public static bool operator !=(Ray a, Ray b)
        {
            return !(a == b);
        }
    }

    public class RaySerializer : IWriteSerializer<Ray>
    {
        public static readonly RaySerializer Instance = new RaySerializer();

        public void Compare(Ray newObj, Ray oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Origin != oldObj.Origin);
            dirty.Set(newObj.Direction != oldObj.Direction);
            dirty.Set(newObj.LayerIndex != oldObj.LayerIndex);
        }

        public void Serialize(Ray newObj, IWriter writer)
        {
            var vs = JVectorSerializer.Instance;
            vs.Serialize(newObj.Origin, writer);
            vs.Serialize(newObj.Direction, writer);
            writer.Write(newObj.LayerIndex);
        }

        public void Serialize(Ray newObj, Ray oldObj, IWriter writer, Bitset dirty)
        {
            var vs = JVectorSerializer.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Origin, oldObj.Origin, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.Direction, oldObj.Direction, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.LayerIndex);
            }
        }
    }


    public class RayParser : IReadParser<Ray>
    {
        public static readonly RayParser Instance = new RayParser();

        public Ray Parse(IReader reader)
        {
            var vp = JVectorParser.Instance;
            var obj = new Ray();
            obj.Origin = vp.Parse(reader);
            obj.Direction = vp.Parse(reader);
            obj.LayerIndex = reader.ReadInt32();
            return obj;
        }

        public int GetDirtyBitsSize(Ray obj)
        {
            return 2;
        }

        public Ray Parse(Ray obj, IReader reader, Bitset dirty)
        {
            var vp = JVectorParser.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                obj.Origin = vp.Parse(obj.Origin, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Direction = vp.Parse(obj.Direction, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.LayerIndex = reader.ReadInt32();
            }
            return obj;
        }
    }
}
