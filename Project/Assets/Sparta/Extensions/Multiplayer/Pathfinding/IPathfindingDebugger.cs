using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

public interface IPathfindingDebugger
{
    void DrawNavMesh(TiledNavMesh navMesh);

    void DrawNavMesh(TiledNavMesh navMesh, float r, float g, float b, float a);

    void DrawStraightPath(StraightPath straightPath);

    void DrawStraightPath(StraightPath straightPath, float r, float g, float b, float a);
}
