using PhysicsVector = Jitter.LinearMath.JVector;
using SharpNav.Pathfinding;
using SocialPoint.Geometry;

namespace SocialPoint.Multiplayer
{
    public static class MultiplayerExtensionsBridge
    {
        public static Vector[] StraightPathToVectors(StraightPath straightPath)
        {
            var navVectors = new Vector[straightPath.Count];

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