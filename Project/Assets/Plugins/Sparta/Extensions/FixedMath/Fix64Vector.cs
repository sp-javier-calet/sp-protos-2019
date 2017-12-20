using System;
using FixMath.NET;

namespace SocialPoint.FixedMath
{
    [Serializable]
    public struct Fix64Vector
    {
        private static Fix64 ZeroEpsilonSq = Fix64Math.Epsilon;
        public static readonly Fix64Vector One = new Fix64Vector(1, 1, 1);
        public static readonly Fix64Vector Zero = new Fix64Vector(0, 0, 0);
        public static readonly Fix64Vector Left = new Fix64Vector(-1, 0, 0);
        public static readonly Fix64Vector Right = new Fix64Vector(1, 0, 0);
        public static readonly Fix64Vector Up = new Fix64Vector(0, 1, 0);
        public static readonly Fix64Vector Down = new Fix64Vector(0, -1, 0);
        public static readonly Fix64Vector Back = new Fix64Vector(0, 0, -1);
        public static readonly Fix64Vector Forward = new Fix64Vector(0, 0, 1);
        public static readonly Fix64Vector MinValue = new Fix64Vector(Fix64.MinValue);
        public static readonly Fix64Vector MaxValue = new Fix64Vector(Fix64.MaxValue);
        internal static Fix64Vector Arbitrary = new Fix64Vector(1, 1, 1);
        internal static Fix64Vector InternalZero = Fix64Vector.Zero;
        public Fix64 x;
        public Fix64 y;
        public Fix64 z;

        public Fix64 SqrMagnitude
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public Fix64 Magnitude
        {
            get
            {
                return Fix64.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
        }

        public Fix64Vector Normalized
        {
            get
            {
                Fix64Vector tsVector = new Fix64Vector(this.x, this.y, this.z);
                tsVector.Normalize();
                return tsVector;
            }
        }

        public Fix64Vector(int x, int y, int z)
        {
            this.x = (Fix64) x;
            this.y = (Fix64) y;
            this.z = (Fix64) z;
        }

        public Fix64Vector(Fix64 x, Fix64 y, Fix64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Fix64Vector(Fix64 xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
        }

        public static bool operator ==(Fix64Vector value1, Fix64Vector value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z;
        }

        public static bool operator !=(Fix64Vector value1, Fix64Vector value2)
        {
            if (value1.x == value2.x && value1.y == value2.y)
                return value1.z != value2.z;
            return true;
        }

        public static Fix64Vector operator %(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Cross(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64 operator *(Fix64Vector value1, Fix64Vector value2)
        {
            return Fix64Vector.Dot(ref value1, ref value2);
        }

        public static Fix64Vector operator *(Fix64Vector value1, Fix64 value2)
        {
            Fix64Vector result;
            Fix64Vector.Multiply(ref value1, value2, out result);
            return result;
        }

        public static Fix64Vector operator *(Fix64 value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Multiply(ref value2, value1, out result);
            return result;
        }

        public static Fix64Vector operator -(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Vector operator +(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Add(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Vector operator /(Fix64Vector value1, Fix64 value2)
        {
            Fix64Vector result;
            Fix64Vector.Divide(ref value1, value2, out result);
            return result;
        }

        public void Scale(Fix64Vector other)
        {
            this.x = this.x * other.x;
            this.y = this.y * other.y;
            this.z = this.z * other.z;
        }

        public void Set(Fix64 x, Fix64 y, Fix64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Fix64Vector Lerp(Fix64Vector from, Fix64Vector to, Fix64 percent)
        {
            return from + (to - from) * percent;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", this.x.ToString(), this.y.ToString(), this.z.ToString());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fix64Vector))
                return false;
            Fix64Vector tsVector = (Fix64Vector) obj;
            return this.x == tsVector.x && this.y == tsVector.y && this.z == tsVector.z;
        }

        public static Fix64Vector Scale(Fix64Vector vecA, Fix64Vector vecB)
        {
            Fix64Vector tsVector;
            tsVector.x = vecA.x * vecB.x;
            tsVector.y = vecA.y * vecB.y;
            tsVector.z = vecA.z * vecB.z;
            return tsVector;
        }

        public static Fix64Vector Min(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Min(ref value1, ref value2, out result);
            return result;
        }

        public static void Min(ref Fix64Vector value1, ref Fix64Vector value2, out Fix64Vector result)
        {
            result.x = value1.x < value2.x ? value1.x : value2.x;
            result.y = value1.y < value2.y ? value1.y : value2.y;
            result.z = value1.z < value2.z ? value1.z : value2.z;
        }

        public static Fix64Vector Max(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Max(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64 Distance(Fix64Vector v1, Fix64Vector v2)
        {
            return Fix64.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z));
        }

        public static void Max(ref Fix64Vector value1, ref Fix64Vector value2, out Fix64Vector result)
        {
            result.x = value1.x > value2.x ? value1.x : value2.x;
            result.y = value1.y > value2.y ? value1.y : value2.y;
            result.z = value1.z > value2.z ? value1.z : value2.z;
        }

        public void MakeZero()
        {
            this.x = Fix64.Zero;
            this.y = Fix64.Zero;
            this.z = Fix64.Zero;
        }

        public bool IsZero()
        {
            return this.SqrMagnitude == Fix64.Zero;
        }

        public bool IsNearlyZero()
        {
            return this.SqrMagnitude < Fix64Vector.ZeroEpsilonSq;
        }

        public static Fix64Vector Transform(Fix64Vector position, Fix64Matrix matrix)
        {
            Fix64Vector result;
            Fix64Vector.Transform(ref position, ref matrix, out result);
            return result;
        }

        public static void Transform(ref Fix64Vector position, ref Fix64Matrix matrix, out Fix64Vector result)
        {
            Fix64 fp1 = position.x * matrix.M11 + position.y * matrix.M21 + position.z * matrix.M31;
            Fix64 fp2 = position.x * matrix.M12 + position.y * matrix.M22 + position.z * matrix.M32;
            Fix64 fp3 = position.x * matrix.M13 + position.y * matrix.M23 + position.z * matrix.M33;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public static void TransposedTransform(ref Fix64Vector position, ref Fix64Matrix matrix, out Fix64Vector result)
        {
            Fix64 fp1 = position.x * matrix.M11 + position.y * matrix.M12 + position.z * matrix.M31;
            Fix64 fp2 = position.x * matrix.M21 + position.y * matrix.M22 + position.z * matrix.M23;
            Fix64 fp3 = position.x * matrix.M31 + position.y * matrix.M32 + position.z * matrix.M33;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public static Fix64 Dot(Fix64Vector vector1, Fix64Vector vector2)
        {
            return Fix64Vector.Dot(ref vector1, ref vector2);
        }

        public static Fix64 Dot(ref Fix64Vector vector1, ref Fix64Vector vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
        }

        public static Fix64Vector Add(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Add(ref value1, ref value2, out result);
            return result;
        }

        public static void Add(ref Fix64Vector value1, ref Fix64Vector value2, out Fix64Vector result)
        {
            Fix64 fp1 = value1.x + value2.x;
            Fix64 fp2 = value1.y + value2.y;
            Fix64 fp3 = value1.z + value2.z;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public static Fix64Vector Divide(Fix64Vector value1, Fix64 scaleFactor)
        {
            Fix64Vector result;
            Fix64Vector.Divide(ref value1, scaleFactor, out result);
            return result;
        }

        public static void Divide(ref Fix64Vector value1, Fix64 scaleFactor, out Fix64Vector result)
        {
            result.x = value1.x / scaleFactor;
            result.y = value1.y / scaleFactor;
            result.z = value1.z / scaleFactor;
        }

        public static Fix64Vector Subtract(Fix64Vector value1, Fix64Vector value2)
        {
            Fix64Vector result;
            Fix64Vector.Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static void Subtract(ref Fix64Vector value1, ref Fix64Vector value2, out Fix64Vector result)
        {
            Fix64 fp1 = value1.x - value2.x;
            Fix64 fp2 = value1.y - value2.y;
            Fix64 fp3 = value1.z - value2.z;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public static Fix64Vector Cross(Fix64Vector vector1, Fix64Vector vector2)
        {
            Fix64Vector result;
            Fix64Vector.Cross(ref vector1, ref vector2, out result);
            return result;
        }

        public static void Cross(ref Fix64Vector vector1, ref Fix64Vector vector2, out Fix64Vector result)
        {
            Fix64 fp1 = vector1.y * vector2.z - vector1.z * vector2.y;
            Fix64 fp2 = vector1.z * vector2.x - vector1.x * vector2.z;
            Fix64 fp3 = vector1.x * vector2.y - vector1.y * vector2.x;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
        }

        public void Negate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        public static Fix64Vector Negate(Fix64Vector value)
        {
            Fix64Vector result;
            Fix64Vector.Negate(ref value, out result);
            return result;
        }

        public static void Negate(ref Fix64Vector value, out Fix64Vector result)
        {
            Fix64 fp1 = -value.x;
            Fix64 fp2 = -value.y;
            Fix64 fp3 = -value.z;
            result.x = fp1;
            result.y = fp2;
            result.z = fp3;
        }

        public static Fix64Vector Normalize(Fix64Vector value)
        {
            Fix64Vector result;
            Fix64Vector.Normalize(ref value, out result);
            return result;
        }

        public void Normalize()
        {
            Fix64 x = this.x * this.x + this.y * this.y + this.z * this.z;
            Fix64 fp = Fix64.One / Fix64.Sqrt(x);
            this.x = this.x * fp;
            this.y = this.y * fp;
            this.z = this.z * fp;
        }

        public static void Normalize(ref Fix64Vector value, out Fix64Vector result)
        {
            Fix64 x = value.x * value.x + value.y * value.y + value.z * value.z;
            Fix64 fp = Fix64.One / Fix64.Sqrt(x);
            result.x = value.x * fp;
            result.y = value.y * fp;
            result.z = value.z * fp;
        }

        public static void Swap(ref Fix64Vector vector1, ref Fix64Vector vector2)
        {
            Fix64 fp1 = vector1.x;
            vector1.x = vector2.x;
            vector2.x = fp1;
            Fix64 fp2 = vector1.y;
            vector1.y = vector2.y;
            vector2.y = fp2;
            Fix64 fp3 = vector1.z;
            vector1.z = vector2.z;
            vector2.z = fp3;
        }

        public static Fix64Vector Multiply(Fix64Vector value1, Fix64 scaleFactor)
        {
            Fix64Vector result;
            Fix64Vector.Multiply(ref value1, scaleFactor, out result);
            return result;
        }

        public static void Multiply(ref Fix64Vector value1, Fix64 scaleFactor, out Fix64Vector result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
        }
    }
}