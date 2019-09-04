
using System.Collections.Generic;
using UnityEngine;

public class GSB_SceneManager : MonoBehaviour
{
    public static GSB_SceneManager Instance = null;

    public GSB_PlayerController Player;
    public GameObject HealthBox = null;
    public GameObject AmmoBox = null;
    public GameObject TimeBar = null;
    public RectTransform TimeBarFiller = null;
    public MeshFilter SelectionMesh;
    public List<LineRenderer> SelectionLine = new List<LineRenderer>();
    public float SlowDown = 0.1f;

    public int HealthMax = 18;
    public int AmmoMax = 4;
    public float AmmoRegenerationTime = 0.75f;
    public int TargetTimeMS = 2000;

    void Awake()
    {
        Instance = this;
    }

    public void OnPressedDown()
    {
        if (Player != null)
        {
            Player.OnPressedDown();
        }
    }
    public void OnPressedUp()
    {
        if (Player != null)
        {
            Player.OnPressedUp();
        }
    }
}
