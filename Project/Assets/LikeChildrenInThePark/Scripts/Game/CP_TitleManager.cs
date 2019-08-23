
using SocialPoint.Rendering.Components;
using UnityEngine;

public class CP_TitleManager : MonoBehaviour
{
    public BCSHModifier TitleBCSH = null;

    public void OnPressed1Player()
    {
        CP_GameManager.Instance.SetGameState(CP_GameManager.GameState.E_PLAYING_1_PLAYER);
    }

    public void OnPressed4Versus()
    {
    }

    void Update()
    {
        if(TitleBCSH != null)
        {
            TitleBCSH.Hue += 0.01f;
            if(TitleBCSH.Hue > 1.0f)
            {
                TitleBCSH.Hue -= 1.0f;
            }
        }
    }
}
