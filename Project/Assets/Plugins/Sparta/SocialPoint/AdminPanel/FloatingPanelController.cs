#if ADMIN_PANEL

using SocialPoint.GUIControl;
using UnityEngine;
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
                NotifyOpenedPanel(_gui);
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
            if(_gui != panel)
            {
                if(_gui != null)
                {
                    NotifyClosedPanel(_gui);
                }

                _gui = panel;

                if(panel != null)
                {
                    NotifyOpenedPanel(panel);
                }

                RefreshPanel();
            }
        }

        public override void ClosePanel()
        {
            NotifyClosedPanel(_gui);
            Close();
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

        public static FloatingPanelController Create(IAdminPanelGUI gui, string prefab=null)
        {
            var ctrl = UIViewController.Factory.Create<FloatingPanelController>(prefab);
            ctrl._gui = gui;
            var fgui = gui as IFloatingPanelGUI;
            if(fgui != null)
            {
                fgui.OnCreateFloatingPanel(ctrl);
            }
            return ctrl;
        }


    }

    public interface IFloatingPanelGUI : IAdminPanelGUI
    {
        void OnCreateFloatingPanel(FloatingPanelController panel);
    }
}

#endif
