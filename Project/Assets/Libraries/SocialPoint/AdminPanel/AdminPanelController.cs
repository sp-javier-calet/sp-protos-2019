using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Console;
using SocialPoint.GUI;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelController : UIViewController
    {
        private Dictionary<string, AdminPanelGUILayout> _categories;
        private Stack<AdminPanelGUILayout> _activePanels;
        private AdminPanelConsole _console;
        private ScrollRect _consoleScroll;
        private Text _consoleText;
        private GameObject _canvasObject;
        private bool _consoleEnabled;
        private ConsoleApplication _consoleApplication;



        private AdminPanelLayout _rightSideLayout;
        private AdminPanelLayout _mainHorizontalLayout;
        private AdminPanelLayout _mainPanel;
        private AdminPanelLayout _mainPanelContent;
        private bool _mainPanelDirty;
        private AdminPanelLayout _consolePanel;


        protected override void OnLoad()
        {
            base.OnLoad();

            _categories = new Dictionary<string, AdminPanelGUILayout>();
            _activePanels = new Stack<AdminPanelGUILayout>();
            _consoleApplication = new ConsoleApplication();
            _mainPanelDirty = false;
            
            // Move and set external console
            _console = new AdminPanelConsole(_consoleApplication);
            AdminPanelGUI.AdminPanelConsole = _console; // FIXME
            
            _console.OnContentChanged += () => {
                if(_consoleText != null)
                {
                    _consoleText.text = _console.Content;
                    if(_console.FixedFocus)
                    {
                        _consoleScroll.verticalNormalizedPosition = 0.0f;
                    }
                }
            };

            Open();
        }

        void InflateGUI()
        {
            AdminPanelLayout rootLayout = new AdminPanelRootLayout(this);
            if(_canvasObject == null)
            {
                _canvasObject = rootLayout.Parent.gameObject;

                AdminPanelLayout horizontalLayout = rootLayout.CreateHorizontalLayout();
                
                    var panelLayout = horizontalLayout.CreatePanelLayout("Admin Panel", () => { Close(); });

                    using(var scrollLayout = panelLayout.CreateVerticalScrollLayout())
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                        rootPanel.OnCreateGUI(scrollLayout);
                    }

                    panelLayout.CreateToggleButton("Console", _consoleEnabled, (value) => {
                        _consoleEnabled = value;
                        RefreshPanel();
                    });

                    _mainHorizontalLayout = horizontalLayout;
            }

            if(_rightSideLayout != null)
            {
                Destroy(_rightSideLayout.Parent.gameObject);
            }
            _rightSideLayout = _mainHorizontalLayout.CreateVerticalLayout(4);

            // Content panel
            _mainPanel = _rightSideLayout.CreatePanelLayout("Panel", () => { ClosePanel(); }, 2);
            _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();
            _mainPanel.SetActive(false);

            // Console panel
            _consolePanel = _rightSideLayout.CreatePanelLayout();
            using(var scrollLayout = _consolePanel.CreateVerticalScrollLayout(out _consoleScroll))
            {
                scrollLayout.CreateLabel("Console");
                scrollLayout.CreateTextArea(_console.Content, out _consoleText);
            }
            _consolePanel.SetActive(_consoleEnabled);

        }

        private void Open()
        {
            if(_canvasObject == null)
            {
                // Load Layout data through handler
                _categories = new Dictionary<string, AdminPanelGUILayout>();
                AdminPanelHandler.InitializeHandler(new AdminPanelHandler(_categories, _consoleApplication));
                InflateGUI();
            }
        }

        private void Close()
        {
            Hide(false);
            //Destroy(_canvasObject);
            //_canvasObject = null;
            _consoleText = null;
        }

        public void ReplacePanel(AdminPanelGUILayout panelLayout)
        {
            AdminPanelGUILayout currentLayout = null;
            if(_activePanels.Count > 0)
            {
                currentLayout = _activePanels.Peek();
            }

            if(currentLayout != panelLayout)
            {
                _activePanels.Clear();
                OpenPanel(panelLayout);
            }
        }

        public void OpenPanel(AdminPanelGUI panel)
        {
            _activePanels.Push(new AdminPanelGUILayout(panel));
            _mainPanelDirty = true;
            RefreshPanel();
        }

        public void OpenPanel(AdminPanelGUILayout panelLayout)
        {
            _activePanels.Push(panelLayout);
            _mainPanelDirty = true;
            RefreshPanel();
        }

        public void ClosePanel()
        {
            _activePanels.Pop();
            _mainPanelDirty = true;
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            if(_mainPanelDirty)
            {
                foreach(Transform child in _mainPanelContent.Parent)
                {
                    Destroy(child.gameObject);
                }

                _mainPanel.SetActive(false);

                if(_activePanels.Count > 0)
                {
                    _mainPanel.SetActive(true);
                    _activePanels.Peek().OnCreateGUI(_mainPanelContent);
                }
            }

            _consolePanel.SetActive(_consoleEnabled);
        }

        // Categories Panel content
        private class AdminPanelCategoriesGUI : AdminPanelGUI
        {
            private Dictionary<string, AdminPanelGUILayout> _categories;
            private AdminPanelController _adminPanelController;

            public AdminPanelCategoriesGUI(AdminPanelController controller, Dictionary<string, AdminPanelGUILayout> categories)
            {
                _adminPanelController = controller;
                _categories = categories;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                // Inflate categories panel
                foreach(var category in _categories)
                {
                    InflateCategory(layout, category.Key, category.Value);
                }
            }

            private void InflateCategory(AdminPanelLayout layout, string categoryLabel, AdminPanelGUILayout panelLayout)
            {
                layout.CreateButton(categoryLabel, () => {
                    _adminPanelController.ReplacePanel(panelLayout);
                });
            }
        }
    }
}
