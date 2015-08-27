using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public abstract class AdminPanelGUI
    {
        public static AdminPanelConsole AdminPanelConsole { set; private get; }

        public AdminPanelConsole Console { get { return AdminPanelGUI.AdminPanelConsole; }}

        public abstract void OnCreateGUI(AdminPanelLayout layout);
    }

    public sealed class AdminPanelGUIOptions
    {
        public static readonly AdminPanelGUIOptions None = new AdminPanelGUIOptions();
    }

    public class AdminPanelLayout : IDisposable
    {

        public RectTransform Parent { get; protected set; }

        protected Vector2 _currentPosition;
        protected Vector2 _aabb;
        private AdminPanelLayout _parentLayout ;

        public Vector2 Position
        {
            get { return _currentPosition;}
        }

        private AdminPanelLayout()
        {
            _currentPosition = new Vector2();
            _aabb = new Vector2();
            _parentLayout = null;
        }

        public AdminPanelLayout(AdminPanelLayout parentLayout)
            : this()
        {
            _parentLayout = parentLayout;
            Parent = parentLayout.Parent;
        }

        public AdminPanelLayout(RectTransform rectTransform)
            : this()
        {
            Parent = rectTransform;
        }

        public void Advance(Vector2 offset)
        {
            Advance(offset.x, offset.y);
        }

        public void Advance(float x, float y)
        {
            _aabb.x = Mathf.Max(_aabb.x, x);
            _aabb.y = Mathf.Max(_aabb.y, y);
            DoAdvance(x, y);
        }

        public virtual void DoAdvance(float x, float y)
        {
            _currentPosition.x += x;
            _currentPosition.y -= y;
        }

        public virtual void Dispose()
        {
            if(_parentLayout != null)
            {
               _parentLayout.Advance(_currentPosition.x, -_currentPosition.y);
            }
        }

        protected void AdjustMinHeight()
        {
            Vector2 finalSize = new Vector2(Parent.rect.size.x, _aabb.y);
            Parent.sizeDelta = finalSize;
            _currentPosition.y = -_aabb.y;
        }
    }

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
            /*
            rectTrans.anchorMin = new Vector2(parentLayout.Position.x / parentLayout.Parent.rect.width, 
                                              Mathf.Clamp(1 - (parentLayout.Position.y / parentLayout.Parent.rect.height), 0, 1.0f));
            rectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            */
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.up;

            rectTrans.sizeDelta = new Vector2(parentLayout.Position.x - parentLayout.Parent.rect.width, //1.0f - (parentLayout.Position.y / parentLayout.Parent.rect.height), 
                                              parentLayout.Parent.rect.height + parentLayout.Position.y);
            ////////
            float width = (relativeSize.x >= 1.0)?  parentLayout.Parent.rect.width - parentLayout.Position.x : // remaining space
                                                    parentLayout.Parent.rect.width * relativeSize.x;
            float height = (relativeSize.y >= 1.0)? parentLayout.Parent.rect.height + parentLayout.Position.y : // remaining space
                                                    parentLayout.Parent.rect.height * relativeSize.y;
            
            rectTrans.sizeDelta = new Vector2(width, height);

            // Position in set through anchor. Problems inside panel
            rectTrans.anchoredPosition = Vector2.zero;//parentLayout.Position;
            rectTrans.anchoredPosition = parentLayout.Position;

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
    }

    public class HorizontalLayout : AdminPanelLayout
    {
        public HorizontalLayout(AdminPanelLayout parentLayout, Vector2 relativeSize) : base(parentLayout)
        {
            GameObject layoutObject = new GameObject("AdminPanel - Horizontal Layout");
            layoutObject.transform.SetParent(parentLayout.Parent);

            RectTransform rectTrans = layoutObject.AddComponent<RectTransform>();
            rectTrans.pivot = Vector2.up;

            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;

            // Left edge anchor and fill parent
//            rectTrans.anchorMin = new Vector2(parentLayout.Position.x / parentLayout.Parent.rect.width , 1 - (parentLayout.Position.y / parentLayout.Parent.rect.height));
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.up;
            rectTrans.sizeDelta = new Vector2(parentLayout.Parent.rect.width - parentLayout.Position.x, parentLayout.Parent.rect.height - parentLayout.Position.y);

            // Position in set through anchor
            rectTrans.anchoredPosition = Vector2.zero;//parentLayout.Position;
            rectTrans.anchoredPosition = parentLayout.Position;

            Parent = rectTrans;
            //parentLayout.Advance(rectTrans.rect.size);
        }

        public HorizontalLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }

        public override void DoAdvance(float x, float y)
        {
            _currentPosition.x += x;
        }

        public override void Dispose()
        {
            AdjustMinHeight();
            base.Dispose();
        }
    }
    
    public class VerticalScrollLayout : AdminPanelLayout
    {
        public VerticalScrollLayout(AdminPanelLayout parentLayout, Vector2 relativeSize) : base(parentLayout)
        {
            GameObject scrollObject = new GameObject("AdminPanel - Scroll View");
            scrollObject.transform.SetParent(parentLayout.Parent);

            RectTransform scrollRectTrans = scrollObject.AddComponent<RectTransform>();
            scrollRectTrans.pivot = Vector2.up;
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = Vector2.zero;

            // Upper edge anchor
            /*scrollRectTrans.anchorMin = new Vector2(Mathf.Max(0.05f, parentLayout.Position.x / parentLayout.Parent.rect.width),
                                                    1.0f - Mathf.Clamp(parentLayout.Position.y / parentLayout.Parent.rect.height, 0.0f, 1.0f));
            scrollRectTrans.anchorMax = new Vector2(0.95f, 1.0f);
*/

            scrollRectTrans.anchorMax = Vector2.up;
            scrollRectTrans.anchorMin = Vector2.up;

            /*
            float width = (relativeSize.x >= 1.0)? (1.0f - (parentLayout.Position.x / parentLayout.Parent.rect.width)) : // remaining space
                                                    parentLayout.Parent.rect.width * relativeSize.x;
            float height = (relativeSize.y >= 1.0)? parentLayout.Parent.rect.height + parentLayout.Position.y : // remaining space
                                                    parentLayout.Parent.rect.height * relativeSize.y;
                                                    
            scrollRectTrans.sizeDelta = new Vector2(width, height);
*/
            float width = (relativeSize.x >= 1.0)?  0.9f * parentLayout.Parent.rect.width - parentLayout.Position.x : // remaining space
                parentLayout.Parent.rect.width * relativeSize.x;
            float height = (relativeSize.y >= 1.0)? Mathf.Max(parentLayout.Parent.rect.height + parentLayout.Position.y, 0) : // remaining space
                parentLayout.Parent.rect.height * relativeSize.y;
            
            scrollRectTrans.sizeDelta = new Vector2(width, height);
            
            // Inside panel
            Vector2 margin = new Vector2(parentLayout.Parent.rect.width * 0.05f, -5.0f); // FIXME MARGINS

            scrollRectTrans.anchoredPosition = parentLayout.Position + margin;
            parentLayout.Advance(margin);

            Image image = scrollObject.AddComponent<Image>();
            image.color = new Color(.2f, .2f, .2f, .5f);

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            scrollObject.AddComponent<Mask>();
            
            
            GameObject scrollContentObject = new GameObject("AdminPanel - Scroll Content");
            scrollContentObject.transform.SetParent(scrollObject.transform);
           
            RectTransform rectTrans = scrollContentObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = Vector2.up;
            rectTrans.anchorMax = Vector2.one;

            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            rectTrans.pivot = Vector2.up;
            rectTrans.anchoredPosition = Vector2.zero;

            // Set initial content size to fit parent. It will be adjusted on Dispose
            rectTrans.sizeDelta = new Vector2(1.0f, height);

            scroll.content = rectTrans;

            Parent = rectTrans;
            parentLayout.Advance(scrollRectTrans.rect.size);

            //Initial Margin
            Advance(0, 5.0f);
        }

        public VerticalScrollLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }

        public override void DoAdvance(float x, float y)
        {
            _currentPosition.y -= y;
        }

        public override void Dispose()
        {
            base.Dispose();
            // Adjust content size to set scroll size
            Parent.sizeDelta = new Vector2(Parent.sizeDelta.x, -Position.y); 
        }
    }
}