using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

public interface IPathfindingDebugger
{
    void SetColor(float r, float g, float b, float a);

    void DrawNavPoly(NavTile tile, NavPoly navPoly);

    void DrawNavMesh(TiledNavMesh navMesh);

    void DrawPolyPath(TiledNavMesh navMesh, StraightPath straightPath);

    void DrawStraightPath(StraightPath straightPath);
}
