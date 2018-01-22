#if ADMIN_PANEL

using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public VerticalLayout CreateVerticalLayout(int weight, float preferedWidth = -1, bool childForceExpandHeight = true)
        {
            return new VerticalLayout(this, weight, preferedWidth, childForceExpandHeight);
        }

        public VerticalLayout CreateVerticalLayout()
        {
            return CreateVerticalLayout(1);
        }

        public VerticalLayout CreateVerticalLayoutNotForceExpandHeight()
        {
            return CreateVerticalLayout(1, -1, false);
        }

        public sealed class VerticalLayout : AdminPanelLayout
        {
            public VerticalLayout(AdminPanelLayout parentLayout, int weight, float preferedWidth, bool childForceExpandHeight) : base(parentLayout)
            {
                var rectTransform = CreateUIObject("Vertical Layout", parentLayout.Parent);
                
                var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = weight;
                layoutElement.flexibleHeight = weight;
                if(preferedWidth > 0)
                {
                    layoutElement.preferredWidth = preferedWidth;
                }
                
                var layoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
                layoutGroup.spacing = DefaultMargin;
                layoutGroup.childForceExpandHeight = childForceExpandHeight;
                layoutGroup.childForceExpandWidth = false;
                
                Parent = rectTransform;
            }
        }
    }
}

#endif
