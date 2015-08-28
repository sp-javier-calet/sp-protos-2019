using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public AdminPanelLayout CreatePanelLayout(int weight)
        {
            return new PanelLayout(this, weight);
        }
        
        public AdminPanelLayout CreatePanelLayout()
        {
            return CreatePanelLayout(1);
        }
    }
    
    public class PanelLayout : AdminPanelLayout
    {
        public PanelLayout(AdminPanelLayout parentLayout, int weight) : base(parentLayout)
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
            layoutGroup.childForceExpandWidth = true;

            Parent = rectTransform;
        }
    }
}
