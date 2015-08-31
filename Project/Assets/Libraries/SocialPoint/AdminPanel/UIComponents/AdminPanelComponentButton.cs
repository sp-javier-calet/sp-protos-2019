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

        public void CreateOpenPanelButton(string label, AdminPanelGUI panel)
        {
            var rectTransform = CreateUIObject("Admin Panel - Button", Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = ForegroundColor;
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { OpenPanel(panel); });
            
            CreateButtonLabel(label, rectTransform);
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
    }
}
