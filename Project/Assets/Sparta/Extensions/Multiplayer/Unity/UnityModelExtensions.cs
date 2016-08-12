using UnityEngine;

namespace SocialPoint.Multiplayer
{
    public static class UnityModelExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this Vector3 v)
        {
            return new UnityEngine.Vector3( v.x, v.y, v.z );
        }

        public static UnityEngine.Quaternion ToUnity(this Quaternion q)
        {
            return new UnityEngine.Quaternion( q.x, q.y, q.z, q.w );
        }

        public static Vector3 ToMultiplayer(this UnityEngine.Vector3 v)
        {
            return new Vector3( v.x, v.y, v.z );
        }

        public static Quaternion ToMultiplayer(this UnityEngine.Quaternion q)
        {
            return new Quaternion( q.x, q.y, q.z, q.w );
        }
    }
}