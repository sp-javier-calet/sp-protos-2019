using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SharpNav.Collections;
using SharpNav.Crowds;
using SharpNav.Geometry;
using SharpNav.IO;
using SharpNav.Pathfinding;

public class PathfindingTest : MonoBehaviour
{
    public MeshFilter mapObject;

    // Use this for initialization
    void Start()
    {
        Mesh map = mapObject.mesh;
        UnityEngine.Vector3[] vertices = map.vertices;
        int[] triangles = map.triangles;
        int totalTriangles = triangles.Length / 3;

        //prepare the geometry from your mesh data
        var tris = TriangleEnumerable.FromIndexedVector3(ConvertVectors(vertices), triangles, 0, 0, 0, totalTriangles);

        //use the default generation settings
        var settings = NavMeshGenerationSettings.Default;
        settings.AgentHeight = 1.7f;
        settings.AgentRadius = 0.6f;

        //generate the mesh
        var navMesh = SharpNav.NavMesh.Generate(tris, settings);
        Debug.Log("Total Tiles: " + navMesh.TileCount);
    }

    SharpNav.Geometry.Vector3[] ConvertVectors(UnityEngine.Vector3[] vectors)
    {
        var navVectors = new SharpNav.Geometry.Vector3[vectors.Length];
        for(int i = 0; i < vectors.Length; i++)
        {
            UnityEngine.Vector3 uVector = vectors[i];
            navVectors[i] = new SharpNav.Geometry.Vector3(uVector.x, uVector.y, uVector.z);
        }
        return navVectors;
    }
}
