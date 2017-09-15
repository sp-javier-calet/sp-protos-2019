#if ADMIN_PANEL

using System;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.AdminPanel
{
    public partial class AdminPanelLayout : IDisposable
    {
        public RectTransform Parent { get; protected set; }

        private IPanelController _adminPanelController;

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

        protected AdminPanelLayout(IPanelController controller)
        {
            _adminPanelController = controller;
        }

        public void Refresh()
        {
            if(IsActiveInHierarchy)
            {
                _adminPanelController.RefreshPanel();
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

        public void SetActive(bool active)
        {
            Parent.gameObject.SetActive(active);
        }

        public virtual void Dispose()
        {
            if(Parent != null)
            {
                UnityEngine.Object.Destroy(Parent.gameObject);
            }
        }

        public void RegisterUpdateable(IUpdateable updateable)
        {
            _adminPanelController.RegisterUpdateable(updateable);
        }

        public void UnregisterUpdateable(IUpdateable updateable)
        {
            _adminPanelController.UnregisterUpdateable(updateable);
        }

        public void Clear()
        {
            if(Parent == null)
            {
                return;
            }
            var itr = Parent.GetEnumerator();
            while(itr.MoveNext())
            {
                var child = (Transform)itr.Current;
                if(child != null)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }
    }
}

#endif
