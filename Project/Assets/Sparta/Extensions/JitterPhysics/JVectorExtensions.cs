namespace Jitter.LinearMath
{
    public static partial class JVectorExtensions
    {
        const float _epsilon = 1e-5f;

        public static JVector Normalized(this JVector v)
        {
            if(v.LengthSquared() < _epsilon)
            {
                v = JVector.Zero;
            }
            else
            {
                v.Normalize();
            }
            return v;
        }

        public static JVector ZeroYValue(this JVector v)
        {
            return new JVector(v.X, 0f, v.Z);
        }

        public static float DistanceSQ(this JVector v, JVector other)
        {
            return (v - other).LengthSquared();
        }

        public static float LengthXZ(this JVector v)
        {
            var sq = v.X * v.X + v.Z * v.Z;
            return JMath.Sqrt(sq);
        }

        public static bool NearlyEquals(this JVector v, JVector other, float sqMagnitudePrecision = 1e-3f)
        {
            return v.DistanceSQ(other) < sqMagnitudePrecision;
        }
    }
}
