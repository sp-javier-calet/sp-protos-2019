using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;


namespace SocialPoint.AdminPanel
{
    public class AdminPanelGUIUtils  
    {
        private const int DefaultFontSize = 14;
        private const int DefaultButtonHeight = 25;
        private const int DefaultMargin = 5;

        private static readonly Vector2 Margin = new Vector2(DefaultMargin, DefaultMargin);


        private static RectTransform CreatePanelObject(AdminPanelLayout layout, Vector2 relativeSize)
        {
            GameObject panelObject = new GameObject("AdminPanel - Panel");

            var rectTransform = panelObject.AddComponent<RectTransform>();
            panelObject.transform.SetParent(layout.Parent);

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = Vector2.up;

            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.sizeDelta = new Vector2(layout.Parent.rect.width * relativeSize.x, layout.Parent.rect.height * relativeSize.y);

            rectTransform.anchoredPosition = layout.Position;

            var image = panelObject.AddComponent<Image>();
            image.color = new Color(.3f, .3f, .3f, .5f);


            return rectTransform;
        }
        
        private static Vector2 CreateButtonObject(AdminPanelLayout layout, string label, Action onClickAction, Vector2 relativeSize)
        {
            if(relativeSize.y != 1.0f)
            {
                Debug.LogWarning("AdminPanel Button - Relative Y size is ignored");
            }

            var buttonObject = new GameObject("AdminPanel - Button");
            buttonObject.transform.SetParent(layout.Parent);

            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            //rectTransform.anchorMin = new Vector2(0.05f, 1.0f);
            rectTransform.anchorMin = new Vector2(0.05f, 1.0f);
            rectTransform.anchorMax = new Vector2(Mathf.Min(new float[] {0.95f, relativeSize.x, 0.95f*(0.95f*layout.Parent.rect.width - layout.Position.x) / (0.95f*layout.Parent.rect.width)}), 1.0f);
            rectTransform.sizeDelta = new Vector2(1.0f, DefaultButtonHeight);

            rectTransform.anchoredPosition = layout.Position;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(.5f, .5f, .5f, .5f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { onClickAction(); });

            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(rectTransform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, label, Vector2.one);
            }

            return rectTransform.rect.size;
        }

        private static Vector2 CreateToggleButtonObject(AdminPanelLayout layout, string label, bool status, Action<bool> onToggle, Vector2 relativeSize)
        {
            if(relativeSize.y != 1.0f)
            {
                Debug.LogWarning("AdminPanel Toggle Button - Relative Y size is ignored");
            }
            
            var buttonObject = new GameObject("AdminPanel - Toggle Button");
            buttonObject.transform.SetParent(layout.Parent);
            
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            //rectTransform.anchorMin = new Vector2(0.05f, 1.0f);
            rectTransform.anchorMin = new Vector2(0.05f, 1.0f);
            rectTransform.anchorMax = new Vector2(Mathf.Min(new float[] {0.95f, relativeSize.x, 0.95f*(0.95f*layout.Parent.rect.width - layout.Position.x) / (0.95f*layout.Parent.rect.width)}), 1.0f);
            rectTransform.sizeDelta = new Vector2(1.0f, DefaultButtonHeight);
            
            rectTransform.anchoredPosition = layout.Position;
            
            var toggle = buttonObject.AddComponent<Toggle>();

            var toggleBackground = new GameObject("AdminPanel - Toggle Background");
            RectTransform bgRectTransform = toggleBackground.AddComponent<RectTransform>();
            bgRectTransform.SetParent(rectTransform);
            bgRectTransform.pivot = Vector2.up;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.sizeDelta = Vector2.one;

            var image = toggleBackground.AddComponent<Image>();
            image.color = new Color(.5f, .5f, .5f, .5f);

            var disableIndicator = new GameObject("AdminPanel - Toggle Disabled Graphic");
            RectTransform disableRectTransform = disableIndicator.AddComponent<RectTransform>();
            disableRectTransform.SetParent(rectTransform);
            disableRectTransform.pivot = Vector2.up;
            disableRectTransform.offsetMin = Vector2.zero;
            disableRectTransform.offsetMax = Vector2.zero;
            disableRectTransform.anchorMin = new Vector2(0.95f, 0);
            disableRectTransform.anchorMax = Vector2.one;
            disableRectTransform.sizeDelta = Vector2.one;
            
            var disImage = disableIndicator.AddComponent<Image>();
            disImage.color = new Color(.8f, .3f, .3f, .8f);


            var toggleIndicator = new GameObject("AdminPanel - Toggle Enabled Graphic");
            RectTransform indicatorRectTransform = toggleIndicator.AddComponent<RectTransform>();
            indicatorRectTransform.SetParent(rectTransform);
            indicatorRectTransform.pivot = Vector2.up;
            indicatorRectTransform.offsetMin = Vector2.zero;
            indicatorRectTransform.offsetMax = Vector2.zero;
            indicatorRectTransform.anchorMin = new Vector2(0.95f, 0);
            indicatorRectTransform.anchorMax = Vector2.one;
            indicatorRectTransform.sizeDelta = Vector2.one;
            
            var indImage = toggleIndicator.AddComponent<Image>();
            indImage.color = new Color(.3f, .8f, .3f, .8f); //(status)? new Color(.3f, .8f, .3f, .8f) : new Color(.8f, .3f, .3f, .8f);

            toggle.targetGraphic = image;
            toggle.graphic = indImage;

            toggle.isOn = status;
            toggle.onValueChanged.AddListener((value) => { 
                //indImage.color = (value)? new Color(.3f, .8f, .3f, .8f) : new Color(.8f, .3f, .3f, .8f);
                onToggle(value); 
        });

            
            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(rectTransform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, label, Vector2.one);
            }
            
            return rectTransform.rect.size;
        }

        private static Vector2 CreateCloseButtonObject(AdminPanelLayout layout, UnityAction onClickAction)
        {
            var buttonObject = new GameObject("AdminPanel - Back Button");
            buttonObject.transform.SetParent(layout.Parent);
            
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.sizeDelta = new Vector2(DefaultButtonHeight, DefaultButtonHeight);
            
            rectTransform.anchoredPosition = Vector2.zero;
            
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(.5f, .5f, .5f, .0f);
            
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClickAction);
            
            using(AdminPanelLayout buttonLayout = new AdminPanelLayout(rectTransform))
            {
                AdminPanelGUIUtils.CreateLabelObject(buttonLayout, "<", Vector2.one);
            }
            
            return rectTransform.rect.size;
        }

        private static Vector2 CreateLabelObject(AdminPanelLayout layout, string label, Vector2 relativeSize)
        {
            var textObject = new GameObject("Admin Panel - Label");
            textObject.transform.SetParent(layout.Parent);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.pivot = Vector2.up;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = new Vector2(relativeSize.x, relativeSize.y);
            rectTransform.sizeDelta = new Vector2(1.0f, 25.0f);

            rectTransform.anchoredPosition = layout.Position;
            
            var text = textObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            return rectTransform.rect.size;
        }

        public static RectTransform CreatePanel(AdminPanelLayout layout, Vector2 relativeSize)
        {
            RectTransform rectTransform = AdminPanelGUIUtils.CreatePanelObject(layout, relativeSize);
            layout.Advance(rectTransform.rect.size + Margin);

            return rectTransform;
        }

        public static RectTransform CreatePanel(AdminPanelLayout layout, Vector2 relativeSize, UnityAction onCloseButton)
        {
            RectTransform rectTransform = AdminPanelGUIUtils.CreatePanelObject(layout, relativeSize);
            AdminPanelGUIUtils.CreateCloseButtonObject(layout, onCloseButton);

            layout.Advance(rectTransform.rect.size + Margin);
            
            return rectTransform;
        }
        
        public static void CreateButton(AdminPanelLayout layout, string label, Action onClickAction, Vector2 relativeSize)
        {
            layout.Advance(AdminPanelGUIUtils.CreateButtonObject(layout, label, onClickAction, relativeSize) + Margin);
        }

        public static void CreateButton(AdminPanelLayout layout, string label, Action onClickAction)
        {
            layout.Advance(AdminPanelGUIUtils.CreateButtonObject(layout, label, onClickAction, Vector2.one) + Margin);
        }

        public static void CreateToggleButton(AdminPanelLayout layout, string label, bool status, Action<bool> onToggle, Vector2 relativeSize)
        {
            layout.Advance(AdminPanelGUIUtils.CreateToggleButtonObject(layout, label, status, onToggle, relativeSize) + Margin);
        }

        public static void CreateToggleButton(AdminPanelLayout layout, string label, bool status, Action<bool> onToggle)
        {
            layout.Advance(AdminPanelGUIUtils.CreateToggleButtonObject(layout, label, status, onToggle, Vector2.one) + Margin);
        }
        
        public static void CreateLabel(AdminPanelLayout layout, string label, Vector2 relativeSize)
        {
            layout.Advance(AdminPanelGUIUtils.CreateLabelObject(layout, label, relativeSize) + Margin);
        }

        public static void CreateLabel(AdminPanelLayout layout, string label)
        {
            layout.Advance(AdminPanelGUIUtils.CreateLabelObject(layout, label, Vector2.one) + Margin);
        }

        public static void CreateMargin(AdminPanelLayout layout, float marginMultiplier = 2.0f)
        {
            layout.Advance(Margin * marginMultiplier);
        }

    }
}
