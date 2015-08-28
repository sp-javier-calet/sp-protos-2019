using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public AdminPanelLayout CreateVerticalScrollLayout(int weight)
        {
            return new VerticalScrollLayout(this, weight);
        }
        
        public AdminPanelLayout CreateVerticalScrollLayout()
        {
            return CreateVerticalScrollLayout(1);
        }

        private class VerticalScrollLayout : AdminPanelLayout
        {
            public VerticalScrollLayout(AdminPanelLayout parentLayout, int weight) : base(parentLayout)
            {
                // Scroll object
                var rectTransform = CreateUIObject("Admin Panel - Vertical Scroll Layout", parentLayout.Parent);
                
                LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = 1;
                layoutElement.flexibleHeight = 1;
                
                Image image = rectTransform.gameObject.AddComponent<Image>();
                image.color = BackgroundColor;
                
                ScrollRect scroll = rectTransform.gameObject.AddComponent<ScrollRect>();
                scroll.horizontal = false;
                
                rectTransform.gameObject.AddComponent<Mask>();

                // Content object
                var contentTransform = CreateUIObject("Admin Panel - Vertical Scroll Content", rectTransform);
                
                VerticalLayoutGroup layoutGroup = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.spacing = DefaultMargin;
                
                var sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                scroll.content = contentTransform;
                Parent = contentTransform;
            }
        }
    }
}
