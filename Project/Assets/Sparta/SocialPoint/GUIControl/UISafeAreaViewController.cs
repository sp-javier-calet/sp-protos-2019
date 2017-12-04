using SocialPoint.Attributes;
using SocialPoint.Hardware;
using UnityEngine;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public class UISafeAreaViewController : MonoBehaviour 
    {
        const string kPersistentTag = "persistent";
        const string kShowSafeArea = "ShowSafeArea";
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        public bool ShowGizmos;

        IDeviceInfo _deviceInfo;
        IAttrStorage _storage;
        Texture _texture;

        void Start()
        {
            _deviceInfo = Services.Instance.Resolve<IDeviceInfo>();
            if(_deviceInfo != null)
            {
                ApplySafeArea(_deviceInfo.SafeAreaRectSize);
            }

            _storage = Services.Instance.Resolve<IAttrStorage>(kPersistentTag);
        }

        public void ApplySafeArea(Rect rect)
        {
            if(rect != Rect.zero)
            {
                #if NGUI

                #else
                var anchorMin = rect.position;
                var anchorMax = rect.position + rect.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                var rectTransform = GetComponent<RectTransform>();
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                #endif
            }
        }

        public Rect GetSafeAreaRect()
        {
            float x = 0f;
            float y = 0f;
            float w = _deviceInfo == null ? Screen.width : _deviceInfo.ScreenSize.x;
            float h = _deviceInfo == null ? Screen.height : _deviceInfo.ScreenSize.y;

#if ADMIN_PANEL
            if(_storage != null)
            {
                var attr = _storage.Load(kShowSafeArea);
                if(attr != null && attr.AsValue.ToBool())
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
            
        Texture CreateSafeAreaTexture()
        {
            // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

            // set the pixel values
            texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(1, 0, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(0, 1, new Color(1f, 1f, 1f, 0.5f));
            texture.SetPixel(1, 1, new Color(1f, 1f, 1f, 0.5f));

            // Apply all SetPixel calls
            texture.Apply();

            return texture;
        }
            
        void OnDrawGizmos()
        {
            if(ShowGizmos)
            {
                if(_texture == null)
                {
                    _texture = CreateSafeAreaTexture();
                }

                Gizmos.DrawGUITexture(new Rect(132f, 63f, 2172f, 1062f), _texture);
            }
        }
    }
}
