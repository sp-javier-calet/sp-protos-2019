using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        private AdminPanelController _adminPanelController;

        public AdminPanel AdminPanel
        {
            get
            {
                return _adminPanelController.AdminPanel;
            }
        }

        public MonoBehaviour Behaviour
        {
            get
            {
                return _adminPanelController;
            }
        }

        /// <summary>
        /// Check if the game object is active in the scene
        /// </summary>
        /// <value><c>true</c> if this instance is active in scene hierarchy; otherwise, <c>false</c>.</value>
        public bool IsActiveInHierarchy
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

        protected AdminPanelLayout(AdminPanelController controller)
        {
            _adminPanelController = controller;
        }

        public void Refresh()
        {
            _adminPanelController.RefreshPanel(true);
        }

        protected void OpenPanel(IAdminPanelGUI panel)
        {
            _adminPanelController.OpenPanel(panel);
        }

        protected void ReplacePanel(IAdminPanelGUI panel)
        {
            _adminPanelController.ReplacePanel(panel);
        }

        protected void ClosePanel()
        {
            _adminPanelController.ClosePanel();
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
