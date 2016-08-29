using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public VerticalLayout CreateVerticalLayout(int weight)
        {
            return new VerticalLayout(this, weight);
        }

        public VerticalLayout CreateVerticalLayout()
        {
            return CreateVerticalLayout(1);
        }

        public sealed class VerticalLayout : AdminPanelLayout
        {
            public VerticalLayout(AdminPanelLayout parentLayout, int weight) : base(parentLayout)
            {
                var rectTransform = CreateUIObject("Vertical Layout", parentLayout.Parent);
                
                var layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = weight;
                layoutElement.flexibleHeight = weight;
                
                var layoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
                layoutGroup.spacing = DefaultMargin;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.childForceExpandWidth = false;
                
                Parent = rectTransform;
            }
        }
    }
}
