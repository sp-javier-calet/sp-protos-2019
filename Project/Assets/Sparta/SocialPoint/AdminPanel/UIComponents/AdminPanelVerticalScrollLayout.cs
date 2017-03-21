#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public VerticalScrollLayout CreateVerticalScrollLayout(out ScrollRect consoleScroll)
        {
            var layout = new VerticalScrollLayout(this, DefaultLayoutWeight);
            consoleScroll = layout.ScrollRect;
            return layout;
        }

        public VerticalScrollLayout CreateVerticalScrollLayout(int weight)
        {
            return new VerticalScrollLayout(this, weight);
        }

        public VerticalScrollLayout CreateVerticalScrollLayout()
        {
            return CreateVerticalScrollLayout(1);
        }

        public sealed class VerticalScrollLayout : AdminPanelLayout
        {
            public  ScrollRect ScrollRect { get; private set; }

            public VerticalScrollLayout(AdminPanelLayout parentLayout, int weight) : base(parentLayout)
            {
                // Scroll object
                var rectTransform = CreateUIObject("Admin Panel - Vertical Scroll Layout", parentLayout.Parent);
                
                LayoutElement layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = weight;
                layoutElement.flexibleHeight = weight;

                // It requires a minimal size or could result in a zero-sized layout
                layoutElement.preferredHeight = DefaultLabelHeight * 4;
                
                Image image = rectTransform.gameObject.AddComponent<Image>();
                image.color = BackgroundColor;
                
                ScrollRect = rectTransform.gameObject.AddComponent<ScrollRect>();
                ScrollRect.horizontal = false;
                
                rectTransform.gameObject.AddComponent<Mask>();

                // Content object
                var contentTransform = CreateUIObject("Admin Panel - Vertical Scroll Content", rectTransform);
                
                VerticalLayoutGroup layoutGroup = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultScrollPadding, DefaultScrollPadding, DefaultPadding, DefaultPadding);
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.spacing = DefaultMargin;
                
                var sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                ScrollRect.content = contentTransform;
                Parent = contentTransform;
            }
        }
    }
}

#endif
