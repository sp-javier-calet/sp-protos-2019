using System;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SocialPoint.AdminPanel
{
    public sealed class FloatingPanelController : UIViewController, IAdminPanelController, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        AdminPanelRootLayout _root;
        AdminPanelLayout _mainPanel;
        AdminPanelLayout _mainPanelContent;

        public IAdminPanelController Parent;
        public IAdminPanelGUI GUI;
        public GameObject Root;

        [HideInInspector]
        public string Title;

        [HideInInspector]
        bool ShowBorder = true;

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
            if(_root == null)
            {
                _root = new AdminPanelRootLayout(this, Root.transform);
                _mainPanel = null;
            }
            if(_mainPanel == null)
            {    
                if(ShowBorder)
                {
                    _mainPanel = _root.CreatePanelLayout(Title, ClosePanel, 2);
                }
                else
                {
                    _mainPanel = _root;
                }
                _mainPanelContent = null;
            }
            InflateContent();
        }

        void InflateContent()
        {
            if(_mainPanelContent == null)
            {
                _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();
            }
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
            if(_root != null)
            {
                _root.Dispose();
                _root = null;
            }
            _mainPanel = null;
            _mainPanelContent = null;
            InflateGUI();
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

        bool _dragging;

        public void OnDrag(PointerEventData eventData)
        {
            _dragging = true;
            var p = ScreenPosition;
            p.x += eventData.delta.x;
            p.y += eventData.delta.y;
            ScreenPosition = p;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!_dragging)
            {
                ShowBorder = !ShowBorder;
                RefreshPanel(true);
            }
        }

        void Update()
        {
            for(var i = 0; i < _updateables.Count; i++)
            {
                _updateables[i].Update();
            }
        }

        List<IUpdateable> _updateables = new List<IUpdateable>();

        public void RegisterUpdateable(IUpdateable updateable)
        {
            if(updateable != null && !_updateables.Contains(updateable))
            {
                _updateables.Add(updateable);
            }
        }

        public void UnregisterUpdateable(IUpdateable updateable)
        {
            _updateables.Remove(updateable);
        }
    }
}
