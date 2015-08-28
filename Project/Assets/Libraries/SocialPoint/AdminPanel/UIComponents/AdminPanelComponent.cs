using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout {
        
        protected const int DefaultMargin = 3;
        protected const int DefaultPadding = 3;
        protected const int DefaultFontSize = 14;
        protected const int DefaultLabelHeight = 20;

        protected static readonly Color BackgroundColor = new Color(.3f, .3f, .3f, .5f);
        protected static readonly Color ForegroundColor = new Color(.5f, .5f, .5f, .7f);

        protected static readonly Color StatusEnabledColor = new Color(.3f, .8f, .3f, .8f);
        protected static readonly Color StatusDisabledColor = new Color(.8f, .3f, .3f, .8f);

        protected RectTransform CreateUIObject(string name, RectTransform parent)
        {
            GameObject gObject = new GameObject(name);
            gObject.transform.SetParent(parent);
            
            RectTransform rectTrans = gObject.AddComponent<RectTransform>();
            rectTrans.pivot = Vector2.up;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            rectTrans.anchorMin = Vector2.zero;
            rectTrans.anchorMax = Vector2.one;

            return rectTrans;
        }
        
    }
}
