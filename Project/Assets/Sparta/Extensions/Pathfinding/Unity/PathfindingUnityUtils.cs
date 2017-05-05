using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Pathfinding;
using SharpNav.Geometry;
using UnityMesh = UnityEngine.Mesh;
using PathMesh = SocialPoint.Pathfinding.NavMeshBuildUtils.Mesh;
using UnityVector = UnityEngine.Vector3;
using PathVector = SharpNav.Geometry.Vector3;

namespace SocialPoint.Pathfinding
{
    public static class PathfindingUnityUtils
    {
        /// <summary>
        /// Combines the meshes in the game object's hierarchy and creates a nav mesh from them.
        /// </summary>
        /// <returns>The nav mesh.</returns>
        /// <param name="go">Parent game object in hierarchy.</param>
        /// <param name="settings">Settings.</param>
        public static TiledNavMesh CreateNavMesh(GameObject go, NavMeshGenerationSettings settings, IEnumerable<ConvexVolume> convexVolumes = null, Func<Area, ushort> areaToFlagsMap = null)
        {
            return CreateNavMesh(new GameObject[] { go }, settings, convexVolumes, areaToFlagsMap);
        }

        public static TiledNavMesh CreateNavMesh(GameObject[] gos, NavMeshGenerationSettings settings, IEnumerable<ConvexVolume> convexVolumes = null, Func<Area, ushort> areaToFlagsMap = null)
        {
            var gosBuildMeshes = new List<PathMesh>(); 

            for(int obj = 0; obj < gos.Length; obj++)
            {
                var go = gos[obj];

                //Get mesh objects
                var meshFilters = go.GetComponentsInChildren<MeshFilter>();
                var buildMeshes = new PathMesh[meshFilters.Length];
                for(int i = 0; i < meshFilters.Length; i++)
                {
                    buildMeshes[i] = UnityMeshToPathfinding(meshFilters[i].sharedMesh, meshFilters[i].transform);
                }
                gosBuildMeshes.AddRange(buildMeshes);
            }

            return NavMeshBuildUtils.CreateNavMesh(gosBuildMeshes, settings, convexVolumes, areaToFlagsMap);
        }

        public static IEnumerable<ConvexVolumeMarker> GetConvexVolumeMarkers(GameObject go)
        {
            return GetConvexVolumeMarkers(new GameObject[] { go });
        }

        public static IEnumerable<ConvexVolumeMarker> GetConvexVolumeMarkers(GameObject[] gos)
        {
            var gosConvexVolumes = new List<ConvexVolumeMarker>();
            for(int obj = 0; obj < gos.Length; obj++)
            {
                var go = gos[obj];
                //Get area volumes
                var convexVolumeMarkers = go.GetComponentsInChildren<ConvexVolumeMarker>();
                gosConvexVolumes.AddRange(convexVolumeMarkers);
            }
            return gosConvexVolumes;
        }

        /// <summary>         
        /// Combines the meshes in the game object's hierarchy.
        /// </summary>         
        /// <returns>The combined mesh.</returns>
        /// <param name="go">Parent game object in hierarchy.</param>
        public static UnityMesh CombineSubMeshesToUnity(GameObject go)
        {
            var meshFilters = go.GetComponentsInChildren<MeshFilter>();
            var combine = new CombineInstance[meshFilters.Length];
            for(int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }
            var mesh = new UnityMesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        public static PathMesh CombineSubMeshesToPathfinding(GameObject go)
        {
            var meshFilters = go.GetComponentsInChildren<MeshFilter>();
            var buildMeshes = new PathMesh[meshFilters.Length];
            for(int i = 0; i < meshFilters.Length; i++)
            {
                buildMeshes[i] = UnityMeshToPathfinding(meshFilters[i].sharedMesh, meshFilters[i].transform);
            }
            return NavMeshBuildUtils.CombineMeshes(buildMeshes);
        }

        public static PathVector[] UnityVectorsToPathfinding(UnityVector[] vectors)
        {
            var navVectors = new PathVector[vectors.Length];
            for(int i = 0; i < vectors.Length; i++)
            {
                navVectors[i] = vectors[i].ToPathfinding();
            }
            return navVectors;
        }

        public static PathMesh UnityMeshToPathfinding(UnityMesh unityMesh, Transform transform)
        {
            var mesh = new PathMesh();
            mesh.Vertices = UnityVectorsToPathfinding(TransformVertices(unityMesh.vertices, transform));
            mesh.Triangles = unityMesh.triangles;
            return mesh;
        }

        static UnityVector[] TransformVertices(UnityVector[] vertices, Transform transform)
        {
            var transVertices = new UnityVector[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                transVertices[i] = transform.TransformPoint(vertices[i]);
            }
            return transVertices;
        }
    }
}
