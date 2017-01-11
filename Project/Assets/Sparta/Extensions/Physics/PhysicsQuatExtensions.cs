using PhysicsQuaternion = Jitter.LinearMath.JQuaternion;

namespace SocialPoint.Geometry
{
    public partial struct Quat
    {
        public static Quat Convert(PhysicsQuaternion q)
        {
            return new Quat(q.X, q.Y, q.Z, q.W);
        }

        public static implicit operator Quat(PhysicsQuaternion q)
        {
            return new Quat(q.X, q.Y, q.Z, q.W);
        }

        public static implicit operator PhysicsQuaternion(Quat q)
        {
            return new PhysicsQuaternion(q._x, q._y, q._z, q._w);
        }
    }
}
