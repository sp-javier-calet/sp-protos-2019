
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
