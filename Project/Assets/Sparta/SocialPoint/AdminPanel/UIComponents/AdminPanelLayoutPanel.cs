#if ADMIN_PANEL

using System;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public PanelLayout CreatePanelLayout(string title, Action onClose, int weight)
        {
            return new PanelLayout(this, title, onClose, weight);
        }

        public PanelLayout CreatePanelLayout(string title, Action onClose)
        {
            return CreatePanelLayout(title, onClose, DefaultLayoutWeight);
        }

        public PanelLayout CreatePanelLayout(string title)
        {
            return CreatePanelLayout(title, null, DefaultLayoutWeight);
        }

        public PanelLayout CreatePanelLayout()
        {
            return CreatePanelLayout(null, null, DefaultLayoutWeight);
        }
    }

    public sealed class PanelLayout : AdminPanelLayout
    {
        public PanelLayout(AdminPanelLayout parentLayout, string title, Action onClose, int weight) : base(parentLayout)
        {
            var rectTransform = CreateUIObject("Admin Panel - Panel", parentLayout.Parent);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = weight;
            layoutElement.flexibleHeight = weight;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = BackgroundColor;
            
            var layoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
            layoutGroup.spacing = DefaultMargin;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;

            // Panel header
            if(title != null)
            {
                CreatePanelTitle(rectTransform, title);
            }

            if(onClose != null)
            {
                CreateCloseButton(rectTransform, onClose);
            }

            Parent = rectTransform;
        }

        Text CreatePanelTitle(Transform panelTransform, string title)
        {
            var rectTransform = CreateUIObject("Admin Panel - Panel title", panelTransform);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = PanelHeaderHeight;
            layoutElement.flexibleWidth = 1;
            
            var text = rectTransform.gameObject.AddComponent<Text>();
            text.text = title;
            text.font = DefaultFont;
            text.fontSize = PanelTitleFontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperRight;

            return text;
        }

        Button CreateCloseButton(Transform panelTransform, Action onClose)
        {
            // Close button decorator
            var rectTransform = CreateUIObject("Admin Panel - Close panel button decorator", panelTransform);
            
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.anchoredPosition = new Vector2(DefaultMargin, -DefaultMargin);
            rectTransform.sizeDelta = new Vector2(PanelTitleFontSize, PanelTitleFontSize);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.color = CloseButtonColor;

            // Close button
            rectTransform = CreateUIObject("Admin Panel - Close panel button", panelTransform);
            
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            rectTransform.anchoredPosition = new Vector2(-10.0f, 10.0f);
            rectTransform.sizeDelta = new Vector2(60, 40);
            
            layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            
            image = rectTransform.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f); // Invisible image
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = button.colors.pressedColor;
            button.colors = colors;

            button.onClick.AddListener(() => onClose());

            return button;
        }
    }
}

#endif
