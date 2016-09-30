using System;
using UnityEngine;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        private IAdminPanelController _adminPanelController;

        public AdminPanel AdminPanel
        {
            get
            {
                return _adminPanelController.AdminPanel;
            }
        }

        /// <summary>
        /// Check if the game object is active in the scene
        /// </summary>
        /// <value><c>true</c> if this instance is active in scene hierarchy; otherwise, <c>false</c>.</value>
        bool IsActiveInHierarchy
        {
            get
            { 
                return Parent.gameObject.activeInHierarchy; 
            }
        }

        public AdminPanelLayout(AdminPanelLayout parentLayout)
        {
            Parent = parentLayout.Parent;
            _adminPanelController = parentLayout._adminPanelController;
        }

        public AdminPanelLayout(RectTransform rectTransform)
        {
            Parent = rectTransform;
        }

        protected AdminPanelLayout(IAdminPanelController controller)
        {
            _adminPanelController = controller;
        }

        public void Refresh()
        {
            if(IsActiveInHierarchy)
            {
                _adminPanelController.RefreshPanel(true);
            }
        }

        public void OpenPanel(IAdminPanelGUI panel)
        {
            _adminPanelController.OpenPanel(panel);
        }

        public void ReplacePanel(IAdminPanelGUI panel)
        {
            _adminPanelController.ReplacePanel(panel);
        }

        public void ClosePanel()
        {
            _adminPanelController.ClosePanel();
        }

        public void OpenFloatingPanel(IAdminPanelGUI panel, FloatingPanelOptions options)
        {
            _adminPanelController.OpenFloatingPanel(panel, options);
        }

        public void SetActive(bool active)
        {
            Parent.gameObject.SetActive(active);
        }

        public virtual void Dispose()
        {
        }
    }
}
