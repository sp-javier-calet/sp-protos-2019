using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public AdminPanelLayout CreateHorizontalLayout()
        {
            return new HorizontalLayout(this, Vector2.one);
        }
    }

    public class HorizontalLayout : AdminPanelLayout
    {
        public HorizontalLayout(AdminPanelLayout parentLayout, Vector2 relativeSize) : base(parentLayout)
        {
            var rectTrans = CreateUIObject("AdminPanel - Horizontal Layout", parentLayout.Parent);
            //rectTrans.sizeDelta = new Vector2(parentLayout.Parent.rect.width - parentLayout.Position.x, parentLayout.Parent.rect.height - parentLayout.Position.y);

            HorizontalLayoutGroup layoutGroup = rectTrans.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(DefaultPadding, DefaultPadding, DefaultPadding, DefaultPadding);
            layoutGroup.spacing = DefaultMargin;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childForceExpandWidth = false;
            
            Parent = rectTrans;
        }
        
        public HorizontalLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }
    }
}