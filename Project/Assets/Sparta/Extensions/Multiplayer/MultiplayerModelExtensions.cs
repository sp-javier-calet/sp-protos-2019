using PathVector = SharpNav.Geometry.Vector3;
using PhysicsVector = Jitter.LinearMath.JVector;

namespace SocialPoint.Multiplayer
{
    public static class MultiplayerModelExtensions
    {
        public static PhysicsVector ToPhysics(this PathVector v)
        {
            return new PhysicsVector(v.X, v.Y, v.Z);
        }

        public static PathVector ToPathfinding(this PhysicsVector v)
        {
            return new PathVector(v.X, v.Y, v.Z);
        }
    }
}
