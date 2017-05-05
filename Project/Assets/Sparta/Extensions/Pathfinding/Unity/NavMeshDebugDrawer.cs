using UnityEngine;
using System;
using SharpNav;
using System.Collections;
using SharpNav.Pathfinding;
using SocialPoint.Exporter;
using SocialPoint.IO;
using SocialPoint.Pathfinding;
using SocialPoint.Utils;

public class NavMeshDebugDrawer : MonoBehaviour
{
    public float AgentHeight = 2.0f;
    public float AgentRadius = 0.6f;

    public Color Color = Color.red;
    public bool DebugDraw = true;

    SharpNav.TiledNavMesh _navMesh;

    public SharpNav.TiledNavMesh NavMesh
    {
        set
        {
            _navMesh = value;
        }
        get
        {
            return _navMesh;
        }
    }

    PathfindingUnityDebugger _debugger;
    Func<NavPoly, bool> _conditionalDraw;

    // Use this for initialization
    void Awake()
    {
        _debugger = new PathfindingUnityDebugger();
        HideChildren();
    }

    void Update()
    {
        if(DebugDraw)
        {
            DebugDrawNavMesh();
        }
    }

    void DebugDrawNavMesh()
    {
        if(_navMesh != null)
        {
            _debugger.SetColor(Color.r, Color.g, Color.b, Color.a);
            _debugger.DrawNavMesh(_navMesh);
        }
    }

    void HideChildren()
    {
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            var child = gameObject.transform.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }
}
