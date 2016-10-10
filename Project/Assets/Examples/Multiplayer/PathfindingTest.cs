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
    StraightPath straightPath;
    SharpNav.Geometry.Vector3 startPoint = SharpNav.Geometry.Vector3.Zero;
    bool started = false;

    List<GameObject> navPathNodes = new List<GameObject>();

    IPathfindingDebugger _debugger;

    // Use this for initialization
    void Start()
    {
        _debugger = new PathfindingUnityDebugger();

        Mesh map = PathfindingUnityUtils.CombineSubMeshes(gameObject);
        SetMesh(gameObject, map);

        if(!loadFromFile)
        {
            //use the default generation settings
            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = 1.7f;
            settings.AgentRadius = 0.5f;

            //generate the mesh
            navMesh = PathfindingUnityUtils.CreateNavMesh(map, settings);
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
        if(navMesh != null)
        {
            _debugger.DrawNavMesh(navMesh);
        }

        if(straightPath != null)
        {
            _debugger.DrawStraightPath(straightPath);
        }
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
                    straightPath = new StraightPath();
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

    void SetMesh(GameObject parent, Mesh mesh)
    {
        //Hide children
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        for(int i = 0; i < meshFilters.Length; i++)
        {
            //Note: If not deactivated it can cause some problems with raycasting, even if they don't have colliders (weird)
            meshFilters[i].gameObject.SetActive(false);
        }

        //Add mesh components to parent object
        parent.AddComponent<MeshFilter>();
        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

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
