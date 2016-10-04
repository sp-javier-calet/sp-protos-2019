using System;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SocialPoint.AdminPanel
{
    public sealed class FloatingPanelController : BasePanelController, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        AdminPanelRootLayout _root;
        AdminPanelLayout _mainPanel;
        AdminPanelLayout _mainPanelContent;
        bool _dragging;
        IAdminPanelGUI _gui;

        public GameObject Root;

        [HideInInspector]
        public string Title;

        [HideInInspector]
        public bool Border = true;

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
            if(_gui != null)
            {
                _gui.OnCreateGUI(_mainPanelContent);
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

        public override void RefreshPanel()
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

        public override void OpenPanel(IAdminPanelGUI panel)
        {
            ReplacePanel(panel);
        }

        public override void ReplacePanel(IAdminPanelGUI panel)
        {
            _gui = panel;
            RefreshPanel();
        }

        public override void ClosePanel()
        {
            Hide(true);
        }

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
                RefreshPanel();
            }
        }

        public static FloatingPanelController Create(IFloatingPanelGUI gui)
        {
            var ctrl = UIViewController.Factory.Create<FloatingPanelController>();
            ctrl._gui = gui;
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
