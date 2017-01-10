#if NGUI
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class NGUIPanelGraphicObject : IGraphicObject
    {
        readonly UIPanel _graphic;

        UIWidget _subWidget;

        UIWidget SubWidget
        {
            get
            {
                if(_subWidget == null)
                {
                    _subWidget = GUIAnimationUtility.GetComponentRecursiveDown<UIWidget>(_graphic.gameObject);
                }

                return _subWidget;
            }
        }

        public static NGUIPanelGraphicObject Load(Transform root, bool searchInChild)
        {
            UIPanel graphic;
            graphic = searchInChild ? root.GetComponentInChildren<UIPanel>() : root.GetComponent<UIPanel>();

            NGUIPanelGraphicObject wrapper = null;
            if(graphic != null)
            {
                wrapper = new NGUIPanelGraphicObject(graphic);
            }
            return wrapper;
        }

        public NGUIPanelGraphicObject(UIPanel panel)
        {
            _graphic = panel;
        }

        public Color Color
        {
            get
            {
                return SubWidget ? SubWidget.color : Color.white;
            }
            set
            {
                if(SubWidget)
                {
                    SubWidget.color = value;
                    Refresh();
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
                Refresh();
            }
        }

        public Material Material
        {
            get
            {
                return SubWidget ? SubWidget.material : null;

            }
            set
            {
                if(SubWidget)
                {
                    SubWidget.material = value;
                    Refresh();
                }
            }
        }

        public void Refresh()
        {
            _graphic.Refresh();
        }
    }
}
#endif