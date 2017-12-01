using SocialPoint.Attributes;
using SocialPoint.Hardware;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public class UISafeAreaController : MonoBehaviour 
    {
        const string kPersistentTag = "persistent";
        const string kShowSafeArea = "ShowSafeArea";
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        public IAttrStorage Storage;
        public IDeviceInfo DeviceInfo;

        public Rect GetSafeAreaRect()
        {
            if(Storage != null)
            {
                Storage.Remove(kShowSafeArea);
                Storage.Remove(kCustomX);
                Storage.Remove(kCustomY);
                Storage.Remove(kCustomWidth);
                Storage.Remove(kCustomHeight);
            }

            float x = 0f;
            float y = 0f;
            float w = DeviceInfo == null ? Screen.width : DeviceInfo.ScreenSize.x;
            float h = DeviceInfo == null ? Screen.height : DeviceInfo.ScreenSize.y;

#if ADMIN_PANEL
            if(Storage != null)
            {
                var attr = Storage.Load(kShowSafeArea);
                if(attr != null && attr.AsValue.ToBool())
                {
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
                }
            }
            else
            {
                return DeviceInfo.SafeAreaRectSize; 
            }
#else
            return DeviceInfo.SafeAreaRectSize;
#endif
        
            return new Rect(x, y, w, h);
        }
    }
}
