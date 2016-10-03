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

        public IAdminPanelGUI GUI;
        public GameObject Root;

        [HideInInspector]
        public string Title;

        [HideInInspector]
        public bool Border = true;

        public AdminPanel AdminPanel
        {
            get
            {
                return null;
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
                if(Border)
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
        }

        public void ReplacePanel(IAdminPanelGUI panel)
        {
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
                Border = !Border;
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

        public static FloatingPanelController Create(IFloatingPanelGUI gui)
        {
            var ctrl = UIViewController.Factory.Create<FloatingPanelController>();
            ctrl.GUI = gui;
            if(gui != null)
            {
                gui.OnCreateFloatingPanel(ctrl);
            }
            return ctrl;
        }
    }

    public interface IFloatingPanelGUI : IAdminPanelGUI
    {
        void OnCreateFloatingPanel(FloatingPanelController panel);
    }
}
