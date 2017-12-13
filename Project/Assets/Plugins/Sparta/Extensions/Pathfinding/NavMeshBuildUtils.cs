using System;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Geometry;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public static class NavMeshBuildUtils
    {
        public class Mesh
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public void Serialize(IWriter writer)
            {
                writer.WriteArray(Vertices, NavVector3Serializer.Instance.Serialize);
                writer.WriteInt32Array(Triangles);
            }

            public void Deserialize(IReader reader)
            {
                Vertices = reader.ReadArray<Vector3>(NavVector3Parser.Instance.Parse);
                Triangles = reader.ReadInt32Array();
            }
        }

        public static TiledNavMesh CreateNavMesh(IEnumerable<Mesh> meshes, NavMeshGenerationSettings settings, IEnumerable<ConvexVolume> areaVolumes = null, Func<Area, ushort> areaToFlagsMap = null)
        {
            return CreateNavMesh(CombineMeshes(meshes), settings, areaVolumes, areaToFlagsMap);
        }

        public static TiledNavMesh CreateNavMesh(Mesh mesh, NavMeshGenerationSettings settings, IEnumerable<ConvexVolume> areaVolumes = null, Func<Area, ushort> areaToFlagsMap = null)
        {
            var vertices = mesh.Vertices;
            var indices = mesh.Triangles;
            int totalTriangles = indices.Length / 3;

            /* vertStride: How the triangles are packed in the data.
             *      0 means tighly packet triangles (last vertex from one is the first vertex for the next).
             *      1 means every three vertices are a triangle by themselves.
             * */
            int vertStride = 1;

            //prepare the geometry from your mesh data
            var tris = TriangleEnumerable.FromIndexedVector3(vertices, indices, 0, vertStride, 0, totalTriangles);

            //generate the mesh
            return SharpNav.NavMesh.Generate(tris, settings, areaVolumes, areaToFlagsMap);
        }

        public static Mesh CombineMeshes(IEnumerable<Mesh> meshes)
        {
            var combinedVertices = new List<Vector3>();
            var combinedIndices = new List<int>();
            int offset = 0;
            var itr = meshes.GetEnumerator();
            while(itr.MoveNext())
            {
                var mesh = itr.Current;
                combinedVertices.AddRange(mesh.Vertices);
                combinedIndices.AddRange(Offset(mesh.Triangles, offset));
                offset += mesh.Vertices.Length;
            }
            itr.Dispose();

            var combinedMesh = new Mesh();
            combinedMesh.Vertices = combinedVertices.ToArray();
            combinedMesh.Triangles = combinedIndices.ToArray();
            return combinedMesh;
        }

        static int[] Offset(int[] original, int offset)
        {
            var offsetValues = new int[original.Length];
            for(int i = 0; i < original.Length; i++)
            {
                offsetValues[i] = original[i] + offset;
            }
            return offsetValues;
        }
    }
}
