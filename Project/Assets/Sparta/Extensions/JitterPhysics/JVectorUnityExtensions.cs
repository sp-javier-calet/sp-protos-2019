﻿using Jitter.LinearMath;

public static partial class JVectorExtensions
{
    public static JVector ToJitter(this UnityEngine.Vector3 v)
    {
        return new JVector(v.x, v.y, v.z);
    }

    public static UnityEngine.Vector3 ToUnity(this JVector v)
    {
        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    public static JVector ToJitter(this UnityEngine.Vector2 v)
    {
        return new JVector(v.x, v.y, 0f);
    }

    public static UnityEngine.Vector2 ToUnityVector2(this JVector v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    public static float LengthXZ(this JVector v)
    {
        var sq = v.X * v.X + v.Z * v.Z;
        return JMath.Sqrt(sq);
    }
}
