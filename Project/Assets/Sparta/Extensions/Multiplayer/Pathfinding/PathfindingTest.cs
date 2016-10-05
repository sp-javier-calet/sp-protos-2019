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
    public GameObject mapObject;

    SharpNav.NavMesh navMesh;

    // Use this for initialization
    void Start()
    {
        CombineMesh(mapObject);
        Mesh map = GetMesh(mapObject);
        UnityEngine.Vector3[] vertices = map.vertices;
        int[] triangles = map.triangles;
        int totalTriangles = triangles.Length / 3;
        Debug.Log("Mesh Triangles: " + totalTriangles);

        //prepare the geometry from your mesh data
        var tris = TriangleEnumerable.FromIndexedVector3(ConvertVectors(vertices), triangles, 0, 1, 0, totalTriangles);

        //use the default generation settings
        var settings = NavMeshGenerationSettings.Default;
        settings.AgentHeight = 1.7f;
        settings.AgentRadius = 0.6f;

        //generate the mesh
        navMesh = Generate(tris, settings);//change to SharpNav.NavMesh.
        Debug.Log("Total Tiles: " + navMesh.TileCount);
    }

    void Update()
    {
        foreach(var tile in navMesh.Tiles)
        {
            var verts = tile.Verts;
            for(int i = 0; i < verts.Length; i++)
            {
                var v1 = verts[i];
                var v2 = (i == verts.Length - 1) ? verts[0] : verts[i + 1];
                Debug.DrawLine(new UnityEngine.Vector3(v1.X, v1.Y, v1.Z), new UnityEngine.Vector3(v2.X, v2.Y, v2.Z), Color.red);
            }
        }
    }

    Mesh GetMesh(GameObject go)
    {
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        return meshFilter.mesh;
    }

    void CombineMesh(GameObject parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while(i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        parent.GetComponent<MeshFilter>().mesh = new Mesh();
        parent.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        parent.gameObject.SetActive(true);
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

    public static SharpNav.NavMesh Generate(IEnumerable<Triangle3> triangles, NavMeshGenerationSettings settings)
    {
        BBox3 bounds = triangles.GetBoundingBox(settings.CellSize);
        var hf = new Heightfield(bounds, settings);
        hf.RasterizeTriangles(triangles);
        hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
        hf.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
        hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

        var chf = new CompactHeightfield(hf, settings);
        chf.Erode(settings.VoxelAgentRadius);
        chf.BuildDistanceField();
        chf.BuildRegions(2, settings.MinRegionSize, settings.MergedRegionSize);

        var cont = chf.BuildContourSet(settings);

        var polyMesh = new PolyMesh(cont, settings);

        var polyMeshDetail = new PolyMeshDetail(polyMesh, chf, settings);

        var buildData = new NavMeshBuilder(polyMesh, polyMeshDetail, new OffMeshConnection[0], settings);

        var navMesh = new SharpNav.NavMesh(buildData);
        return navMesh;
    }
}
