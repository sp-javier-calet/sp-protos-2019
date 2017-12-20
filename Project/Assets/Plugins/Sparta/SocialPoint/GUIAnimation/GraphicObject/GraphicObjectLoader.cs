using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Load the Graphic Object, compatible with ngui and native ui
    public static class GraphicObjectLoader
    {
        public static IGraphicObject Load(Transform trans, bool recursive)
        {
            #if NGUI
            return LoadNGUI(trans, recursive);
            #else
            return LoadNative(trans, recursive);
            #endif
        }

        static IGraphicObject LoadNative(Transform trans, bool recursive)
        {
            return NativeGraphicObject.Load(trans, recursive);
        }

        #if NGUI
        static IGraphicObject LoadNGUI(Transform trans, bool recursive)
        {
            IGraphicObject graphic = NGUIPanelGraphicObject.Load(trans, false);

            if(graphic == null)
            {
                graphic = NGUIWidgetGraphicObject.Load(trans, recursive);
            }

            if(graphic == null)
            {
                graphic = NGUIPanelGraphicObject.Load(trans, recursive);
            }
            return graphic;
        }
        #endif
    }
}
