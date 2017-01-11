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
