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
            //rectTrans.sizeDelta = new Vector2(parentLayout.Parent.rect.width - parentLayout.Position.x, parentLayout.Parent.rect.height - parentLayout.Position.y);

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

    /*
    public class VerticalLayout : AdminPanelLayout
    {
        public VerticalLayout(AdminPanelLayout parentLayout, Vector2 relativeSize) : base(parentLayout)
        {
            GameObject layoutObject = new GameObject("AdminPanel - Vertical Layout");
            layoutObject.transform.SetParent(parentLayout.Parent);
            RectTransform rectTrans = layoutObject.AddComponent<RectTransform>();
            
            rectTrans.pivot = Vector2.up;
            
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            
            // Upper edge anchor
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.up;
            
            rectTrans.sizeDelta = new Vector2(parentLayout.Position.x - parentLayout.Parent.rect.width, //1.0f - (parentLayout.Position.y / parentLayout.Parent.rect.height), 
                                              parentLayout.Parent.rect.height + parentLayout.Position.y);
            ////////
            
            float width = parentLayout.Parent.rect.width * relativeSize.x;
            float height =  parentLayout.Parent.rect.height * relativeSize.y;
            
            rectTrans.sizeDelta = new Vector2(width, height);
            
            // Position in set through anchor. Problems inside panel
            //rectTrans.anchoredPosition = Vector2.zero;
            
            VerticalLayoutGroup layoutGroup = layoutObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 5.0f; //FIXME Margin
            
            LayoutElement layoutElement = layoutObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
            layoutElement.preferredHeight = height;
            
            Parent = rectTrans;
            parentLayout.Advance(rectTrans.rect.size);
        }
        
        public VerticalLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }
        
        public override void DoAdvance(float x, float y)
        {
            _currentPosition.y -= y;
        }
    }*/
}
