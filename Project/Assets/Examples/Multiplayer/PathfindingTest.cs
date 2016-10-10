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
    public GameObject PathNodePrefab;
    public GameObject PathEdgePrefab;

    /*If true, NavMesh will be serialized and then parsed again before using it.
     * Use this to test serialization.
     * */
    public bool UseSerialization = false;

    SharpNav.TiledNavMesh _navMesh;
    StraightPath _straightPath;
    IPathfindingDebugger _debugger;

    Pathfinder _pathfinder;

    SharpNav.Geometry.Vector3 _startPoint = SharpNav.Geometry.Vector3.Zero;
    bool _started = false;

    //Visuals
    List<GameObject> _visualPathNodes = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        Mesh map = PathfindingUnityUtils.CombineSubMeshes(gameObject);
        SetMesh(gameObject, map);

        //Create NavMesh
        var settings = NavMeshGenerationSettings.Default;//Use the default generation settings
        settings.AgentHeight = 1.7f;
        settings.AgentRadius = 0.5f;
        var generatedNavMesh = PathfindingUnityUtils.CreateNavMesh(map, settings);

        if(!UseSerialization)
        {
            _navMesh = generatedNavMesh;
        }
        else
        {
            var data = SaveNavMesh(generatedNavMesh);
            _navMesh = LoadNavMesh(data);
        }

        _pathfinder = new Pathfinder(_navMesh);
        _debugger = new PathfindingUnityDebugger();
    }

    void Update()
    {
        if(_navMesh != null)
        {
            _debugger.DrawNavMesh(_navMesh);
        }

        if(_straightPath != null)
        {
            _debugger.DrawStraightPath(_straightPath);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        UnityEngine.Ray clickRay = eventData.pressEventCamera.ScreenPointToRay(eventData.pressPosition);
        UnityEngine.RaycastHit hit;
        if(Physics.Raycast(clickRay, out hit, float.MaxValue))
        {
            if(_started)
            {
                var endPoint = hit.point.ToPathfinding();
                var extents = SharpNav.Geometry.Vector3.One;

                _straightPath = _pathfinder.GetPath(_startPoint, endPoint, extents);

                UpdateVisualPath();
            }
            _started = true;
            _startPoint = hit.point.ToPathfinding();
        }
    }

    void SetMesh(GameObject parent, Mesh mesh)
    {
        //Hide children. If not deactivated it can cause some problems with raycasting, even if they don't have colliders (weird)
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        for(int i = 0; i < meshFilters.Length; i++)
        {
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

    void UpdateVisualPath()
    {
        //Clear previous objects
        for(int i = 0; i < _visualPathNodes.Count; i++)
        {
            Destroy(_visualPathNodes[i]);
        }

        //Create new ones
        for(int i = 0; i < _straightPath.Count; i++)
        {
            var pathVert = _straightPath[i];
            var point = pathVert.Point;
            _visualPathNodes.Add(Instantiate(PathNodePrefab, point.Position.ToUnity(), Quaternion.identity) as GameObject);
        }

        for(int i = 0; i < _straightPath.Count - 1; i++)
        {
            var pathVert1 = _straightPath[i];
            var pathVert2 = _straightPath[i + 1];
            var point1 = pathVert1.Point.Position.ToUnity();
            var point2 = pathVert2.Point.Position.ToUnity();
            var edge = Instantiate(PathEdgePrefab, point1, Quaternion.identity) as GameObject;
            edge.transform.LookAt(point2);
            edge.transform.localScale = new UnityEngine.Vector3(1, 1, UnityEngine.Vector3.Distance(point1, point2) * 0.5f);
            _visualPathNodes.Add(edge);
        }
    }

    byte[] SaveNavMesh(TiledNavMesh navMesh)
    {
        var stream = new MemoryStream();
        NavMeshSerializer.Instance.Serialize(navMesh, new SystemBinaryWriter(stream));
        return stream.ToArray();
    }

    TiledNavMesh LoadNavMesh(byte[] data)
    {
        var stream = new MemoryStream(data);
        return NavMeshParser.Instance.Parse(new SystemBinaryReader(stream));
    }
}
