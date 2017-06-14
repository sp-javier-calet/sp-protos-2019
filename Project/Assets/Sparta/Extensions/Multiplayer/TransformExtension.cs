﻿using System;
using Jitter.LinearMath;
using SocialPoint.Multiplayer;

public static class TransformExtension 
{
    public static JVector Forward(this Transform transform)
    {
        return transform.Rotation.Forward();
    }

    public static JVector Right(this Transform transform)
    {
        return transform.Rotation.Right();
    }
    
    public static void LookAt(this Transform transform, JVector targetPos)
    {
        var direction = targetPos - transform.Position;
        var yRotation = (float)Math.Atan2(direction.X, direction.Z);
        JQuaternion.CreateFromYawPitchRoll(yRotation, 0f, 0f, out transform.Rotation);
    }
}
