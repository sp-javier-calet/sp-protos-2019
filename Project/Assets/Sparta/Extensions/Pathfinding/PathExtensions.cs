using System.Collections;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public static class PathExtensions
    {
        public static Vector3[] ToVector3Array(this StraightPath straightPath)
        {
            var navVectors = new Vector3[straightPath.Count];

            for(int i = 0; i < straightPath.Count; i++)
            {
                var pathVert = straightPath[i];
                var point = pathVert.Point;
                navVectors[i] = point.Position;
            }

            return navVectors;
        }
    }
}

