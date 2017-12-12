using SocialPoint.Attributes;
using SocialPoint.Hardware;
using UnityEngine;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public class UISafeAreaViewController : MonoBehaviour 
    {
        const string kPersistentTag = "persistent";
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        public bool ShowGizmos;

        Rect _screenRect;
        Rect _gizmoRect;
        Texture _texture;

        IDeviceInfo _deviceInfo;
        IAttrStorage _storage;

        void Start()
        {
            _deviceInfo = Services.Instance.Resolve<IDeviceInfo>();
            _storage = Services.Instance.Resolve<IAttrStorage>(kPersistentTag);

            _screenRect = _deviceInfo == null ? new Rect(0f, 0f, Screen.width, Screen.height) : new Rect(0f, 0f, _deviceInfo.ScreenSize.x, _deviceInfo.ScreenSize.y);
            ApplyGizmoSafeArea(_screenRect);

            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            ApplySafeArea(GetSafeAreaRect());
        }
 
        public void ApplySafeArea(Rect rect)
        {
            if(rect != Rect.zero)
            {
#if NGUI
                SocialPoint.Base.Log.w("We have not NGUI libraries in base game");
#else
                var anchorMin = rect.position;
                var anchorMax = rect.position + rect.size;
                anchorMin.x /= _screenRect.width;
                anchorMin.y /= _screenRect.height;
                anchorMax.x /= _screenRect.width;
                anchorMax.y /= _screenRect.height;

                var rectTransform = GetComponent<RectTransform>();
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
#endif
            }
        }

        Rect GetSafeAreaRect()
        {
            float x = 0f;
            float y = 0f;
            float w = _screenRect.width;
            float h = _screenRect.height;

#if ADMIN_PANEL
            if(_storage != null)
            {
                var xAttr = _storage.Load(kCustomX);
                if(xAttr != null)
                {
                    x = xAttr.AsValue.ToFloat();  
                }

                var yAttr = _storage.Load(kCustomY);
                if(yAttr != null)
                {
                    y = yAttr.AsValue.ToFloat(); 
                }

                var widthAttr = _storage.Load(kCustomWidth);
                if(widthAttr != null)
                {
                    w = widthAttr.AsValue.ToFloat(); 
                }

                var heightAttr = _storage.Load(kCustomHeight);
                if(heightAttr != null)
                {
                    h = heightAttr.AsValue.ToFloat(); 
                }
            }
            else
            {
                return _deviceInfo.SafeAreaRectSize; 
            }
#else
            return DeviceInfo.SafeAreaRectSize;
#endif
        
            return new Rect(x, y, w, h);
        }
            
        static Texture CreateSafeAreaTexture()
        {
            // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            // set the pixel values
            texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.25f));
            texture.SetPixel(1, 0, new Color(1f, 1f, 1f, 0.25f));
            texture.SetPixel(0, 1, new Color(1f, 1f, 1f, 0.25f));
            texture.SetPixel(1, 1, new Color(1f, 1f, 1f, 0.25f));

            // Apply all SetPixel calls
            texture.Apply();

            return texture;
        }

        public void ApplyGizmoSafeArea(Rect rect)
        {
            _gizmoRect = rect;
        }

        void OnDrawGizmos()
        {
            if(ShowGizmos)
            {
                if(_texture == null)
                {
                    _texture = CreateSafeAreaTexture();
                }

                Gizmos.DrawGUITexture(_gizmoRect, _texture);
            }
        }
    }
}
