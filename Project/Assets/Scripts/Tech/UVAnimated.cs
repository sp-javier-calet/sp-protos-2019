
using UnityEngine;

public class UVAnimated : MonoBehaviour
{
    public Vector2 UVSpeed = Vector2.zero;

    Renderer[] _renderers = null;

    void Awake()
    {
        _renderers = GetComponents<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_renderers != null)
        {
            for(var i = 0; i < _renderers.Length; ++i)
            {
                if(_renderers[i].material != null)
                {
                    _renderers[i].material.mainTextureOffset += UVSpeed;
                }
            }
        }
    }
}
