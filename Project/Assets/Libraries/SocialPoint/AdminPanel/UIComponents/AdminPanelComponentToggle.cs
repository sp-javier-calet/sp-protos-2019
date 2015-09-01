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

            // Status indicators parameters
            Vector2 anchorMin = Vector2.one;
            Vector2 anchorMax = Vector2.one;
            int indicatorSize = (int)Mathf.Round(DefaultLabelHeight / 3);
            Vector2 anchoredPosition = new Vector2(-indicatorSize * 2, -indicatorSize - 1);
            Vector2 indicatorSizeDelta = new Vector2(indicatorSize, indicatorSize);

            // Disabled indicator
            var disableIndicator = CreateUIObject("Admin Panel - Toggle Disabled Graphic", rectTransform);
            disableIndicator.anchorMin = anchorMin;
            disableIndicator.anchorMax = anchorMax;
            disableIndicator.anchoredPosition = anchoredPosition;
            disableIndicator.sizeDelta = indicatorSizeDelta;

            var disImage = disableIndicator.gameObject.AddComponent<Image>();
            disImage.color = StatusDisabledColor;

            // Enabled indicator
            var toggleIndicator = CreateUIObject("Admin Panel - Toggle Enabled Graphic", rectTransform);
            toggleIndicator.anchorMin = anchorMin;
            toggleIndicator.anchorMax = anchorMax;
            toggleIndicator.anchoredPosition = anchoredPosition;
            toggleIndicator.sizeDelta = indicatorSizeDelta;

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

