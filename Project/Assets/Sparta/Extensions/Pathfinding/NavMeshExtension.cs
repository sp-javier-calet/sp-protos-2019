using System;
using System.Collections;
using SharpNav;
using SharpNav.Pathfinding;
using PolyAction = System.Action<SharpNav.Pathfinding.NavTile, SharpNav.Pathfinding.NavPoly>;

namespace SocialPoint.Pathfinding
{
    public static class NavMeshExtension
    {
        public static void IteratePolys(this TiledNavMesh navMesh, PolyAction action)
        {
            //IMPORTANT: Use TileCount, PolyCount, VertCount instead of the arrays.Lenth, arrays can have additional "blank" positions
            for(int t = 0; t < navMesh.TileCount; t++)
            {
                var tile = navMesh.Tiles[t];
                var polys = tile.Polys;
                for(int p = 0; p < tile.PolyCount; p++)
                {
                    var poly = polys[p];
                    action(tile, poly);
                }
            }
        }

        public static void SetFlags(this TiledNavMesh navMesh, Func<Area, ushort> areaToFlagsMap)
        {
            navMesh.IteratePolys((NavTile tile, NavPoly poly) => {
                poly.Flags = areaToFlagsMap(poly.Area);
            });
        }
    }
}
