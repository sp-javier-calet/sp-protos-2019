using UnityEngine;
using System.Collections;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SocialPoint.Pathfinding
{
    public class PathfindingUnityDebugger : IPathfindingDebugger
    {
        public Color DefaultNavMeshColor = Color.red;
        public Color DefaultPathColor = Color.green;

        public void DrawNavMesh(TiledNavMesh navMesh)
        {
            DrawNavMesh(navMesh, DefaultNavMeshColor.r, DefaultNavMeshColor.g, DefaultNavMeshColor.b, DefaultNavMeshColor.a);
        }

        public void DrawNavMesh(TiledNavMesh navMesh, float r, float g, float b, float a)
        {
            Color color = new Color(r, g, b, a);

            //IMPORTANT: Use TileCount, PolyCount, VertCount instead of the arrays.Lenth, arrays can have additional "blank" positions
            for(int t = 0; t < navMesh.TileCount; t++)
            {
                var tile = navMesh.Tiles[t];
                var polys = tile.Polys;
                for(int p = 0; p < tile.PolyCount; p++)
                {
                    var poly = polys[p];
                    var verts = poly.Verts;
                    for(int i = 0; i < poly.VertCount; i++)
                    {
                        int index1 = verts[i];
                        int index2 = verts[(i + 1) % poly.VertCount];
                        var v1 = tile.Verts[index1].ToUnity();
                        var v2 = tile.Verts[index2].ToUnity();

                        Debug.DrawLine(v1, v2, color);
                    }
                }
            }
        }

        public void DrawStraightPath(StraightPath straightPath)
        {
            DrawStraightPath(straightPath, DefaultPathColor.r, DefaultPathColor.g, DefaultPathColor.b, DefaultPathColor.a);
        }

        public void DrawStraightPath(StraightPath straightPath, float r, float g, float b, float a)
        {
            Color color = new Color(r, g, b, a);

            for(int i = 0; i < straightPath.Count - 1; i++)
            {
                var v1 = GetUnityPoint(i, straightPath);
                var v2 = GetUnityPoint(i + 1, straightPath);

                Debug.DrawLine(v1, v2, color);
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
