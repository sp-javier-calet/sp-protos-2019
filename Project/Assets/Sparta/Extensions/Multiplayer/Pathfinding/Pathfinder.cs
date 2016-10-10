using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public class Pathfinder
    {
        TiledNavMesh _navMesh;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Pathfinding.Pathfinder"/> class.
        /// </summary>
        /// <param name="data">Binary serialized NavMesh Data.</param>
        public Pathfinder(Stream navMeshData)
        {

        }

        public Pathfinder(TiledNavMesh navMesh)
        {

        }
    }
}
