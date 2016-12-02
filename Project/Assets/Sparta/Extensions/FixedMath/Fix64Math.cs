using FixMath.NET;

namespace SocialPoint.FixedMath
{
    public static class Fix64Math
    {
        public static readonly Fix64 Pi = Fix64.Pi;
        public static readonly Fix64 Deg2RadFactor = Pi / (Fix64) 180;
        public static readonly Fix64 PiOver2 = Fix64.PiOver2;
        public static readonly Fix64 Epsilon = Fix64.One / (Fix64) 1000;
        public static readonly Fix64 Half = (Fix64)2147483648L;

        public static Fix64 Deg2Rad(Fix64 angle)
        {
            return Deg2RadFactor * angle;
        }

        public static Fix64 Max(Fix64 val1, Fix64 val2)
        {
            return val1 > val2 ? val1 : val2;
        }

        public static Fix64 Min(Fix64 val1, Fix64 val2)
        {
            return val1 < val2 ? val1 : val2;
        }

        public static Fix64 Max(Fix64 val1, Fix64 val2, Fix64 val3)
        {
            Fix64 Fix64 = val1 > val2 ? val1 : val2;
            return Fix64 > val3 ? Fix64 : val3;
        }

        public static Fix64 Clamp(Fix64 value, Fix64 min, Fix64 max)
        {
            value = value > max ? max : value;
            value = value < min ? min : value;
            return value;
        }

        public static void Absolute(ref Fix64Matrix matrix, out Fix64Matrix result)
        {
            result.M11 = Fix64.Abs(matrix.M11);
            result.M12 = Fix64.Abs(matrix.M12);
            result.M13 = Fix64.Abs(matrix.M13);
            result.M21 = Fix64.Abs(matrix.M21);
            result.M22 = Fix64.Abs(matrix.M22);
            result.M23 = Fix64.Abs(matrix.M23);
            result.M31 = Fix64.Abs(matrix.M31);
            result.M32 = Fix64.Abs(matrix.M32);
            result.M33 = Fix64.Abs(matrix.M33);
        }
    }
}
