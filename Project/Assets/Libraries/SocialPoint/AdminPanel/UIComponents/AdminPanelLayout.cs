using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }
        
        protected Vector2 _currentPosition;
        protected Vector2 _aabb;
        private AdminPanelLayout _parentLayout ;
        
        public Vector2 Position
        {
            get { return _currentPosition;}
        }
        
        protected AdminPanelLayout()
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
            //DoAdvance(x, y);
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
                //_parentLayout.Advance(_currentPosition.x, -_currentPosition.y);
            }
        }
        
        protected void AdjustMinHeight()
        {
            Vector2 finalSize = new Vector2(Parent.rect.size.x, _aabb.y);
            Parent.sizeDelta = finalSize;
            _currentPosition.y = -_aabb.y;
        }
    }
}
