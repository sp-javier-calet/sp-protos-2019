namespace SocialPoint.Multiplayer
{
    public static class UnityModelExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this Jitter.LinearMath.JVector v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }

        public static UnityEngine.Quaternion ToUnity(this Jitter.LinearMath.JQuaternion q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }


        public static Jitter.LinearMath.JVector ToMultiplayer(this UnityEngine.Vector3 v)
        {
            return new Jitter.LinearMath.JVector(v.x, v.y, v.z);
        }

        public static Jitter.LinearMath.JQuaternion ToMultiplayer(this UnityEngine.Quaternion q)
        {
            return new Jitter.LinearMath.JQuaternion(q.x, q.y, q.z, q.w);
        }
    }
}