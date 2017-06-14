﻿using Jitter.LinearMath;

public static partial class JQuaternionExtensions
{
    public static UnityEngine.Quaternion ToUnity(this JQuaternion q)
    {
        return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
    }
}