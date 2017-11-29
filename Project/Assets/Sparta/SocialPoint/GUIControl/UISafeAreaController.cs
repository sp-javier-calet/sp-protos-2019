using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SocialPoint.Attributes;
using UnityEngine;
using SocialPoint.Dependency;

public class UISafeAreaController : MonoBehaviour 
{
    const string kPersistentTag = "persistent";
    const string kShowSafeArea = "ShowSafeArea";
    const string kCustomX = "CustomSafeAreaX";
    const string kCustomY = "CustomSafeAreaY";
    const string kCustomWidth = "CustomSafeAreaWidth";
    const string kCustomHeight = "CustomSafeAreaHeight";

    public IAttrStorage Storage;

#if UNITY_IOS
    [DllImport("__Internal")]
    private extern static void GetSafeAreaImpl(out float x, out float y, out float w, out float h);
#endif

#if ADMIN_PANEL
    public bool ShowSafearea;

    Texture _safeAreaGizmoTexture;
    Color _safeAreaGizmoColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);

    Texture CreateOnDrawGizmosTexture()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        // set the pixel values
        texture.SetPixel(0, 0, _safeAreaGizmoColor);
        texture.SetPixel(1, 0, _safeAreaGizmoColor);
        texture.SetPixel(0, 1, _safeAreaGizmoColor);
        texture.SetPixel(1, 1, _safeAreaGizmoColor);

        // Apply all SetPixel calls
        texture.Apply();

        return texture;
    }

    void OnDrawGizmos() 
    {
        if(_safeAreaGizmoTexture == null)
        {
            _safeAreaGizmoTexture = CreateOnDrawGizmosTexture();
        }

        if(Storage != null)
        {
            var attr = Storage.Load(kShowSafeArea);
            if(attr != null && attr.AsValue.ToBool())
            {
                Gizmos.DrawGUITexture(GetStoredSafeArea(), _safeAreaGizmoTexture);
            }
        }
        else if(ShowSafearea)
        {
            Gizmos.DrawGUITexture(GetIphoneXSafeArea(), _safeAreaGizmoTexture);
        }
    }
#endif

    Rect GetStoredSafeArea()
    {
        float x = 0.0f;
        float y = 0.0f;
        float w = Screen.width;
        float h = Screen.height;

        var xAttr = Storage.Load(kCustomX);
        if(xAttr != null)
        {
            x = xAttr.AsValue.ToFloat();  
        }

        var yAttr = Storage.Load(kCustomY);
        if(yAttr != null)
        {
            y = yAttr.AsValue.ToFloat(); 
        }

        var widthAttr = Storage.Load(kCustomWidth);
        if(widthAttr != null)
        {
            w = widthAttr.AsValue.ToFloat(); 
        }

        var heightAttr = Storage.Load(kCustomHeight);
        if(heightAttr != null)
        {
            h = heightAttr.AsValue.ToFloat(); 
        }

        return new Rect(x, y, w, h);
    }

    public Rect GetIphoneXSafeArea()
    {
        // Iphone X save area
        return new Rect(132.0f, 63.0f, 2172.0f, 1062.0f);
    }

    public Rect GetScreenArea()
    {
        return new Rect(0.0f, 0.0f, Screen.width, Screen.height);
    }

    public Rect GetSafeArea()
    {
        float x = 0.0f;
        float y = 0.0f;
        float w = Screen.width;
        float h = Screen.height;

#if ADMIN_PANEL
        if(Storage != null)
        {
            var attr = Storage.Load(kShowSafeArea);
            if(attr != null && attr.AsValue.ToBool())
            {
                return GetStoredSafeArea();
            }
        }

#elif UNITY_IOS && !UNITY_EDITOR
        GetSafeAreaImpl(out x, out y, out w, out h);
#endif

        return new Rect(x, y, w, h);
    }
}
