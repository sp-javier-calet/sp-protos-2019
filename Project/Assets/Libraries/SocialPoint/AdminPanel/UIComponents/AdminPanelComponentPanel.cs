using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public AdminPanelLayout CreatePanelLayout(string title, Action onClose, int weight)
        {
            return new PanelLayout(this, title, onClose, weight);
        }

        public AdminPanelLayout CreatePanelLayout(string title, Action onClose)
        {
            return CreatePanelLayout(title, onClose, DefaultLayoutWeight);
        }

        public AdminPanelLayout CreatePanelLayout(string title)
        {
            return CreatePanelLayout(title, null, DefaultLayoutWeight);
        }

        public AdminPanelLayout CreatePanelLayout()
        {
            return CreatePanelLayout(null, null, DefaultLayoutWeight);
        }
    }
    
    public class PanelLayout : AdminPanelLayout
    {
        private Text _titleComponent;
        public string Title 
        {
            get
            {
                return _titleComponent.text;
            }
            set
            {
                _titleComponent.text = value;
            }
        }

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

        private void CreatePanelTitle(RectTransform panelTransform, string title)
        {
            var rectTransform = CreateUIObject("Admin Panel - Panel title", panelTransform);
            
            var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = PanelHeaderHeight;
            layoutElement.flexibleWidth = 1;
            
            _titleComponent = rectTransform.gameObject.AddComponent<Text>();
            _titleComponent.text = title;
            _titleComponent.font = DefaultFont;
            _titleComponent.fontSize = PanelTitleFontSize;
            _titleComponent.color = Color.white;
            _titleComponent.alignment = TextAnchor.UpperRight;
        }

        private void CreateCloseButton(RectTransform panelTransform, Action onClose)
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
            rectTransform.anchoredPosition = new Vector2(-5.0f, 5.0f);
            rectTransform.sizeDelta = new Vector2(50, 30);
            
            layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            
            image = rectTransform.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f); // Invisible image
            
            var button = rectTransform.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { onClose(); });
        }
    }
}
