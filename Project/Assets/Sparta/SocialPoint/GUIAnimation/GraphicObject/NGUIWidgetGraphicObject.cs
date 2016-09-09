#if NGUI
using UnityEngine;
using System.Collections;

namespace SocialPoint.GUIAnimation
{
    public sealed class NGUIWidgetGraphicObject : IGraphicObject
    {
        UIWidget _graphic;

        public static NGUIWidgetGraphicObject Load (Transform root, bool searchInChild)
        {
            UIWidget graphic = null;
            if (searchInChild)
            {
                graphic = GUIAnimationUtility.GetComponentRecursiveDown<UIWidget> (root.gameObject);
            }
            else
            {
                graphic = root.GetComponent<UIWidget> ();
            }
			
            NGUIWidgetGraphicObject wrapper = null;
            if (graphic != null)
            {
                wrapper = new NGUIWidgetGraphicObject (graphic);
            }
            return wrapper;
        }

        public NGUIWidgetGraphicObject (UIWidget widget)
        {
            _graphic = widget;
        }

        public Color Color
        {
            get
            {
                return _graphic.color;
            }
            set
            {
                Color color = _graphic.color;
                color.r = value.r;
                color.g = value.g;
                color.b = value.b;
                color.a *= value.a;
                _graphic.color = color;
s
                if (!Application.isPlaying)
                {
                    Refresh ();
                }
            }
        }

        public float Alpha
        {
            get
            {
                return _graphic.alpha;
            }
            set
            {
                _graphic.alpha = value;
                if (!Application.isPlaying)
                {
                    Refresh ();
                }
            }
        }

        public Material Material
        {
            get
            {
                return _graphic.material;
            }
            set
            {
                _graphic.material = value;
                Refresh ();
            }
        }

        public void Refresh ()
        {
            UIPanel panel = _graphic.GetComponentInParent<UIPanel> ();
            if (panel)
            {
                panel.Refresh ();
                panel.Update ();
            }
        }
    }
}
#endif