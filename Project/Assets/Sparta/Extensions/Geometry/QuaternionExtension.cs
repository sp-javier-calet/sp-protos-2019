using Jitter.LinearMath;

public static class QuaternionExtension
{
    public static JVector Forward(this JQuaternion q)
    {
        return RotateVector(q, new JVector(0f, 0f, 1f));
    }

    public static JVector Up(this JQuaternion q)
    {
        return RotateVector(q, new JVector(0f, 1f, 0f));
    }

    public static JVector Right(this JQuaternion q)
    {
        return RotateVector(q, new JVector(1f, 0f, 0f));
    }

    public static JVector RotateVector(this JQuaternion q, JVector v)
    {
        JVector t = 2f * JVector.Cross(new JVector(q.X, q.Y, q.Z), v);
        return v + q.W * t + JVector.Cross(new JVector(q.X, q.Y, q.Z), t);
    }
}
