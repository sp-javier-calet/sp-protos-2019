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
        var qVector = new JVector(q.X, q.Y, q.Z);
        var t = 2f * JVector.Cross(qVector, v);
        return v + q.W * t + JVector.Cross(qVector, t);
    }
}
