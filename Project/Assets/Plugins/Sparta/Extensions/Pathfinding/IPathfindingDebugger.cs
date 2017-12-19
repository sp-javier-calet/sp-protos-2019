using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public interface IPathfindingDebugger
    {
        void SetColor(float r, float g, float b, float a);

        void DrawNavPoly(TiledNavMesh navMesh, NavPolyId navPolyRef);

        void DrawNavPoly(NavTile tile, NavPoly navPoly);

        void DrawNavMesh(TiledNavMesh navMesh);

        void DrawStraightPath(StraightPath straightPath);
    }
}