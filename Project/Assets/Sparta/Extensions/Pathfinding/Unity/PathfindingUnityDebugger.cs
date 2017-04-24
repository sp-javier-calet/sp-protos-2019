using UnityEngine;
using System;
using System.Collections;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public class PathfindingUnityDebugger : IPathfindingDebugger
    {
        public Func<NavPoly, bool> NavPolyConditionalDraw;

        Color _color = Color.red;

        public void SetColor(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);
        }

        public void DrawNavMesh(TiledNavMesh navMesh)
        {
            navMesh.IteratePolys(DrawNavPoly);
        }

        public void DrawNavPoly(TiledNavMesh navMesh, NavPolyId navPolyRef)
        {
            NavTile tile;
            NavPoly poly;
            if(navMesh.TryGetTileAndPolyByRef(navPolyRef, out tile, out poly))
            {
                DrawNavPoly(tile, poly);
            }
        }

        public void DrawNavPoly(NavTile tile, NavPoly navPoly)
        {
            if(NavPolyConditionalDraw != null && !NavPolyConditionalDraw(navPoly))
            {
                return;
            }

            var verts = navPoly.Verts;
            for(int i = 0; i < navPoly.VertCount; i++)
            {
                int index1 = verts[i];
                int index2 = verts[(i + 1) % navPoly.VertCount];
                var v1 = tile.Verts[index1].ToUnity();
                var v2 = tile.Verts[index2].ToUnity();

                Debug.DrawLine(v1, v2, _color);
            }
        }

        public void DrawStraightPath(StraightPath straightPath)
        {
            for(int i = 0; i < straightPath.Count - 1; i++)
            {
                var v1 = GetUnityPoint(i, straightPath);
                var v2 = GetUnityPoint(i + 1, straightPath);

                Debug.DrawLine(v1, v2, _color);
            }
        }

        UnityEngine.Vector3 GetUnityPoint(int i, StraightPath straightPath)
        {
            var pathVert = straightPath[i];
            var point = pathVert.Point;
            return point.Position.ToUnity();
        }
    }
}
