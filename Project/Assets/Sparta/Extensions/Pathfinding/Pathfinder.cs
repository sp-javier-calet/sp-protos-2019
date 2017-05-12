﻿using System.Collections.Generic;
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
        public TiledNavMesh NavMesh;

        /// <summary>
        /// Maximun distance in each axis that the 'start' and 'end' point can search for the closest navigation poly.
        /// </summary>
        Vector3 _extents;

        /// <summary>
        /// Maximun number that can be queued in a query.
        /// This value should be tweaked according to each game necessities.
        /// It is used to create containers (Pools, PriorityQueue, etc) with this value as capacity.
        /// The created containers are for expanding neighbours and other important search functions, so this number should be big enough or paths may be incomplete in some cases.
        /// </summary>
        int _maxNodes;

        NavQueryFilter _filter;
        NavMeshQuery _query;
        PathBuildFlags _flags;
        Path _path;

        public Pathfinder(TiledNavMesh navMesh, Vector3 extents, int maxNodes = 128)
        {
            NavMesh = navMesh;
            _extents = extents;
            _maxNodes = maxNodes;
            _filter = new NavQueryFilter();
            _query = new NavMeshQuery(NavMesh, _filter, _maxNodes);
            _flags = new PathBuildFlags();
            _path = new Path();
        }

        public void SetAreaCost(byte area, float cost)
        {
            if(area == Area.Null)
            {
                throw new System.Exception("Cannot assign cost to null {0} area");
            }
            _filter.SetAreaCost(area, cost);
        }

        public void SetIncludeFlags(ushort flags)
        {
            _filter.SetIncludeFlags(flags);
        }

        public void SetExcludeFlags(ushort flags)
        {
            _filter.SetExcludeFlags(flags);
        }

        /// <summary>
        /// Gets shortest path between two points in the navigation mesh.
        /// </summary>
        /// <returns>True if path is found, false otherwise.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <param name="straightPath">Resulting path.</param>
        public bool TryGetPath(Vector3 startPoint, Vector3 endPoint, StraightPath straightPath)
        {
            straightPath.Clear();
            //First, find poly path between targets
            NavPoint startNavPoint = _query.FindNearestPoly(startPoint, _extents, _filter);
            NavPoint endNavPoint = _query.FindNearestPoly(endPoint, _extents, _filter);
            _path.Clear();
            if(_query.FindPath(ref startNavPoint, ref endNavPoint, _filter, _path))
            {
                //With poly path found, find straight path
                if(_query.FindStraightPath(startNavPoint.Position, endNavPoint.Position, _path, straightPath, _flags))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
