using UnityEngine;
using System.IO;
using SharpNav;
using System.Collections;
using SharpNav.Pathfinding;
using SocialPoint.IO;
using SocialPoint.Pathfinding;

public class NavMeshToFile : MonoBehaviour
{
    public float AgentHeight = 2.0f;
    public float AgentRadius = 0.6f;

    public string FilePath = "";

    SharpNav.TiledNavMesh _navMesh;
    IPathfindingDebugger _debugger;

    // Use this for initialization
    void Start()
    {
        Mesh map = PathfindingUnityUtils.CombineSubMeshes(gameObject);

        //Create NavMesh
        var settings = NavMeshGenerationSettings.Default;//Use the default generation settings
        settings.AgentHeight = AgentHeight;
        settings.AgentRadius = AgentRadius;
        _navMesh = PathfindingUnityUtils.CreateNavMesh(map, settings);

        SaveNavMesh(_navMesh);
        _debugger = new PathfindingUnityDebugger();
    }

    void Update()
    {
        DebugDrawNavMesh();
    }

    void SaveNavMesh(TiledNavMesh navMesh)
    {
        try
        {
            var stream = new FileStream(FilePath, FileMode.OpenOrCreate);
            NavMeshSerializer.Instance.Serialize(navMesh, new SystemBinaryWriter(stream));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error trying to save NavMesh: " + e.Message);
        }
    }

    void DebugDrawNavMesh()
    {
        if (_navMesh != null)
        {
            Color c = Color.red;
            _debugger.SetColor(c.r, c.g, c.b, c.a);
            _debugger.DrawNavMesh(_navMesh);
        }
    }
}
