
using UnityEngine;
using UnityEngine.UI;

public class CP_ProgressBarPlayer : MonoBehaviour
{
    public RectTransform IconTransform;
    public Camera CameraRendering = null;
    public RawImage ImageTarget = null;

    RenderTexture _renderTexture;

    void Awake()
    {
        if(CameraRendering != null)
        {
            _renderTexture = new RenderTexture(160, 256, 16, RenderTextureFormat.ARGB32);

            CameraRendering.targetTexture = _renderTexture;

            if(ImageTarget != null)
            {
                ImageTarget.texture = _renderTexture;
            }
        }
    }

    public void SetIconPosition(Vector2 position)
    {
        if(IconTransform != null)
        {
            IconTransform.anchoredPosition = position;
        }
    }
}
