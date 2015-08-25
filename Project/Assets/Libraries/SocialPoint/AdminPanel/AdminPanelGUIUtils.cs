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
        private const float DefaultButtonWidthPercent = 0.95f;
        private const int DefaultMargin = 5;


        private static RectTransform CreatePanelObject(AdminPanelLayout layout, Vector2 relativeSize)
        {
            GameObject panelObject = new GameObject("AdminPanel - Category Panel");
            var image = panelObject.AddComponent<Image>();
            panelObject.transform.SetParent(layout.Parent);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.pivot = new Vector2(0.0f, 1.0f);
            image.rectTransform.anchoredPosition = Vector3.zero;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            image.rectTransform.sizeDelta = new Vector2(-layout.Parent.rect.width * (1-relativeSize.x), -layout.Parent.rect.height * (1-relativeSize.y));
            image.rectTransform.anchoredPosition = layout.Position;
            image.color = new Color(1f, .3f, .3f, .5f);

            return image.rectTransform;
        }
        
        private static Vector2 CreateButtonObject(AdminPanelLayout layout, string label, UnityAction onClickAction)
        {
            var buttonObject = new GameObject("AdminPanel - Button");

            var image = buttonObject.AddComponent<Image>();
            buttonObject.transform.SetParent(layout.Parent);
            image.rectTransform.anchoredPosition = layout.Position;

            image.rectTransform.anchorMin = new Vector2(0, 0.5f);
            image.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.rect.width * -(1 - DefaultButtonWidthPercent), DefaultButtonHeight);
            image.color = new Color(1f, .3f, .3f, .5f);
            
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClickAction);

            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(image.rectTransform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, label);
            }

            return image.rectTransform.rect.size;//new Vector2(0, DefaultButtonHeight + DefaultMargin);
        }

        private static Vector2 CreateLabelObject(AdminPanelLayout layout, string label)
        {
            var textObject = new GameObject("Admin Panel - Label");
            textObject.transform.SetParent(layout.Parent);
            var text = textObject.AddComponent<Text>();
            text.rectTransform.anchoredPosition = layout.Position;
            text.rectTransform.sizeDelta = new Vector2(1.0f, 25.0f);
            text.rectTransform.anchorMin = new Vector2(0.0f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(1.0f, 0.5f);
            text.text = label;
            text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            return text.rectTransform.rect.size;
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
            layout.Advance(rectTransform.rect.width, rectTransform.rect.height);

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
            layout.Advance(0, DefaultMargin * marginMultiplier);
        }

        public static void CreateScrollView(AdminPanelLayout layout)
        {
            layout.Advance(CreateScrollViewObject(layout));
        }
    }
}
