using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        private AdminPanelView _view;

        public AdminPanelLayout(AdminPanelLayout parentLayout)
        {
            Parent = parentLayout.Parent;
            _view = parentLayout._view;
        }
        
        public AdminPanelLayout(RectTransform rectTransform)
        {
            Parent = rectTransform;
        }

        protected AdminPanelLayout(AdminPanelView view)
        {
            _view = view;
        }
        
        protected void OpenPanel(AdminPanelGUI panel)
        {
            _view.OpenPanel(panel);
        }

        protected void ClosePanel()
        {
            _view.ClosePanel();
        }

        public virtual void Dispose()
        {
        }
    }
}
