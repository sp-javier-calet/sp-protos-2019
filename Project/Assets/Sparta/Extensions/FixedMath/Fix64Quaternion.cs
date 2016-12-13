using System;
using FixMath.NET;

namespace SocialPoint.FixedMath
{
    [Serializable]
    public struct Fix64Quaternion
    {
        public static readonly Fix64Quaternion Identity = new Fix64Quaternion((Fix64) 0, (Fix64) 0, (Fix64) 0, (Fix64) 1);
        public Fix64 x;
        public Fix64 y;
        public Fix64 z;
        public Fix64 w;

        public Fix64Quaternion(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Fix64Quaternion operator *(Fix64Quaternion value1, Fix64Quaternion value2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Multiply(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Quaternion operator +(Fix64Quaternion value1, Fix64Quaternion value2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Add(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Quaternion operator -(Fix64Quaternion value1, Fix64Quaternion value2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Quaternion Add(Fix64Quaternion quaternion1, Fix64Quaternion quaternion2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Add(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        public static void CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll, out Fix64Quaternion result)
        {
            Fix64 x1 = roll * Fix64Math.Half;
            Fix64 fp1 = Fix64.Sin(x1);
            Fix64 fp2 = Fix64.Cos(x1);
            Fix64 x2 = pitch * Fix64Math.Half;
            Fix64 fp3 = Fix64.Sin(x2);
            Fix64 fp4 = Fix64.Cos(x2);
            Fix64 x3 = yaw * Fix64Math.Half;
            Fix64 fp5 = Fix64.Sin(x3);
            Fix64 fp6 = Fix64.Cos(x3);
            result.x = fp6 * fp3 * fp2 + fp5 * fp4 * fp1;
            result.y = fp5 * fp4 * fp2 - fp6 * fp3 * fp1;
            result.z = fp6 * fp4 * fp1 - fp5 * fp3 * fp2;
            result.w = fp6 * fp4 * fp2 + fp5 * fp3 * fp1;
        }

        public static void Add(ref Fix64Quaternion quaternion1, ref Fix64Quaternion quaternion2, out Fix64Quaternion result)
        {
            result.x = quaternion1.x + quaternion2.x;
            result.y = quaternion1.y + quaternion2.y;
            result.z = quaternion1.z + quaternion2.z;
            result.w = quaternion1.w + quaternion2.w;
        }

        public static Fix64Quaternion Conjugate(Fix64Quaternion value)
        {
            Fix64Quaternion tsQuaternion;
            tsQuaternion.x = -value.x;
            tsQuaternion.y = -value.y;
            tsQuaternion.z = -value.z;
            tsQuaternion.w = value.w;
            return tsQuaternion;
        }

        public static Fix64Quaternion Subtract(Fix64Quaternion quaternion1, Fix64Quaternion quaternion2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Subtract(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        public static void Subtract(ref Fix64Quaternion quaternion1, ref Fix64Quaternion quaternion2, out Fix64Quaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }

        public static Fix64Quaternion Multiply(Fix64Quaternion quaternion1, Fix64Quaternion quaternion2)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Multiply(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        public static void Multiply(ref Fix64Quaternion quaternion1, ref Fix64Quaternion quaternion2, out Fix64Quaternion result)
        {
            Fix64 fp1 = quaternion1.x;
            Fix64 fp2 = quaternion1.y;
            Fix64 fp3 = quaternion1.z;
            Fix64 fp4 = quaternion1.w;
            Fix64 fp5 = quaternion2.x;
            Fix64 fp6 = quaternion2.y;
            Fix64 fp7 = quaternion2.z;
            Fix64 fp8 = quaternion2.w;
            Fix64 fp9 = fp2 * fp7 - fp3 * fp6;
            Fix64 fp10 = fp3 * fp5 - fp1 * fp7;
            Fix64 fp11 = fp1 * fp6 - fp2 * fp5;
            Fix64 fp12 = fp1 * fp5 + fp2 * fp6 + fp3 * fp7;
            result.x = fp1 * fp8 + fp5 * fp4 + fp9;
            result.y = fp2 * fp8 + fp6 * fp4 + fp10;
            result.z = fp3 * fp8 + fp7 * fp4 + fp11;
            result.w = fp4 * fp8 - fp12;
        }

        public static Fix64Quaternion Multiply(Fix64Quaternion quaternion1, Fix64 scaleFactor)
        {
            Fix64Quaternion result;
            Fix64Quaternion.Multiply(ref quaternion1, scaleFactor, out result);
            return result;
        }

        public static void Multiply(ref Fix64Quaternion quaternion1, Fix64 scaleFactor, out Fix64Quaternion result)
        {
            result.x = quaternion1.x * scaleFactor;
            result.y = quaternion1.y * scaleFactor;
            result.z = quaternion1.z * scaleFactor;
            result.w = quaternion1.w * scaleFactor;
        }

        public void Normalize()
        {
            Fix64 fp = (Fix64) 1 / Fix64.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w);
            this.x = this.x * fp;
            this.y = this.y * fp;
            this.z = this.z * fp;
            this.w = this.w * fp;
        }

        public static Fix64Quaternion CreateFromMatrix(Fix64Matrix matrix)
        {
            Fix64Quaternion result;
            Fix64Quaternion.CreateFromMatrix(ref matrix, out result);
            return result;
        }

        public static void CreateFromMatrix(ref Fix64Matrix matrix, out Fix64Quaternion result)
        {
            Fix64 fp1 = matrix.M11 + matrix.M22 + matrix.M33;
            if (fp1 > Fix64.Zero)
            {
                Fix64 fp2 = Fix64.Sqrt(fp1 + Fix64.One);
                result.w = fp2 * Fix64Math.Half;
                Fix64 fp3 = Fix64Math.Half / fp2;
                result.x = (matrix.M23 - matrix.M32) * fp3;
                result.y = (matrix.M31 - matrix.M13) * fp3;
                result.z = (matrix.M12 - matrix.M21) * fp3;
            }
            else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
            {
                Fix64 fp2 = Fix64.Sqrt(Fix64.One + matrix.M11 - matrix.M22 - matrix.M33);
                Fix64 fp3 = Fix64Math.Half / fp2;
                result.x = Fix64Math.Half * fp2;
                result.y = (matrix.M12 + matrix.M21) * fp3;
                result.z = (matrix.M13 + matrix.M31) * fp3;
                result.w = (matrix.M23 - matrix.M32) * fp3;
            }
            else if (matrix.M22 > matrix.M33)
            {
                Fix64 fp2 = Fix64.Sqrt(Fix64.One + matrix.M22 - matrix.M11 - matrix.M33);
                Fix64 fp3 = Fix64Math.Half / fp2;
                result.x = (matrix.M21 + matrix.M12) * fp3;
                result.y = Fix64Math.Half * fp2;
                result.z = (matrix.M32 + matrix.M23) * fp3;
                result.w = (matrix.M31 - matrix.M13) * fp3;
            }
            else
            {
                Fix64 fp2 = Fix64.Sqrt(Fix64.One + matrix.M33 - matrix.M11 - matrix.M22);
                Fix64 fp3 = Fix64Math.Half / fp2;
                result.x = (matrix.M31 + matrix.M13) * fp3;
                result.y = (matrix.M32 + matrix.M23) * fp3;
                result.z = Fix64Math.Half * fp2;
                result.w = (matrix.M12 - matrix.M21) * fp3;
            }
        }

        public override string ToString()
        {
            return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", (object) this.x, (object) this.y, (object) this.z, (object) this.w);
        }
    }
}