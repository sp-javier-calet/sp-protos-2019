using System;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public sealed class FloatingPanelController : UIViewController, IAdminPanelController
    {
        AdminPanelRootLayout _root;
        AdminPanelLayout _mainPanel;
        AdminPanelLayout _mainPanelContent;

        public IAdminPanelController Parent;
        public IAdminPanelGUI GUI;
        public GameObject Root;

        [HideInInspector]
        public string Title;

        public AdminPanel AdminPanel
        {
            get
            {
                if(Parent == null)
                {
                    return null;
                }
                return Parent.AdminPanel;
            }
        }

        void OnLevelWasLoaded(int i)
        {
            Hide();
        }

        void InflateGUI()
        {
            _root = new AdminPanelRootLayout(this, Root.transform);
            _mainPanel = _root.CreatePanelLayout(Title, ClosePanel, 2);
            _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();

            if(GUI != null)
            {
                GUI.OnCreateGUI(_mainPanelContent);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(_root == null)
            {
                InflateGUI();
            }
        }

        public void RefreshPanel(bool force = false)
        {
            
        }

        public void OpenPanel(IAdminPanelGUI panel)
        {
            if(Parent != null)
            {
                Parent.OpenPanel(panel);
            }
        }

        public void ReplacePanel(IAdminPanelGUI panel)
        {
            GUI = panel;
        }

        public void OpenFloatingPanel(IAdminPanelGUI panel, FloatingPanelOptions options)
        {
            if(panel != null)
            {
                Parent.OpenFloatingPanel(panel, options);
            }
        }

        public void ClosePanel()
        {
            Hide(true);
        }
    }
}
