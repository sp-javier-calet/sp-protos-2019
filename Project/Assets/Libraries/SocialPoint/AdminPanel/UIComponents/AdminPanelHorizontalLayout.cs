using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout
    {
        public HorizontalLayout CreateHorizontalLayout()
        {
            return new HorizontalLayout(this, Vector2.one);
        }

        public class HorizontalLayout : AdminPanelLayout
        {
            public HorizontalLayout(AdminPanelLayout parentLayout, Vector2 relativeSize) : base(parentLayout)
            {
                var rectTrans = CreateUIObject("Horizontal Layout", parentLayout.Parent);
                
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
}