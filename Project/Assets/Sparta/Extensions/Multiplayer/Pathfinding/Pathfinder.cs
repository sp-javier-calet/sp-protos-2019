using System;
using System.IO;
using System.Collections;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

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
        /// <returns>The path.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="extents">Maximun distance in each axis that the 'start' and 'end' point can search for the closest navigation poly.</param>
        public StraightPath GetPath(Vector3 startPoint, Vector3 endPoint, Vector3 extents)
        {
            //Default empty path
            var straightPath = new StraightPath();

            //First, find poly path between targets
            var query = new NavMeshQuery(_navMesh, _maxNodes);
            NavPoint startPoly = query.FindNearestPoly(startPoint, extents);
            NavPoint endPoly = query.FindNearestPoly(endPoint, extents);
            var path = new SharpNav.Pathfinding.Path();
            if(query.FindPath(ref startPoly, ref endPoly, new NavQueryFilter(), path))
            {
                //With poly path found, find straight path
                query.FindStraightPath(startPoint, endPoint, path, straightPath, new PathBuildFlags());
            }

            return straightPath;
        }
    }
}
