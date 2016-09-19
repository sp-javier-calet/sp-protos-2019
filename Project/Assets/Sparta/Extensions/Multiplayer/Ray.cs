using System;
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
        public JVector origin
        {
            get;
            set;
        }

        /// <summary>
        /// Direction is always a normalized vector. If you assign a vector of non unit length, it will be normalized.
        /// </summary>
        public JVector direction
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

        public Ray(JVector pOrigin, JVector pDirection)
        {
            origin = pOrigin;

            _direction = pDirection;
            direction = pDirection;
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
            var hash = origin.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, direction.GetHashCode());
            return hash;
        }

        public static bool operator ==(Ray a, Ray b)
        {
            return a.origin == b.origin && a.direction == b.direction;
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
            dirty.Set(newObj.origin != oldObj.origin);
            dirty.Set(newObj.direction != oldObj.direction);
        }

        public void Serialize(Ray newObj, IWriter writer)
        {
            var vs = Vector3Serializer.Instance;
            vs.Serialize(newObj.origin, writer);
            vs.Serialize(newObj.direction, writer);
        }

        public void Serialize(Ray newObj, Ray oldObj, IWriter writer, Bitset dirty)
        {
            var vs = Vector3Serializer.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.origin, oldObj.origin, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                vs.Serialize(newObj.direction, oldObj.direction, writer);
            }
        }
    }


    public class RayParser : IReadParser<Ray>
    {
        public static readonly RayParser Instance = new RayParser();

        public Ray Parse(IReader reader)
        {
            var vp = Vector3Parser.Instance;
            var obj = new Ray();
            obj.origin = vp.Parse(reader);
            obj.direction = vp.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Ray obj)
        {
            return 2;
        }

        public Ray Parse(Ray obj, IReader reader, Bitset dirty)
        {
            var vp = Vector3Parser.Instance;
            if(Bitset.NullOrGet(dirty))
            {
                obj.origin = vp.Parse(obj.origin, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.direction = vp.Parse(obj.direction, reader);
            }
            return obj;
        }
    }
}
