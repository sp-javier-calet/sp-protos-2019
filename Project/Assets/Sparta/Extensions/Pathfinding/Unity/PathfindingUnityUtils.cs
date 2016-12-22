using UnityEngine;
using SharpNav;
using SocialPoint.Geometry;
using SharpNav.Geometry;
using UnityVector = UnityEngine.Vector3;
using PathVector = SharpNav.Geometry.Vector3;

namespace SocialPoint.Pathfinding
{
    public static class PathfindingUnityUtils
    {
        /// <summary>
        /// Combines the meshes in the game object's hierarchy.
        /// </summary>
        /// <returns>The combined mesh.</returns>
        /// <param name="go">Parent game object in hierarchy.</param>
        public static Mesh CombineSubMeshes(GameObject go)
        {
            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            for(int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        /// <summary>
        /// Creates a nav mesh from a Unity Mesh object.
        /// </summary>
        /// <returns>The nav mesh.</returns>
        /// <param name="mesh">Unity Mesh.</param>
        /// <param name="settings">Settings.</param>
        public static TiledNavMesh CreateNavMesh(Mesh mesh, NavMeshGenerationSettings settings)
        {
            var vertices = mesh.vertices;
            var navVertices = ConvertVectorsToPathfinding(vertices);

            var indices = mesh.triangles;
            int totalTriangles = indices.Length / 3;

            /* vertStride: How the triangles are packed in the data.
             *      0 means tighly packet triangles (last vertex from one is the first vertex for the next).
             *      1 means every three vertices are a triangle by themselves.
             * */
            int vertStride = 1;

            //prepare the geometry from your mesh data
            var tris = TriangleEnumerable.FromIndexedVector3(navVertices, indices, 0, vertStride, 0, totalTriangles);

            //generate the mesh
            return SharpNav.NavMesh.Generate(tris, settings);
        }

        public static PathVector[] ConvertVectorsToPathfinding(UnityVector[] vectors)
        {
            var navVectors = new PathVector[vectors.Length];
            for(int i = 0; i < vectors.Length; i++)
            {
                navVectors[i] = Vector.Convert(vectors[i]);
            }
            return navVectors;
        }

        public static UnityVector[] ConvertVectorsToUnity(PathVector[] vectors)
        {
            var navVectors = new UnityVector[vectors.Length];
            for(int i = 0; i < vectors.Length; i++)
            {
                navVectors[i] = Vector.Convert(vectors[i]);
            }
            return navVectors;
        }
    }
}
