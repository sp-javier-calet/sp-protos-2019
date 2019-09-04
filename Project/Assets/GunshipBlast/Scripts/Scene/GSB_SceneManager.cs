
using UnityEngine;

public class GSB_SceneManager : MonoBehaviour
{
    public static GSB_SceneManager Instance = null;

    public GSB_PlayerController Player;
    public MeshFilter SelectionMesh;
    public LineRenderer SelectionLine;
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
