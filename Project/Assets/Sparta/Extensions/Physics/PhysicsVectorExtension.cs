using PhysicsVector = Jitter.LinearMath.JVector;

namespace SocialPoint.Geometry
{
    // PhysicsVector adapter
    public partial struct Vector
    {
        public static Vector Convert(PhysicsVector v)
        {
            return v;
        }

        public static implicit operator Vector(PhysicsVector v)
        {
            return new Vector(v.X, v.Y, v.Z);
        }

        public static implicit operator PhysicsVector(Vector v)
        {
            return new PhysicsVector(v._x, v._y, v._z);
        }
    }
}