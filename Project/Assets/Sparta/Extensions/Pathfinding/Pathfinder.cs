using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using Jitter.LinearMath;
using SocialPoint.Multiplayer;

namespace SocialPoint.Pathfinding
{
    public class Pathfinder
    {
        /// <summary>
        /// The nav mesh to query for paths.
        /// </summary>
        TiledNavMesh _navMesh;

        /// <summary>
        /// Maximun number that can be queued in a query.
        /// This value should be tweaked according to each game necessities.
        /// It is used to create containers (Pools, PriorityQueue, etc) with this value as capacity.
        /// </summary>
        int _maxNodes;

        public Pathfinder(TiledNavMesh navMesh, int maxNodes = 128)
        {
            _navMesh = navMesh;
            _maxNodes = maxNodes;
        }

        /// <summary>
        /// Gets shortest path between two points in the navigation mesh.
        /// </summary>
        /// <returns>True if path is found, false otherwise.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="extents">Maximun distance in each axis that the 'start' and 'end' point can search for the closest navigation poly.</param>
        /// <param name="straightPath">Resulting path.</param>
        public bool TryGetPath(Vector3 startPoint, Vector3 endPoint, Vector3 extents, out StraightPath straightPath)
        {
            straightPath = new StraightPath();
            //First, find poly path between targets
            var query = new NavMeshQuery(_navMesh, _maxNodes);
            NavPoint startNavPoint = query.FindNearestPoly(startPoint, extents);
            NavPoint endNavPoint = query.FindNearestPoly(endPoint, extents);
            var path = new SharpNav.Pathfinding.Path();
            if(query.FindPath(ref startNavPoint, ref endNavPoint, new NavQueryFilter(), path))
            {
                //With poly path found, find straight path
                if(query.FindStraightPath(startNavPoint.Position, endNavPoint.Position, path, straightPath, new PathBuildFlags()))
                {
                    return true;
                }
            }

            return false;
        }

        public static JVector[] StraightPathToVector(StraightPath straightPath)
        {
            var navVectors = new JVector[straightPath.Count];

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
