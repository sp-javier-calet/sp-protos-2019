using UnityEngine;
using System.Collections;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public class PathfindingUnityDebugger : IPathfindingDebugger
    {
        Color _color = Color.red;

        public void SetColor(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);
        }

        public void DrawNavPoly(NavTile tile, NavPoly navPoly)
        {
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

        public void DrawNavMesh(TiledNavMesh navMesh)
        {
            //IMPORTANT: Use TileCount, PolyCount, VertCount instead of the arrays.Lenth, arrays can have additional "blank" positions
            for(int t = 0; t < navMesh.TileCount; t++)
            {
                var tile = navMesh.Tiles[t];
                var polys = tile.Polys;
                for(int p = 0; p < tile.PolyCount; p++)
                {
                    var poly = polys[p];
                    DrawNavPoly(tile, poly);
                }
            }
        }

        public void DrawPolyPath(TiledNavMesh navMesh, StraightPath straightPath)
        {
            for(int i = 0; i < straightPath.Count; i++)
            {
                var pathVert = straightPath[i];
                var point = pathVert.Point;
                NavTile tile;
                NavPoly poly;
                if(navMesh.TryGetTileAndPolyByRef(point.Polygon, out tile, out poly))
                {
                    DrawNavPoly(tile, poly);
                }
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
