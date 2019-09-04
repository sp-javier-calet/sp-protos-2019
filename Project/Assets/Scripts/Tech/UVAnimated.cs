
using UnityEngine;
using UnityEngine.UI;

public class UVAnimated : MonoBehaviour
{
    public Vector2 UVSpeed = Vector2.zero;

    Renderer[] _renderers = null;
    RawImage _rawImage = null;

    void Awake()
    {
        _renderers = GetComponents<Renderer>();
        _rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if(_renderers != null)
        {
            for(var i = 0; i < _renderers.Length; ++i)
            {
                if(_renderers[i].material != null)
                {
                    _renderers[i].material.mainTextureOffset += (UVSpeed * Time.timeScale);
                }
            }
        }

        if(_rawImage != null)
        {
            Rect r = _rawImage.uvRect;
            r.x += (UVSpeed.x * Time.timeScale);
            r.y += (UVSpeed.y * Time.timeScale);

            _rawImage.uvRect = r;
        }
    }
}
