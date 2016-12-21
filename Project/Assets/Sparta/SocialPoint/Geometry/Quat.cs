using System;
using SocialPoint.Utils;

namespace SocialPoint.Geometry
{
    public partial struct Quat
    {
        #region Static readonly variables

        /// <summary>
        /// A quaternion with components (0,0,0,1);
        /// </summary>
        public static readonly Quat Identity;

        static Quat()
        {
            Identity = new Quat(0.0f, 0.0f, 0.0f, 1.0f);
        }

        #endregion

        public float X;
        public float Y;
        public float Z;
        public float W;

        public Quat(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override int GetHashCode()
        {
            var hash = X.GetHashCode();
            CryptographyUtils.HashCombine(hash, Y.GetHashCode());
            CryptographyUtils.HashCombine(hash, Z.GetHashCode());
            CryptographyUtils.HashCombine(hash, W.GetHashCode());
            return hash;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Quat))
            {
                return false;
            }

            var other = (Quat)obj;
            return (Math.Abs(X - other.X) <= float.Epsilon) && (Math.Abs(Y - other.Y) <= float.Epsilon) && (Math.Abs(Z - other.Z) <= float.Epsilon) && (Math.Abs(W - other.W) <= float.Epsilon);
        }

        public static bool operator ==(Quat value1, Quat value2)
        {
            return (Math.Abs(value1.X - value2.X) <= float.Epsilon) && (Math.Abs(value1.Y - value2.Y) <= float.Epsilon) && (Math.Abs(value1.Z - value2.Z) <= float.Epsilon) && (Math.Abs(value1.W - value2.W) <= float.Epsilon);
        }

        public static bool operator !=(Quat value1, Quat value2)
        {
            return (Math.Abs(value1.X - value2.X) > float.Epsilon) || (Math.Abs(value1.Y - value2.Y) > float.Epsilon) || (Math.Abs(value1.Z - value2.Z) > float.Epsilon) || (Math.Abs(value1.W - value2.W) > float.Epsilon);
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", X, Y, Z, W);
        }
    }

    // Unity Quaternion adapter
    public partial struct Quat
    {
        public static Quat Convert(UnityEngine.Quaternion q)
        {
            return new Quat(q.x, q.y, q.z, q.w);
        }

        public static implicit operator Quat(UnityEngine.Quaternion q)
        {
            return new Quat(q.x, q.y, q.z, q.w);
        }

        public static implicit operator UnityEngine.Quaternion(Quat q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}