using UnityEngine;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public static class UnityModelExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this BulletSharp.Math.Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }

        public static UnityEngine.Quaternion ToUnity(this BulletSharp.Math.Quaternion q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        public static BulletSharp.Math.Vector3 ToMultiplayer(this UnityEngine.Vector3 v)
        {
            return new BulletSharp.Math.Vector3(v.x, v.y, v.z);
        }

        public static BulletSharp.Math.Quaternion ToMultiplayer(this UnityEngine.Quaternion q)
        {
            return new BulletSharp.Math.Quaternion(q.x, q.y, q.z, q.w);
        }
    }
}