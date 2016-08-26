using UnityEngine;

public sealed class UI3DCamera : MonoBehaviour
{
    private bool _revertFogState = false;

    void OnPreRender()
    {
        _revertFogState = RenderSettings.fog;
        RenderSettings.fog = false;
    }

    void OnPostRender()
    {
        RenderSettings.fog = _revertFogState;
    }
}
