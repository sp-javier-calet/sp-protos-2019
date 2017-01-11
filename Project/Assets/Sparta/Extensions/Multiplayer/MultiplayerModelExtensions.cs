using PathVector = SharpNav.Geometry.Vector3;
using PhysicsVector = Jitter.LinearMath.JVector;
using SharpNav.Pathfinding;
using SocialPoint.Geometry;

namespace SocialPoint.Multiplayer
{
    public static class MultiplayerExtensionsBridge
    {
        public static PhysicsVector[] StraightPathToVectors(StraightPath straightPath)
        {
            var navVectors = new PhysicsVector[straightPath.Count];

            for(int i = 0; i < straightPath.Count; i++)
            {
                var pathVert = straightPath[i];
                var point = pathVert.Point;
                navVectors[i] = Vector.Convert(point.Position);
            }

            return navVectors;
        }
    }
}

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

        public PhysicsVector ToPhysics()
        {
            return this;
        }
    }
}
