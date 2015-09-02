using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        private AdminPanelController _adminPanelController;

        public AdminPanelLayout(AdminPanelLayout parentLayout)
        {
            Parent = parentLayout.Parent;
            _adminPanelController = parentLayout._adminPanelController;
        }
        
        public AdminPanelLayout(RectTransform rectTransform)
        {
            Parent = rectTransform;
        }

        protected AdminPanelLayout(AdminPanelController view)
        {
            _adminPanelController = view;
        }
        
        protected void OpenPanel(AdminPanelGUI panel)
        {
            _adminPanelController.OpenPanel(panel);
        }

        protected void ReplacePanel(AdminPanelGUI panel)
        {
            _adminPanelController.ReplacePanel(panel);
        }

        protected void ClosePanel()
        {
            _adminPanelController.ClosePanel();
        }

        public AdminPanel AdminPanel
        {
            get
            {
                return _adminPanelController.AdminPanel;
            }
        }

        public void SetActive(bool active)
        {
            Parent.gameObject.SetActive(active);
        }

        public void Inflate(AdminPanelGUI gui)
        {
            gui.OnCreateGUI(this);
        }

        public virtual void Dispose()
        {
        }
    }
}
