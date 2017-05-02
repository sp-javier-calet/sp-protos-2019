using Jitter.LinearMath;

public static partial class JVectorExtensions
{
    public static JVector Normalized(this JVector v)
    {
        if(v.LengthSquared() < 1e-4f)
        {
            return JVector.Zero;
        }
        v.Normalize();
        return v;
    }

    public static JVector ZeroXValue(this JVector v)
    {
        return new JVector(0f, v.Y, v.Z);
    }

    public static JVector ZeroYValue(this JVector v)
    {
        return new JVector(v.X, 0f, v.Z);
    }

    public static JVector ZeroZValue(this JVector v)
    {
        return new JVector(v.X, v.Y, 0f);
    }

    public static float DistanceSQ(this JVector v, JVector other)
    {
        return (v - other).LengthSquared();
    }

    public static bool NearlyEquals(this JVector v, JVector other, float sqMagnitudePrecision = 1e-3f)
    {
        return v.DistanceSQ(other) < sqMagnitudePrecision;
    }
}
