using UnityVector = UnityEngine.Vector3;
using UnityQuaternion = UnityEngine.Quaternion;
using PhysicsVector = Jitter.LinearMath.JVector;
using PhysicsQuaternion = Jitter.LinearMath.JQuaternion;

namespace SocialPoint.Multiplayer
{
    public static class UnityModelExtensions
    {
        public static UnityVector ToUnity(this PhysicsVector v)
        {
            return new UnityVector(v.X, v.Y, v.Z);
        }

        public static UnityQuaternion ToUnity(this PhysicsQuaternion q)
        {
            return new UnityQuaternion(q.X, q.Y, q.Z, q.W);
        }


        public static PhysicsVector ToMultiplayer(this UnityVector v)
        {
            return new PhysicsVector(v.x, v.y, v.z);
        }

        public static PhysicsQuaternion ToMultiplayer(this UnityQuaternion q)
        {
            return new PhysicsQuaternion(q.x, q.y, q.z, q.w);
        }
    }
}