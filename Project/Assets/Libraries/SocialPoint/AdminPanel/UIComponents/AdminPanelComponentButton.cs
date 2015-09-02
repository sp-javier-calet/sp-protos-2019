using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public void CreateButton(string label, Action onClick)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button", Parent);
          
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = ForegroundColor;
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { onClick(); });
            
            CreateButtonLabel(label, rectTransform);
        }

        public void CreateOpenPanelButton(string label, AdminPanelGUI panel, bool replacePanel = false)
        {
            var rectTransform = CreateUIObject("Admin Panel - Open Panel Button", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = ForegroundColor;
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { 
                if(replacePanel)
                {
                    ReplacePanel(panel);
                }
                else
                {
                    OpenPanel(panel);
                }
            });
            
            CreateButtonLabel(label, rectTransform);
            CreateOpenPanelIndicator(rectTransform);
        }

        private void CreateButtonLabel(string label, RectTransform buttonTransform)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button Label", buttonTransform);
            
            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = label;
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
        }

        private void CreateOpenPanelIndicator(RectTransform buttonTransform)
        {
            var rectTransform = CreateUIObject("Admin Panel - Open Panel Indicator", buttonTransform);
            rectTransform.anchorMin = Vector2.right;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = new Vector2(-DefaultFontSize * 1.5f, 0);
            rectTransform.sizeDelta = new Vector2(DefaultFontSize, 1.0f);

            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = ">";
            text.font = DefaultFont;
            text.fontSize = DefaultFontSize / 2;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleRight;
            
            LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
        }
    }
}
