using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public void CreateToggleButton(string label, bool status, Action<bool> onToggle)
        {
            var rectTransform = CreateUIObject("Admin Panel - Toggle Button", Parent);

            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = DefaultLabelHeight;
            layoutElement.flexibleWidth = 1;

            var toggleBackground = CreateUIObject("Admin Panel - Toggle Background", rectTransform);
            var image = toggleBackground.gameObject.AddComponent<Image>();
            image.color = ForegroundColor;

            // Status indicators
            Vector2 anchorMin = new Vector2(.9f,  .3f);
            Vector2 anchorMax = new Vector2(.95f, .7f);

            var disableIndicator = CreateUIObject("Admin Panel - Toggle Enabled Graphic", rectTransform);
            disableIndicator.anchorMin = anchorMin;
            disableIndicator.anchorMax = anchorMax;

            var disImage = disableIndicator.gameObject.AddComponent<Image>();
            disImage.color = StatusDisabledColor;
            
            var toggleIndicator = CreateUIObject("Admin Panel - Toggle Disabled Graphic", rectTransform);
            toggleIndicator.anchorMin = anchorMin;
            toggleIndicator.anchorMax = anchorMax;

            var indImage = toggleIndicator.gameObject.AddComponent<Image>();
            indImage.color = StatusEnabledColor;

            // Toggle button
            var toggle = rectTransform.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = indImage;
            
            toggle.isOn = status;
            toggle.onValueChanged.AddListener((value) => {
                onToggle(value); 
            });

            CreateToggleButtonLabel(label, rectTransform);
        }
        
        private void CreateToggleButtonLabel(string label, RectTransform buttonTransform)
        {
            var rectTransform = CreateUIObject("Admin Panel - Toggle Button Label", buttonTransform);
            
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

