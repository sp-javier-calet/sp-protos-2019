using FixMath.NET;

namespace SocialPoint.FixedMath
{
    public struct Fix64Matrix
    {
        public static readonly Fix64Matrix Zero = new Fix64Matrix();
        public static readonly Fix64Matrix Identity = new Fix64Matrix();
        public Fix64 M11;
        public Fix64 M12;
        public Fix64 M13;
        public Fix64 M21;
        public Fix64 M22;
        public Fix64 M23;
        public Fix64 M31;
        public Fix64 M32;
        public Fix64 M33;

        static Fix64Matrix()
        {
            Fix64Matrix.Identity.M11 = Fix64.One;
            Fix64Matrix.Identity.M22 = Fix64.One;
            Fix64Matrix.Identity.M33 = Fix64.One;
        }

        public Fix64Matrix(Fix64 m11, Fix64 m12, Fix64 m13, Fix64 m21, Fix64 m22, Fix64 m23, Fix64 m31, Fix64 m32, Fix64 m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        public static Fix64Matrix operator *(Fix64Matrix value1, Fix64Matrix value2)
        {
            Fix64Matrix result;
            Fix64Matrix.Multiply(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Vector operator *(Fix64Vector vec, Fix64Matrix mat)
        {
            return new Fix64Vector(Fix64Vector.Dot(vec, new Fix64Vector(mat.M11, mat.M12, mat.M13)),
                Fix64Vector.Dot(vec, new Fix64Vector(mat.M21, mat.M22, mat.M23)),
                Fix64Vector.Dot(vec, new Fix64Vector(mat.M31, mat.M32, mat.M33)));
        }

        public static Fix64Matrix operator +(Fix64Matrix value1, Fix64Matrix value2)
        {
            Fix64Matrix result;
            Fix64Matrix.Add(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Matrix operator -(Fix64Matrix value1, Fix64Matrix value2)
        {
            Fix64Matrix.Multiply(ref value2, -Fix64.One, out value2);
            Fix64Matrix result;
            Fix64Matrix.Add(ref value1, ref value2, out result);
            return result;
        }

        public static Fix64Matrix CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll)
        {
            Fix64Quaternion result1;
            Fix64Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out result1);
            Fix64Matrix result2;
            Fix64Matrix.CreateFromQuaternion(ref result1, out result2);
            return result2;
        }

        public static Fix64Matrix CreateRotationX(Fix64 radians)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            Fix64Matrix tsMatrix;
            tsMatrix.M11 = Fix64.One;
            tsMatrix.M12 = Fix64.Zero;
            tsMatrix.M13 = Fix64.Zero;
            tsMatrix.M21 = Fix64.Zero;
            tsMatrix.M22 = fp1;
            tsMatrix.M23 = fp2;
            tsMatrix.M31 = Fix64.Zero;
            tsMatrix.M32 = -fp2;
            tsMatrix.M33 = fp1;
            return tsMatrix;
        }

        public static void CreateRotationX(Fix64 radians, out Fix64Matrix result)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            result.M11 = Fix64.One;
            result.M12 = Fix64.Zero;
            result.M13 = Fix64.Zero;
            result.M21 = Fix64.Zero;
            result.M22 = fp1;
            result.M23 = fp2;
            result.M31 = Fix64.Zero;
            result.M32 = -fp2;
            result.M33 = fp1;
        }

        public static Fix64Matrix CreateRotationY(Fix64 radians)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            Fix64Matrix tsMatrix;
            tsMatrix.M11 = fp1;
            tsMatrix.M12 = Fix64.Zero;
            tsMatrix.M13 = -fp2;
            tsMatrix.M21 = Fix64.Zero;
            tsMatrix.M22 = Fix64.One;
            tsMatrix.M23 = Fix64.Zero;
            tsMatrix.M31 = fp2;
            tsMatrix.M32 = Fix64.Zero;
            tsMatrix.M33 = fp1;
            return tsMatrix;
        }

        public static void CreateRotationY(Fix64 radians, out Fix64Matrix result)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            result.M11 = fp1;
            result.M12 = Fix64.Zero;
            result.M13 = -fp2;
            result.M21 = Fix64.Zero;
            result.M22 = Fix64.One;
            result.M23 = Fix64.Zero;
            result.M31 = fp2;
            result.M32 = Fix64.Zero;
            result.M33 = fp1;
        }

        public static Fix64Matrix CreateRotationZ(Fix64 radians)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            Fix64Matrix tsMatrix;
            tsMatrix.M11 = fp1;
            tsMatrix.M12 = fp2;
            tsMatrix.M13 = Fix64.Zero;
            tsMatrix.M21 = -fp2;
            tsMatrix.M22 = fp1;
            tsMatrix.M23 = Fix64.Zero;
            tsMatrix.M31 = Fix64.Zero;
            tsMatrix.M32 = Fix64.Zero;
            tsMatrix.M33 = Fix64.One;
            return tsMatrix;
        }

        public static void CreateRotationZ(Fix64 radians, out Fix64Matrix result)
        {
            Fix64 fp1 = Fix64.Cos(radians);
            Fix64 fp2 = Fix64.Sin(radians);
            result.M11 = fp1;
            result.M12 = fp2;
            result.M13 = Fix64.Zero;
            result.M21 = -fp2;
            result.M22 = fp1;
            result.M23 = Fix64.Zero;
            result.M31 = Fix64.Zero;
            result.M32 = Fix64.Zero;
            result.M33 = Fix64.One;
        }

        public static Fix64Matrix Multiply(Fix64Matrix matrix1, Fix64Matrix matrix2)
        {
            Fix64Matrix result;
            Fix64Matrix.Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }

        public static void Multiply(ref Fix64Matrix matrix1, ref Fix64Matrix matrix2, out Fix64Matrix result)
        {
            Fix64 fp1 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31;
            Fix64 fp2 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32;
            Fix64 fp3 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33;
            Fix64 fp4 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31;
            Fix64 fp5 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32;
            Fix64 fp6 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33;
            Fix64 fp7 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31;
            Fix64 fp8 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32;
            Fix64 fp9 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33;
            result.M11 = fp1;
            result.M12 = fp2;
            result.M13 = fp3;
            result.M21 = fp4;
            result.M22 = fp5;
            result.M23 = fp6;
            result.M31 = fp7;
            result.M32 = fp8;
            result.M33 = fp9;
        }

        public static Fix64Matrix Add(Fix64Matrix matrix1, Fix64Matrix matrix2)
        {
            Fix64Matrix result;
            Fix64Matrix.Add(ref matrix1, ref matrix2, out result);
            return result;
        }

        public static void Add(ref Fix64Matrix matrix1, ref Fix64Matrix matrix2, out Fix64Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
        }

        public static Fix64Matrix Inverse(Fix64Matrix matrix)
        {
            Fix64Matrix result;
            Fix64Matrix.Inverse(ref matrix, out result);
            return result;
        }

        public Fix64 Determinant()
        {
            return this.M11 * this.M22 * this.M33 + this.M12 * this.M23 * this.M31 + this.M13 * this.M21 * this.M32 - this.M31 * this.M22 * this.M13 - this.M32 * this.M23 * this.M11 - this.M33 * this.M21 * this.M12;
        }

        public static void Invert(ref Fix64Matrix matrix, out Fix64Matrix result)
        {
            Fix64 fp1 = (Fix64) 1 / matrix.Determinant();
            Fix64 fp2 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32) * fp1;
            Fix64 fp3 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12) * fp1;
            Fix64 fp4 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13) * fp1;
            Fix64 fp5 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33) * fp1;
            Fix64 fp6 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31) * fp1;
            Fix64 fp7 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23) * fp1;
            Fix64 fp8 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * fp1;
            Fix64 fp9 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * fp1;
            Fix64 fp10 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * fp1;
            result.M11 = fp2;
            result.M12 = fp3;
            result.M13 = fp4;
            result.M21 = fp5;
            result.M22 = fp6;
            result.M23 = fp7;
            result.M31 = fp8;
            result.M32 = fp9;
            result.M33 = fp10;
        }

        public static void Inverse(ref Fix64Matrix matrix, out Fix64Matrix result)
        {
            Fix64 fp1 = (Fix64) 1024 * matrix.M11 * matrix.M22 * matrix.M33 - (Fix64) 1024 * matrix.M11 * matrix.M23 * matrix.M32 - (Fix64) 1024 * matrix.M12 * matrix.M21 * matrix.M33 + (Fix64) 1024 * matrix.M12 * matrix.M23 * matrix.M31 + (Fix64) 1024 * matrix.M13 * matrix.M21 * matrix.M32 - (Fix64) 1024 * matrix.M13 * matrix.M22 * matrix.M31;
            Fix64 fp2 = (Fix64) 1024 * matrix.M22 * matrix.M33 - (Fix64) 1024 * matrix.M23 * matrix.M32;
            Fix64 fp3 = (Fix64) 1024 * matrix.M13 * matrix.M32 - (Fix64) 1024 * matrix.M12 * matrix.M33;
            Fix64 fp4 = (Fix64) 1024 * matrix.M12 * matrix.M23 - (Fix64) 1024 * matrix.M22 * matrix.M13;
            Fix64 fp5 = (Fix64) 1024 * matrix.M23 * matrix.M31 - (Fix64) 1024 * matrix.M33 * matrix.M21;
            Fix64 fp6 = (Fix64) 1024 * matrix.M11 * matrix.M33 - (Fix64) 1024 * matrix.M31 * matrix.M13;
            Fix64 fp7 = (Fix64) 1024 * matrix.M13 * matrix.M21 - (Fix64) 1024 * matrix.M23 * matrix.M11;
            Fix64 fp8 = (Fix64) 1024 * matrix.M21 * matrix.M32 - (Fix64) 1024 * matrix.M31 * matrix.M22;
            Fix64 fp9 = (Fix64) 1024 * matrix.M12 * matrix.M31 - (Fix64) 1024 * matrix.M32 * matrix.M11;
            Fix64 fp10 = (Fix64) 1024 * matrix.M11 * matrix.M22 - (Fix64) 1024 * matrix.M21 * matrix.M12;
            if (fp1 == (Fix64) 0)
            {
                result.M11 = Fix64.MaxValue;
                result.M12 = Fix64.MaxValue;
                result.M13 = Fix64.MaxValue;
                result.M21 = Fix64.MaxValue;
                result.M22 = Fix64.MaxValue;
                result.M23 = Fix64.MaxValue;
                result.M31 = Fix64.MaxValue;
                result.M32 = Fix64.MaxValue;
                result.M33 = Fix64.MaxValue;
            }
            else
            {
                result.M11 = fp2 / fp1;
                result.M12 = fp3 / fp1;
                result.M13 = fp4 / fp1;
                result.M21 = fp5 / fp1;
                result.M22 = fp6 / fp1;
                result.M23 = fp7 / fp1;
                result.M31 = fp8 / fp1;
                result.M32 = fp9 / fp1;
                result.M33 = fp10 / fp1;
            }
        }

        public static Fix64Matrix Multiply(Fix64Matrix matrix1, Fix64 scaleFactor)
        {
            Fix64Matrix result;
            Fix64Matrix.Multiply(ref matrix1, scaleFactor, out result);
            return result;
        }

        public static void Multiply(ref Fix64Matrix matrix1, Fix64 scaleFactor, out Fix64Matrix result)
        {
            Fix64 fp = scaleFactor;
            result.M11 = matrix1.M11 * fp;
            result.M12 = matrix1.M12 * fp;
            result.M13 = matrix1.M13 * fp;
            result.M21 = matrix1.M21 * fp;
            result.M22 = matrix1.M22 * fp;
            result.M23 = matrix1.M23 * fp;
            result.M31 = matrix1.M31 * fp;
            result.M32 = matrix1.M32 * fp;
            result.M33 = matrix1.M33 * fp;
        }

        public static Fix64Matrix CreateFromLookAt(Fix64Vector position, Fix64Vector target)
        {
            Fix64Matrix result;
            Fix64Matrix.LookAt(out result, position, target);
            return result;
        }

        public static void LookAt(out Fix64Matrix result, Fix64Vector position, Fix64Vector target)
        {
            Fix64Vector tsVector1 = target - position;
            tsVector1.Normalize();
            Fix64Vector vector2 = Fix64Vector.Cross(Fix64Vector.Up, tsVector1);
            vector2.Normalize();
            Fix64Vector tsVector2 = Fix64Vector.Cross(tsVector1, vector2);
            result.M11 = vector2.x;
            result.M21 = tsVector2.x;
            result.M31 = tsVector1.x;
            result.M12 = vector2.y;
            result.M22 = tsVector2.y;
            result.M32 = tsVector1.y;
            result.M13 = vector2.z;
            result.M23 = tsVector2.z;
            result.M33 = tsVector1.z;
        }

        public static Fix64Matrix CreateFromQuaternion(Fix64Quaternion quaternion)
        {
            Fix64Matrix result;
            Fix64Matrix.CreateFromQuaternion(ref quaternion, out result);
            return result;
        }

        public static void CreateFromQuaternion(ref Fix64Quaternion quaternion, out Fix64Matrix result)
        {
            Fix64 fp1 = quaternion.x * quaternion.x;
            Fix64 fp2 = quaternion.y * quaternion.y;
            Fix64 fp3 = quaternion.z * quaternion.z;
            Fix64 fp4 = quaternion.x * quaternion.y;
            Fix64 fp5 = quaternion.z * quaternion.w;
            Fix64 fp6 = quaternion.z * quaternion.x;
            Fix64 fp7 = quaternion.y * quaternion.w;
            Fix64 fp8 = quaternion.y * quaternion.z;
            Fix64 fp9 = quaternion.x * quaternion.w;
            result.M11 = Fix64.One - (Fix64) 2 * (fp2 + fp3);
            result.M12 = (Fix64) 2 * (fp4 + fp5);
            result.M13 = (Fix64) 2 * (fp6 - fp7);
            result.M21 = (Fix64) 2 * (fp4 - fp5);
            result.M22 = Fix64.One - (Fix64) 2 * (fp3 + fp1);
            result.M23 = (Fix64) 2 * (fp8 + fp9);
            result.M31 = (Fix64) 2 * (fp6 + fp7);
            result.M32 = (Fix64) 2 * (fp8 - fp9);
            result.M33 = Fix64.One - (Fix64) 2 * (fp2 + fp1);
        }

        public static Fix64Matrix Transpose(Fix64Matrix matrix)
        {
            Fix64Matrix result;
            Fix64Matrix.Transpose(ref matrix, out result);
            return result;
        }

        public static void Transpose(ref Fix64Matrix matrix, out Fix64Matrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
        }

        public Fix64 Trace()
        {
            return this.M11 + this.M22 + this.M33;
        }

        public static void CreateFromAxisAngle(ref Fix64Vector axis, Fix64 angle, out Fix64Matrix result)
        {
            Fix64 fp1 = axis.x;
            Fix64 fp2 = axis.y;
            Fix64 fp3 = axis.z;
            Fix64 fp4 = Fix64.Sin(angle);
            Fix64 fp5 = Fix64.Cos(angle);
            Fix64 fp6 = fp1 * fp1;
            Fix64 fp7 = fp2 * fp2;
            Fix64 fp8 = fp3 * fp3;
            Fix64 fp9 = fp1 * fp2;
            Fix64 fp10 = fp1 * fp3;
            Fix64 fp11 = fp2 * fp3;
            result.M11 = fp6 + fp5 * (Fix64.One - fp6);
            result.M12 = fp9 - fp5 * fp9 + fp4 * fp3;
            result.M13 = fp10 - fp5 * fp10 - fp4 * fp2;
            result.M21 = fp9 - fp5 * fp9 - fp4 * fp3;
            result.M22 = fp7 + fp5 * (Fix64.One - fp7);
            result.M23 = fp11 - fp5 * fp11 + fp4 * fp1;
            result.M31 = fp10 - fp5 * fp10 + fp4 * fp2;
            result.M32 = fp11 - fp5 * fp11 - fp4 * fp1;
            result.M33 = fp8 + fp5 * (Fix64.One - fp8);
        }

        public static Fix64Matrix CreateFromAxisAngle(Fix64Vector axis, Fix64 angle)
        {
            Fix64Matrix result;
            Fix64Matrix.CreateFromAxisAngle(ref axis, angle, out result);
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", (object) this.M11.RawValue, (object) this.M12.RawValue, (object) this.M13.RawValue, (object) this.M21.RawValue, (object) this.M22.RawValue, (object) this.M23.RawValue, (object) this.M31.RawValue, (object) this.M32.RawValue, (object) this.M33.RawValue);
        }

        private void LookAt(Fix64Vector position, Fix64Vector target)
        {
            Fix64Matrix.LookAt(out this, position, target);
        }
    }
}