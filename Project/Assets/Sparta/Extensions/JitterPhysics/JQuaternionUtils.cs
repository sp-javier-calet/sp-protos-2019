using System;

namespace Jitter.LinearMath
{
    public static class JQuaternionUtils
    {
        public static JQuaternion LookRotation(JVector forward)
        {
            forward.Normalize();

            JQuaternion rotation;
            var yRotation = (float)Math.Atan2(forward.X, forward.Z);
            JQuaternion.CreateFromYawPitchRoll(yRotation, 0f, 0f, out rotation);
            return rotation;
        }

        public static JQuaternion SlerpQuaternion(ref JQuaternion a, ref JQuaternion b, float t, out JQuaternion r)
        {
            var t_ = 1 - t;
            var theta = acos(a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W);
            var sn = sin(theta);
            var Wa = sin(t_ * theta) / sn;
            var Wb = sin(t * theta) / sn;
            r.X = Wa * a.X + Wb * b.X;
            r.Y = Wa * a.Y + Wb * b.Y;
            r.Z = Wa * a.Z + Wb * b.Z;
            r.W = Wa * a.W + Wb * b.W;
            r.Normalize();
            return r;
        }

        public static void LerpQuaternion(ref JQuaternion quaternion1, ref JQuaternion quaternion2, float amount, out JQuaternion result)
        {
            float num = amount;
            float num2 = 1f - num;
            float num5 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
            if(num5 >= 0f)
            {
                result.X = (num2 * quaternion1.X) + (num * quaternion2.X);
                result.Y = (num2 * quaternion1.Y) + (num * quaternion2.Y);
                result.Z = (num2 * quaternion1.Z) + (num * quaternion2.Z);
                result.W = (num2 * quaternion1.W) + (num * quaternion2.W);
            }
            else
            {
                result.X = (num2 * quaternion1.X) - (num * quaternion2.X);
                result.Y = (num2 * quaternion1.Y) - (num * quaternion2.Y);
                result.Z = (num2 * quaternion1.Z) - (num * quaternion2.Z);
                result.W = (num2 * quaternion1.W) - (num * quaternion2.W);
            }
            float num4 = (((result.X * result.X) + (result.Y * result.Y)) + (result.Z * result.Z)) + (result.W * result.W);
            float num3 = 1f / ((float)Math.Sqrt((float)num4));
            result.X *= num3;
            result.Y *= num3;
            result.Z *= num3;
            result.W *= num3;
        }

        public static void Slerp(ref JQuaternion quaternion1, ref JQuaternion quaternion2, float amount, out JQuaternion result)
        {
            float num2;
            float num3;
            float num = amount;
            float num4 = (((quaternion1.X * quaternion2.X) + (quaternion1.Y * quaternion2.Y)) + (quaternion1.Z * quaternion2.Z)) + (quaternion1.W * quaternion2.W);
            bool flag = false;
            if(num4 < 0f)
            {
                flag = true;
                num4 = -num4;
            }
            if(num4 > 0.999999f)
            {
                num3 = 1f - num;
                num2 = flag ? -num : num;
            }
            else
            {
                float num5 = (float)Math.Acos((float)num4);
                float num6 = (float)(1.0 / Math.Sin((float)num5));
                num3 = ((float)Math.Sin((float)((1f - num) * num5))) * num6;
                num2 = flag ? (((float)-Math.Sin((float)(num * num5))) * num6) : (((float)Math.Sin((float)(num * num5))) * num6);
            }
            result.X = (num3 * quaternion1.X) + (num2 * quaternion2.X);
            result.Y = (num3 * quaternion1.Y) + (num2 * quaternion2.Y);
            result.Z = (num3 * quaternion1.Z) + (num2 * quaternion2.Z);
            result.W = (num3 * quaternion1.W) + (num2 * quaternion2.W);
        }

        static float acos(float v)
        {
            return (float)Math.Acos((double)v);
        }

        static float sin(float v)
        {
            return (float)Math.Sin((double)v);
        }
    }
}
