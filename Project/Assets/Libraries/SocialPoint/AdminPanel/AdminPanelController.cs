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

        private AdminPanelLayout _mainPanel;
        private AdminPanelLayout _mainPanelContent;
        private AdminPanelLayout _categoriesPanelContent;
        private AdminPanelLayout _consolePanel;
        private bool _mainPanelDirty;


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

            _canvasObject = rootLayout.Parent.gameObject;

            AdminPanelLayout horizontalLayout = rootLayout.CreateHorizontalLayout();
            
            var panelLayout = horizontalLayout.CreatePanelLayout("Admin Panel", () => { Close(); });
            _categoriesPanelContent = panelLayout.CreateVerticalScrollLayout();

            panelLayout.CreateToggleButton("Console", _consoleEnabled, (value) => {
                _consoleEnabled = value;
                RefreshPanel();
            });

            var rightVerticalLayout = horizontalLayout.CreateVerticalLayout(4);

            // Content panel
            _mainPanel = rightVerticalLayout.CreatePanelLayout("Panel", () => { ClosePanel(); }, 2);
            _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();
            _mainPanel.SetActive(false);

            // Console panel
            _consolePanel = rightVerticalLayout.CreatePanelLayout();
            using(var scrollLayout = _consolePanel.CreateVerticalScrollLayout(out _consoleScroll))
            {
                scrollLayout.CreateLabel("Console");
                scrollLayout.CreateTextArea(_console.Content, out _consoleText);
            }
            _consolePanel.SetActive(_consoleEnabled);
        }

        public void Open()
        {
            if(_canvasObject != null)
            {
                _canvasObject.SetActive(true);
            }
            else
            {
                // Load Layout data through handler
                _categories = new Dictionary<string, AdminPanelGUILayout>();
                AdminPanelHandler.InitializeHandler(new AdminPanelHandler(_categories, _consoleApplication));
                InflateGUI();
            }

            RefreshPanel();
        }

        private void Close()
        {
            _canvasObject.SetActive(false);
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

            foreach(Transform child in _categoriesPanelContent.Parent)
            {
                Destroy(child.gameObject);
            }

            AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
            rootPanel.OnCreateGUI(_categoriesPanelContent);

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
                    //InflateCategory(layout, category.Key, category.Value);
                    layout.CreateOpenPanelButton(category.Key, category.Value, true);
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
