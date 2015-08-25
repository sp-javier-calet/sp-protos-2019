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
        private float _offset;

        protected Vector3 _currentPosition;
        public Vector3 Position
        {
            get { return _currentPosition;}
        }

        protected AdminPanelLayout()
        {
            _currentPosition = new Vector3();
        }

        public AdminPanelLayout(RectTransform parent)
        {
            _currentPosition = new Vector3();
            Parent = parent;
        }

        public AdminPanelLayout(AdminPanelLayout parentLayout)
        {
            _currentPosition = new Vector3();
            Parent = parentLayout.Parent;
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

        }
    }

    public class VerticalLayout : AdminPanelLayout
    {
        public VerticalLayout(RectTransform parent)
        {
            GameObject layoutObject = new GameObject("AdminPanel - Vertical Layout");
            layoutObject.transform.SetParent(parent);
            RectTransform rectTrans = layoutObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 1.0f);
            rectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            rectTrans.pivot = new Vector2(0.5f, 1.0f);
            rectTrans.localPosition = Vector2.zero;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            
            Parent = rectTrans;
            Advance(0, 20.0f);
        }
        
        public VerticalLayout(AdminPanelLayout parentLayout)
            : this(parentLayout.Parent)
        {
        }

        public override void Advance(float x, float y)
        {
            _currentPosition.y -= y;
        }
    }

    public class HorizontalLayout : AdminPanelLayout
    {
        public HorizontalLayout(RectTransform parent)
        {
            GameObject layoutObject = new GameObject("AdminPanel - Horizontal Layout");
            layoutObject.transform.SetParent(parent);
            RectTransform rectTrans = layoutObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 1.0f);
            rectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            rectTrans.pivot = new Vector2(0.5f, 1.0f);
            rectTrans.localPosition = Vector2.zero;
            rectTrans.offsetMin = Vector2.zero;
            rectTrans.offsetMax = Vector2.zero;
            
            Parent = rectTrans;
            Advance(20.0f, 0.0f);
        }
        
        public HorizontalLayout(AdminPanelLayout parentLayout)
            : this(parentLayout.Parent)
        {
        }

        public override void Advance(float x, float y)
        {
            _currentPosition.x += x;
        }
    }

    public class VerticalScrollLayout : AdminPanelLayout
    {
        public VerticalScrollLayout(AdminPanelLayout parentLayout)
        {
            GameObject scrollObject = new GameObject("AdminPanel - Scroll View");
            scrollObject.transform.SetParent(parentLayout.Parent);
            RectTransform scrollRectTrans = scrollObject.AddComponent<RectTransform>();
            scrollRectTrans.anchorMin = new Vector2(0.0f, 1.0f);
            scrollRectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            scrollRectTrans.pivot = new Vector2(0.5f, 1.0f);
            scrollRectTrans.localPosition = Vector2.zero;
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = Vector2.zero;
            scrollRectTrans.localPosition = parentLayout.Position;
            Image image = scrollObject.AddComponent<Image>();
            image.color = new Color(1f, .3f, .3f, .5f);
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            
            
            GameObject scrollContentObject = new GameObject("AdminPanel - Scroll Content");
            scrollContentObject.transform.SetParent(scrollObject.transform);
            RectTransform rectTrans = scrollContentObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
            rectTrans.anchorMax = new Vector2(1.0f, 0.5f);
            rectTrans.offsetMin = new Vector2(0.0f, -50.0f);
            rectTrans.offsetMax = new Vector2(0.0f, 0.0f);
            rectTrans.pivot = new Vector2(0.5f, 1.0f);
            rectTrans.localPosition = Vector2.zero;
            scroll.content = rectTrans;

            parentLayout.Advance(0, scrollRectTrans.rect.height);
            Parent = rectTrans;
        }

        public override void Advance(float x, float y)
        {
            _currentPosition.y -= y;
        }
    }
}