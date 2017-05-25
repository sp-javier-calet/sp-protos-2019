// Copyright (c) 2014 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;
using SharpNav.Geometry;

namespace SharpNav
{
    //TODO right now this is basically an alias for TiledNavMesh. Fix this in the future.

    /// <summary>
    /// A TiledNavMesh generated from a collection of triangles and some settings
    /// </summary>
    public class NavMesh : TiledNavMesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavMesh" /> class.
        /// </summary>
        /// <param name="builder">The NavMeshBuilder data</param>
        public NavMesh(NavMeshBuilder builder)
            : base(builder)
        {
        }

        /// <summary>
        /// Generates a <see cref="NavMesh"/> given a collection of triangles and some settings.
        /// </summary>
        /// <param name="triangles">The triangles that form the level.</param>
        /// <param name="settings">The settings to generate with.</param>
        /// <param name="areaVolumes">Input convex volumes delimiting areas.</param>
        /// <param name="areaToFlagsMap">Function to assign navigation flags to polygons given their areas.</param>
        /// <returns>A <see cref="NavMesh"/>.</returns>
        public static NavMesh Generate(IEnumerable<Triangle3> triangles, NavMeshGenerationSettings settings, IEnumerable<ConvexVolume> areaVolumes = null, Func<Area, ushort> areaToFlagsMap = null)
        {
            //[SP-Change] Added optional parameters "areaVolumes" and "areaToFlagsMap"

            BBox3 bounds = triangles.GetBoundingBox(settings.CellSize);
            var hf = new Heightfield(bounds, settings);
            hf.RasterizeTriangles(triangles);
            hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            hf.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            var chf = new CompactHeightfield(hf, settings);
            chf.Erode(settings.VoxelAgentRadius);

            //[SP-Change] Mark areas here (using original cpp demo code as reference)
            SetAreas(areaVolumes, chf);

            chf.BuildDistanceField();
            chf.BuildRegions(2, settings.MinRegionSize, settings.MergedRegionSize);

            var cont = chf.BuildContourSet(settings);

            var polyMesh = new PolyMesh(cont, settings);

            var polyMeshDetail = new PolyMeshDetail(polyMesh, chf, settings);

            //[SP-Change] Set poly flags here (using original cpp demo code as reference)
            SetPolyFlags(polyMesh, areaToFlagsMap);

            var buildData = new NavMeshBuilder(polyMesh, polyMeshDetail, new Pathfinding.OffMeshConnection[0], settings);

            var navMesh = new NavMesh(buildData);
            return navMesh;
        }

        static void SetAreas(IEnumerable<ConvexVolume> areaVolumes, CompactHeightfield chf)
        {
            if(areaVolumes != null)
            {
                var itr = areaVolumes.GetEnumerator();
                while(itr.MoveNext())
                {
                    MarkConvexPolyArea(itr.Current, chf);
                }
                itr.Dispose();
            }
        }

        static void SetPolyFlags(PolyMesh polyMesh, Func<Area, ushort> areaToFlagsMap)
        {
            if(areaToFlagsMap == null)
            {
                areaToFlagsMap = DefaultAreaToFlagsMap;
            }

            for(int i = 0; i < polyMesh.Polys.Length; i++)
            {
                var poly = polyMesh.Polys[i];
                if(poly.Area != Area.Null)
                {
                    poly.Flags = areaToFlagsMap(poly.Area);
                }
            }
        }

        static ushort DefaultAreaToFlagsMap(Area area)
        {
            return 0xffff;
        }

        static void MarkConvexPolyArea(ConvexVolume poly, CompactHeightfield chf)
        {
            Vector3 bMin = new Vector3(poly.Vertices[0]);
            Vector3 bMax = new Vector3(poly.Vertices[0]);
            for(int i = 1; i < poly.Vertices.Length; ++i)
            {
                MinValues(ref bMin, ref poly.Vertices[i]);
                MaxValues(ref bMax, ref poly.Vertices[i]);
            }
            bMin.Y = poly.Hmin;
            bMax.Y = poly.Hmax;

            int minx = (int)((bMin.X - chf.Bounds.Min.X) / chf.CellSize);
            int miny = (int)((bMin.Y - chf.Bounds.Min.Y) / chf.CellHeight);
            int minz = (int)((bMin.Z - chf.Bounds.Min.Z) / chf.CellSize);
            int maxx = (int)((bMax.X - chf.Bounds.Min.X) / chf.CellSize);
            int maxy = (int)((bMax.Y - chf.Bounds.Min.Y) / chf.CellHeight);
            int maxz = (int)((bMax.Z - chf.Bounds.Min.Z) / chf.CellSize);

            if(maxx < 0)
                return;
            if(minx >= chf.Width)
                return;
            if(maxz < 0)
                return;
            if(minz >= chf.Length)
                return;

            if(minx < 0)
                minx = 0;
            if(maxx >= chf.Width)
                maxx = chf.Width - 1;
            if(minz < 0)
                minz = 0;
            if(maxz >= chf.Length)
                maxz = chf.Length - 1;    

            // TODO: Optimize.
            var p = Vector3.Zero;
            for(int z = minz; z <= maxz; ++z)
            {
                var zwidth = z * chf.Width;
                var zBoundPoint = chf.Bounds.Min.Z + (z + 0.5f) * chf.CellSize;
                for(int x = minx; x <= maxx; ++x)
                {
                    var xBoundPoint = chf.Bounds.Min.X + (x + 0.5f) * chf.CellSize;
                    CompactCell c = chf.Cells[x + zwidth];
                    for(int i = (int)c.StartIndex, ni = (int)(c.StartIndex + c.Count); i < ni; ++i)
                    {
                        CompactSpan s = chf.Spans[i];
                        if(chf.Areas[i] == Area.Null)
                            continue;
                        if((int)s.Minimum >= miny && (int)s.Minimum <= maxy)
                        {
                            p.X = xBoundPoint;
                            p.Z = zBoundPoint;

                            if(PointInPoly(poly, ref p))
                            {
                                chf.Areas[i] = poly.Area;
                                chf.Tags[i] = poly.Tag;
                            }
                        }
                    }
                }
            }
        }

        static void MinValues(ref Vector3 vMain, ref Vector3 vOther)
        {
            vMain.X = Math.Min(vMain.X, vOther.X);
            vMain.Y = Math.Min(vMain.Y, vOther.Y);
            vMain.Z = Math.Min(vMain.Z, vOther.Z);
        }

        static void MaxValues(ref Vector3 vMain, ref Vector3 vOther)
        {
            vMain.X = Math.Max(vMain.X, vOther.X);
            vMain.Y = Math.Max(vMain.Y, vOther.Y);
            vMain.Z = Math.Max(vMain.Z, vOther.Z);
        }

        static bool PointInPoly(ConvexVolume poly, ref Vector3 p)
        {
            int i, j;
            bool c = false;
            for(i = 0, j = poly.Vertices.Length - 1; i < poly.Vertices.Length; j = i++)
            {
                Vector3 vi = poly.Vertices[i];
                Vector3 vj = poly.Vertices[j];
                if(((vi.Z > p.Z) != (vj.Z > p.Z)) &&
                   (p.X < (vj.X - vi.X) * (p.Z - vi.Z) / (vj.Z - vi.Z) + vi.X))
                    c = !c;
            }
            return c;
        }
    }
}
