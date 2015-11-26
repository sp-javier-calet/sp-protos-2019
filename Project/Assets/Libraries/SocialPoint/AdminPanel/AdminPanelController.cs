using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelController : UIViewController
    {
        private Stack<IAdminPanelGUI> _activePanels;

        private Text _consoleText;
        private AdminPanelRootLayout _root;
        private ScrollRect _consoleScroll;

        private AdminPanelLayout _mainPanel;
        private AdminPanelLayout _mainPanelContent;
        private AdminPanelLayout _categoriesPanelContent;
        private AdminPanelLayout _consolePanel;

        private bool _consoleEnabled;
        private bool _mainPanelDirty;

        public AdminPanel AdminPanel;

        protected override void OnLoad()
        {
            base.OnLoad();

            _activePanels = new Stack<IAdminPanelGUI>();
            _mainPanelDirty = false;

            AdminPanel.Console.OnContentChanged += () => {
                if(_consoleText != null)
                {
                    _consoleText.text = AdminPanel.Console.Content;
                    if(AdminPanel.Console.FixedFocus)
                    {
                        _consoleScroll.verticalNormalizedPosition = 0.0f;
                    }
                }
            };
        }

        void InflateGUI()
        {
            _root = new AdminPanelRootLayout(this);

            AdminPanelLayout horizontalLayout = _root.CreateHorizontalLayout();
            
            var panelLayout = horizontalLayout.CreatePanelLayout("Admin Panel", () => {
                Hide(false);
            });
            _categoriesPanelContent = panelLayout.CreateVerticalScrollLayout();

            panelLayout.CreateToggleButton("Console", _consoleEnabled, (value) => {
                _consoleEnabled = value;
                RefreshPanel();
            });

            var rightVerticalLayout = horizontalLayout.CreateVerticalLayout(4);

            // Content panel
            _mainPanel = rightVerticalLayout.CreatePanelLayout("Panel", ClosePanel, 2);
            _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();
            _mainPanel.SetActive(false);

            // Console panel
            _consolePanel = rightVerticalLayout.CreatePanelLayout();
            using(var scrollLayout = _consolePanel.CreateVerticalScrollLayout(out _consoleScroll))
            {
                scrollLayout.CreateLabel("Console");
                _consoleText = scrollLayout.CreateTextArea(AdminPanel.Console.Content);
            }
            _consolePanel.SetActive(false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(_root == null)
            {
                InflateGUI();
            }
            RefreshPanel();
            AdminPanel.OnAppearing();
        }

        protected override void OnDisappeared()
        {
            base.OnDisappeared();
            AdminPanel.OnDisappeared();
        }

        public void ReplacePanel(IAdminPanelGUI gui)
        {
            IAdminPanelGUI currentGUI = null;
            if(_activePanels.Count > 0)
            {
                currentGUI = _activePanels.Peek();
            }

            if(currentGUI != gui)
            {
                _activePanels.Clear();
                OpenPanel(gui);
            }
        }

        public void OpenPanel(IAdminPanelGUI panel)
        {
            _activePanels.Push(new AdminPanelGUIGroup(panel));
            _mainPanelDirty = true;
            RefreshPanel();
        }

        public void OpenPanel(AdminPanelGUIGroup panelLayout)
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

        public void RefreshPanel(bool force = false)
        {
            // Categories panel
            foreach(Transform child in _categoriesPanelContent.Parent)
            {
                Destroy(child.gameObject);
            }

            IAdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(AdminPanel.Categories);
            rootPanel.OnCreateGUI(_categoriesPanelContent);

            // Main panel content
            if(_mainPanelDirty || force)
            {
                // Destroy current content and hide main panel
                foreach(Transform child in _mainPanelContent.Parent)
                {
                    Destroy(child.gameObject);
                }
                _mainPanel.SetActive(false);

                // Inflate panel if needed
                if(_activePanels.Count > 0)
                {
                    _activePanels.Peek().OnCreateGUI(_mainPanelContent);
                    _mainPanel.SetActive(true);
                }

                _mainPanelDirty = false;
            }

            // Console
            _consolePanel.SetActive(_consoleEnabled);
        }

        // Categories Panel content
        private class AdminPanelCategoriesGUI : IAdminPanelGUI
        {
            private Dictionary<string, IAdminPanelGUI> _categories;

            public AdminPanelCategoriesGUI(Dictionary<string, IAdminPanelGUI> categories)
            {
                _categories = categories;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                // Inflate categories panel
                foreach(var category in _categories)
                {
                    layout.CreateOpenPanelButton(category.Key, category.Value, true, true);
                }
            }
        }
    }
}
