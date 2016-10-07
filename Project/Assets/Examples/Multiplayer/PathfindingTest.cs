using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SharpNav;
using SharpNav.Collections;
using SharpNav.Crowds;
using SharpNav.Geometry;
using SharpNav.Pathfinding;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Pathfinding;

public class PathfindingTest : MonoBehaviour, IPointerClickHandler
{
    const string kTestNavMeshFile = "/Users/abarrera/Projects/sp-unity-BaseGame/Project/Assets/Examples/Multiplayer/Resources/test_bin_nav_mesh";

    public GameObject pathNodePrefab;
    public bool loadFromFile = false;

    SharpNav.TiledNavMesh navMesh;
    int maxNodes = 1000;

    List<SharpNav.Geometry.Vector3> navPath = new List<SharpNav.Geometry.Vector3>();
    SharpNav.Geometry.Vector3 startPoint = SharpNav.Geometry.Vector3.Zero;
    bool started = false;

    List<GameObject> navPathNodes = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        CombineMesh(gameObject);
        Mesh map = GetMesh(gameObject);
        UnityEngine.Vector3[] vertices = map.vertices;
        int[] triangles = map.triangles;
        int totalTriangles = triangles.Length / 3;
        Debug.Log("Mesh Triangles: " + totalTriangles);

        if(!loadFromFile)
        {
            //prepare the geometry from your mesh data
            var tris = TriangleEnumerable.FromIndexedVector3(ConvertVectorsToPathfinding(vertices), triangles, 0, 1, 0, totalTriangles);

            //use the default generation settings
            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = 1.7f;
            settings.AgentRadius = 0.5f;

            //generate the mesh
            navMesh = SharpNav.NavMesh.Generate(tris, settings);
            SaveNavMesh();
        }
        else
        {
            LoadNavMesh();
        }
        Debug.Log("Total Tiles: " + navMesh.TileCount);
    }

    void Update()
    {
        DrawNavmesh();
        DrawPath();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        UnityEngine.Ray clickRay = eventData.pressEventCamera.ScreenPointToRay(eventData.pressPosition);
        UnityEngine.RaycastHit hit;
        if(Physics.Raycast(clickRay, out hit, float.MaxValue))
        {
            if(started)
            {
                var endPoint = hit.point.ToPathfinding();
                var query = new NavMeshQuery(navMesh, maxNodes);
                NavPoint startPoly = query.FindNearestPoly(startPoint, SharpNav.Geometry.Vector3.One);
                NavPoint endPoly = query.FindNearestPoly(endPoint, SharpNav.Geometry.Vector3.One);
                var path = new SharpNav.Pathfinding.Path();
                if(query.FindPath(ref startPoly, ref endPoly, new NavQueryFilter(), path))
                {
                    StraightPath straightPath = new StraightPath();
                    if(query.FindStraightPath(startPoint, endPoint, path, straightPath, new PathBuildFlags()))
                    {
                        navPath.Clear();
                        foreach(var item in navPathNodes)
                        {
                            Destroy(item);
                        }

                        for(int i = 0; i < straightPath.Count; i++)
                        {
                            var pathVert = straightPath[i];
                            var point = pathVert.Point;
                            navPath.Add(point.Position);

                            navPathNodes.Add(Instantiate(pathNodePrefab, point.Position.ToUnity(), Quaternion.identity) as GameObject);
                        }
                    }
                }
                else
                {
                    Debug.Log("Path not found between selected points!");
                }
            }
            started = true;
            startPoint = hit.point.ToPathfinding();
        }
    }

    void DrawNavmesh()
    {
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
                    var v1 = tile.Verts[index1];
                    var v2 = tile.Verts[index2];
                    Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), Color.red);
                }
            }
        }
    }

    void DrawPath()
    {
        for(int i = 0; i < navPath.Count - 1; i++)
        {
            var v1 = navPath[i];
            var v2 = navPath[i + 1];
            Debug.DrawLine(v1.ToUnity(), v2.ToUnity(), Color.green);
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
            meshFilters[i].gameObject.SetActive(false);//Note: If not deactivated it can cause some problems with raycasting, even if they don't have colliders (weird)
            i++;
        }

        parent.AddComponent<MeshFilter>();
        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);

        parent.AddComponent<MeshCollider>();
        MeshCollider meshCollider = parent.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;

        parent.AddComponent<MeshRenderer>();
        MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();
        MeshRenderer childRenderer = parent.GetComponentInChildren<MeshRenderer>();
        meshRenderer.material = childRenderer.material;
        meshRenderer.shadowCastingMode = childRenderer.shadowCastingMode;
        meshRenderer.receiveShadows = childRenderer.receiveShadows;
        meshRenderer.reflectionProbeUsage = childRenderer.reflectionProbeUsage;
        meshRenderer.useLightProbes = childRenderer.useLightProbes;

        parent.gameObject.SetActive(true);
    }

    SharpNav.Geometry.Vector3[] ConvertVectorsToPathfinding(UnityEngine.Vector3[] vectors)
    {
        var navVectors = new SharpNav.Geometry.Vector3[vectors.Length];
        for(int i = 0; i < vectors.Length; i++)
        {
            navVectors[i] = vectors[i].ToPathfinding();
        }
        return navVectors;
    }

    UnityEngine.Vector3[] ConvertVectorsToUnity(SharpNav.Geometry.Vector3[] vectors)
    {
        var navVectors = new UnityEngine.Vector3[vectors.Length];
        for(int i = 0; i < vectors.Length; i++)
        {
            navVectors[i] = vectors[i].ToUnity();
        }
        return navVectors;
    }

    void SaveNavMesh()
    {
        var stream = new FileStream(kTestNavMeshFile, FileMode.OpenOrCreate);
        NavMeshSerializer.Instance.Serialize(navMesh, new SystemBinaryWriter(stream));
    }

    void LoadNavMesh()
    {
        var stream = new FileStream(kTestNavMeshFile, FileMode.Open);
        navMesh = NavMeshParser.Instance.Parse(new SystemBinaryReader(stream));
    }
}
