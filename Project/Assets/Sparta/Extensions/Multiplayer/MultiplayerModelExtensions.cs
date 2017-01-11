using PathVector = SharpNav.Geometry.Vector3;
using PhysicsVector = Jitter.LinearMath.JVector;
using SharpNav.Pathfinding;

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

    public static class MultiplayerExtensionsBridge
    {
        public static PhysicsVector[] StraightPathToVectors(StraightPath straightPath)
        {
            var navVectors = new PhysicsVector[straightPath.Count];

            for(int i = 0; i < straightPath.Count; i++)
            {
                var pathVert = straightPath[i];
                var point = pathVert.Point;
                navVectors[i] = point.Position.ToPhysics();
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

    // PathVector adapter
    public partial struct Vector
    {
        public static implicit operator Vector(PathVector v)
        {
            return new Vector(v.X, v.Y, v.Z);
        }

        public static implicit operator PathVector(Vector v)
        {
            return new PathVector(v._x, v._y, v._z);
        }

        public PathVector ToPathfinding()
        {
            return this;
        }
    }
}
