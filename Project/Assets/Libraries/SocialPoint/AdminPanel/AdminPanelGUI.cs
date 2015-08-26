using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public abstract class AdminPanelGUI
    {
        public abstract void OnCreateGUI(AdminPanelLayout layout);
    }

    public sealed class AdminPanelGUIOptions
    {
        public static readonly AdminPanelGUIOptions None = new AdminPanelGUIOptions();
    }

    public class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        protected Vector3 _currentPosition;
        private AdminPanelLayout _parentLayout ;

        public Vector3 Position
        {
            get { return _currentPosition;}
        }

        public AdminPanelLayout(AdminPanelLayout parentLayout)
        {
            _currentPosition = new Vector3();
            _parentLayout = parentLayout;
            Parent = parentLayout.Parent;
        }

        public AdminPanelLayout(RectTransform rectTransform)
        {
            _currentPosition = new Vector3();
            _parentLayout = null;
            Parent = rectTransform;
        }
        
        public void Advance(Vector2 offset)
        {
            Advance(offset.x, offset.y);
        }

        public virtual void Advance(float x, float y)
        {
            _currentPosition.x += x;
            _currentPosition.y -= y;
        }

        public void Dispose()
        {
            if(_parentLayout != null)
            {
                _parentLayout.Advance(_currentPosition.x, -_currentPosition.y);
            }
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
            rectTrans.anchorMin = new Vector2(parentLayout.Position.x / parentLayout.Parent.rect.width, 
                                              1 - (parentLayout.Position.y / parentLayout.Parent.rect.height));
            rectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            rectTrans.sizeDelta = new Vector2(1.0f - (parentLayout.Position.y / parentLayout.Parent.rect.height), 
                                              parentLayout.Parent.rect.height - parentLayout.Position.y);

            // Position in set through anchor. Problems inside panel
            rectTrans.anchoredPosition = Vector2.zero;//parentLayout.Position;

            Parent = rectTrans;
            parentLayout.Advance(rectTrans.rect.size);
        }

        public VerticalLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }

        public override void Advance(float x, float y)
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
            rectTrans.anchorMin = new Vector2(parentLayout.Position.x / parentLayout.Parent.rect.width , 1 - (parentLayout.Position.y / parentLayout.Parent.rect.height));
            rectTrans.anchorMax = Vector2.up;
            rectTrans.sizeDelta = new Vector2(parentLayout.Parent.rect.width - parentLayout.Position.x, parentLayout.Parent.rect.height - parentLayout.Position.y);

            // Position in set through anchor
            rectTrans.anchoredPosition = Vector2.zero;//parentLayout.Position;
            rectTrans.anchoredPosition = parentLayout.Position;

            Parent = rectTrans;
            parentLayout.Advance(rectTrans.rect.size);
        }

        public HorizontalLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }

        public override void Advance(float x, float y)
        {
            _currentPosition.x += x;
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
            scrollRectTrans.anchorMin = new Vector2(Mathf.Max(0.05f, parentLayout.Position.x / parentLayout.Parent.rect.width),
                                                    1.0f - Mathf.Clamp(parentLayout.Position.y / parentLayout.Parent.rect.height, 0.0f, 1.0f));
            scrollRectTrans.anchorMax = new Vector2(0.95f, 1.0f);
            scrollRectTrans.sizeDelta = new Vector2(1.0f - (parentLayout.Position.y / parentLayout.Parent.rect.height), parentLayout.Parent.rect.height - parentLayout.Position.y);

            // Inside panel
            scrollRectTrans.anchoredPosition = parentLayout.Position;

            Image image = scrollObject.AddComponent<Image>();
            image.color = new Color(.2f, .2f, .2f, .5f);

            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            scrollObject.AddComponent<Mask>();
            
            
            GameObject scrollContentObject = new GameObject("AdminPanel - Scroll Content");
            scrollContentObject.transform.SetParent(scrollObject.transform);

            RectTransform rectTrans = scrollContentObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
            rectTrans.anchorMax = new Vector2(1.0f, 0.5f);
            rectTrans.offsetMin = new Vector2(0.0f, -50.0f);
            rectTrans.offsetMax = new Vector2(0.0f, 0.0f);
            rectTrans.pivot = Vector2.up;
            rectTrans.localPosition = Vector2.zero;

            scroll.content = rectTrans;

            Parent = rectTrans;
            parentLayout.Advance(scrollRectTrans.rect.size);
        }

        public VerticalScrollLayout(AdminPanelLayout parentLayout) 
            : this(parentLayout, Vector2.one)
        {
        }

        public override void Advance(float x, float y)
        {
            _currentPosition.y -= y;
        }
    }
}