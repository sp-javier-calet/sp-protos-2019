using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.GUIAnimation
{
    public sealed class NativeGraphicObject : IGraphicObject
    {
        Graphic _graphic;

        public static NativeGraphicObject Load(Transform root, bool searchInChild)
        {
            Graphic graphic;
            graphic = searchInChild ? root.GetComponentInChildren<Graphic>() : root.GetComponent<Graphic>();

            NativeGraphicObject wrapper = null;
            if(graphic != null)
            {
                wrapper = new NativeGraphicObject(graphic);
            }
            return wrapper;
        }

        public NativeGraphicObject(Graphic panel)
        {
            _graphic = panel;
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
            }
        }

        public float Alpha
        {
            get
            {
                return _graphic.color.a;
            }
            set
            {
                Color color = _graphic.color;
                color.a = value;
                _graphic.color = color;
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
            }
        }

        public void Refresh()
        {
            // It is not needed to do anything with the native UI system to refresh the UI widget
        }
    }
}
