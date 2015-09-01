using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public AdminPanelLayout CreateVerticalLayout(int weight)
        {
            return new VerticalLayout(this, weight);
        }

        public AdminPanelLayout CreateVerticalLayout()
        {
            return CreateVerticalLayout(1);
        }
    }
    
    public class VerticalLayout : AdminPanelLayout
    {
        public VerticalLayout(AdminPanelLayout parentLayout, int weight) : base(parentLayout)
        {
            var rectTransform = CreateUIObject("AdminPanel - Vertical Layout", parentLayout.Parent);

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
