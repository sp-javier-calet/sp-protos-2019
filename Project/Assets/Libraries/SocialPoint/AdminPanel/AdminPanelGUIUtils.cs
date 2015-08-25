using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;


namespace SocialPoint.AdminPanel
{
    public class AdminPanelGUIUtils  
    {
        private const int DefaultFontSize = 20;
        private const int DefaultButtonHeight = 25;
        private const int DefaultMargin = 5;


        private static RectTransform CreatePanelObject(AdminPanelLayout layout, Vector2 relativeSize)
        {
            GameObject panelObject = new GameObject("AdminPanel - Panel");

            var rectTransform = panelObject.AddComponent<RectTransform>();
            panelObject.transform.SetParent(layout.Parent);

            rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            rectTransform.pivot = new Vector2(0.0f, 1.0f);

            rectTransform.anchoredPosition = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(layout.Parent.rect.width * relativeSize.x, layout.Parent.rect.height * relativeSize.y);
            rectTransform.anchoredPosition = layout.Position;

            var image = panelObject.AddComponent<Image>();
            image.color = new Color(.3f, .3f, .3f, .5f);


            return rectTransform;
        }
        
        private static Vector2 CreateButtonObject(AdminPanelLayout layout, string label, UnityAction onClickAction)
        {
            var buttonObject = new GameObject("AdminPanel - Button");
            buttonObject.transform.SetParent(layout.Parent);

            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            rectTransform.anchorMin = new Vector2(0.05f, 1.0f);
            rectTransform.anchorMax = new Vector2(0.95f, 1.0f);
            rectTransform.sizeDelta = new Vector2(1.0f, DefaultButtonHeight);

            rectTransform.anchoredPosition = layout.Position;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(.5f, .5f, .5f, .5f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClickAction);

            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(rectTransform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, label);
            }

            return rectTransform.rect.size;
        }

        private static Vector2 CreateLabelObject(AdminPanelLayout layout, string label)
        {
            var textObject = new GameObject("Admin Panel - Label");
            textObject.transform.SetParent(layout.Parent);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            rectTransform.sizeDelta = new Vector2(1.0f, 25.0f);
            rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            rectTransform.anchorMax = new Vector2(1.0f, 1.0f);

            rectTransform.anchoredPosition = layout.Position;
            
            var text = textObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            return rectTransform.rect.size;
        }

        private static Vector2 CreateScrollViewObject(AdminPanelLayout layout)
        {
            GameObject scrollObject = new GameObject("AdminPanel - Scroll View");
            scrollObject.transform.SetParent(layout.Parent);
            RectTransform scrollRectTrans = scrollObject.AddComponent<RectTransform>();
            scrollRectTrans.anchorMin = new Vector2(0.0f, 0.0f);
            scrollRectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            scrollRectTrans.pivot = new Vector2(0.5f, 1.0f);
            scrollRectTrans.localPosition = Vector2.zero;
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = Vector2.zero;
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            
            
            GameObject scrollContentObject = new GameObject("AdminPanel - Scroll Content");
            scrollContentObject.transform.SetParent(scrollObject.transform);
            RectTransform rectTrans = scrollContentObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
            rectTrans.anchorMax = new Vector2(1.0f, 0.5f);
            rectTrans.offsetMin = new Vector2(0.0f, -50.0f);
            rectTrans.offsetMax = new Vector2(0.0f, 0.0f);
            rectTrans.pivot = new Vector2(0.5f, 1.0f);
            rectTrans.localPosition = Vector2.zero;
            scroll.content = rectTrans;

            return scrollRectTrans.rect.size;
        }

        public static RectTransform CreatePanel(AdminPanelLayout layout, Vector2 relativeSize)
        {
            RectTransform rectTransform = AdminPanelGUIUtils.CreatePanelObject(layout, relativeSize);
            layout.Advance(rectTransform.rect.size);

            return rectTransform;
        }

        public static void CreateButton(AdminPanelLayout layout, string label, UnityAction onClickAction)
        {
            layout.Advance(AdminPanelGUIUtils.CreateButtonObject(layout, label, onClickAction));
        }
    
        public static void CreateLabel(AdminPanelLayout layout, string label)
        {
            layout.Advance(AdminPanelGUIUtils.CreateLabelObject(layout, label));
        }

        public static void CreateMargin(AdminPanelLayout layout, float marginMultiplier = 2.0f)
        {
            layout.Advance(DefaultMargin * marginMultiplier, DefaultMargin * marginMultiplier);
        }

        public static void CreateScrollView(AdminPanelLayout layout)
        {
            layout.Advance(CreateScrollViewObject(layout));
        }
    }
}
