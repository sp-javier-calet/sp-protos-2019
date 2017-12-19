#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        protected const int DefaultMargin = 5;
        protected const int DefaultPadding = 5;
        protected const int DefaultScrollPadding = 10;
        protected const int DefaultFontSize = 22;
        protected const int TextAreaFontSize = 10;
        protected const int PanelTitleFontSize = 10;
        protected const int PanelHeaderHeight = 12;
        protected const int DefaultLabelHeight = 30;
        protected const int DefaultLayoutWeight = 1;

        protected static readonly Color BackgroundColor = new Color(.3f, .3f, .3f, .5f);
        protected static readonly Color ForegroundColor = new Color(.5f, .5f, .5f, .7f);
        protected static readonly Color InputColor = new Color(1.0f, 1.0f, 1.0f, .8f);
        protected static readonly Color DisabledColor = new Color(.15f, .15f, .15f, .8f);

        protected static readonly Color StatusEnabledColor = new Color(.3f, .8f, .3f, .8f);
        protected static readonly Color StatusDisabledColor = new Color(.8f, .3f, .3f, .8f);
        protected static readonly Color CloseButtonColor = new Color(.8f, .3f, .3f, .8f);

        static Font _defaultFont;

        public static Font DefaultFont
        { 
            get
            {
                if(_defaultFont == null)
                {
                    _defaultFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                }
                return _defaultFont;
            }
        }

        protected RectTransform CreateUIObject(string name, Transform parent)
        {
            var gObject = new GameObject(name);
            gObject.transform.SetParent(parent, false);
            gObject.layer = parent.gameObject.layer;
            var rectTransform = gObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.localScale = Vector3.one;

            return rectTransform;
        }

        public void CreateMargin(int multiplier = 1)
        {
            var rectTransform = CreateUIObject("Admin Panel - Margin", Parent);
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = DefaultMargin * multiplier;
            layoutElement.minWidth = DefaultMargin * multiplier;
        }
    }
}

#endif
