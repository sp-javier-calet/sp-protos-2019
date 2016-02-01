#if NGUI
using UnityEngine;
using System.Collections;

namespace SocialPoint.GUIAnimation
{
    public class NGUIPanelGraphicObject : IGraphicObject
    {
        UIPanel _graphic;

        UIWidget _subWidget;

        UIWidget SubWidget
        {
            get
            {
                if (_subWidget == null)
                {
                    _subWidget = GUIAnimationUtility.GetComponentRecursiveDown<UIWidget> (_graphic.gameObject);
                }

                return _subWidget;
            }
        }

        public static NGUIPanelGraphicObject Load (Transform root, bool searchInChild)
        {
            UIPanel graphic = null;
            if (searchInChild)
            {
                graphic = root.GetComponentInChildren<UIPanel> ();
            }
            else
            {
                graphic = root.GetComponent<UIPanel> ();
            }

            NGUIPanelGraphicObject wrapper = null;
            if (graphic != null)
            {
                wrapper = new NGUIPanelGraphicObject (graphic);
            }
            return wrapper;
        }

        public NGUIPanelGraphicObject (UIPanel panel)
        {
            _graphic = panel;
        }

        public Color Color
        {
            get
            {
                if (SubWidget)
                {
                    return SubWidget.color;
                }

                return Color.white;
            }
            set
            {
                if (SubWidget)
                {
                    SubWidget.color = value;
//					Refresh();
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
//				Refresh();
            }
        }

        public Material Material
        {
            get
            {
                if (SubWidget)
                {
                    return SubWidget.material;
                }

                return null;
            }
            set
            {
                if (SubWidget)
                {
                    SubWidget.material = value;
                    Refresh ();
                }
            }
        }

        public void Refresh ()
        {
            _graphic.Refresh ();
        }
    }
}
#endif