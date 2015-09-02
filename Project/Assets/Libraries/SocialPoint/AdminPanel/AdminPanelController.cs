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
        private Stack<AdminPanelGUI> _activePanels;

        private Text _consoleText;
        private GameObject _canvasObject;
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

            _activePanels = new Stack<AdminPanelGUI>();
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
                scrollLayout.CreateTextArea(AdminPanel.Console.Content, out _consoleText);
            }
            _consolePanel.SetActive(false);
        }

        public void Open()
        {
            if(_canvasObject != null)
            {
                _canvasObject.SetActive(true);
            }
            else
            {
                InflateGUI();
            }

            RefreshPanel();
        }

        private void Close()
        {
            _canvasObject.SetActive(false);
        }

        public void ReplacePanel(AdminPanelGUI gui)
        {
            AdminPanelGUI currentGUI = null;
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

        public void OpenPanel(AdminPanelGUI panel)
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

        private void RefreshPanel()
        {
            // Categories panel
            foreach(Transform child in _categoriesPanelContent.Parent)
            {
                Destroy(child.gameObject);
            }

            AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(AdminPanel.Categories);
            rootPanel.OnCreateGUI(_categoriesPanelContent);

            // Main panel content
            if(_mainPanelDirty)
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
        private class AdminPanelCategoriesGUI : AdminPanelGUI
        {
            private Dictionary<string, AdminPanelGUI> _categories;

            public AdminPanelCategoriesGUI(Dictionary<string, AdminPanelGUI> categories)
            {
                _categories = categories;
            }

            public void OnConfigure(AdminPanel adminPanel)
            {
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                // Inflate categories panel
                foreach(var category in _categories)
                {
                    layout.CreateOpenPanelButton(category.Key, category.Value, true);
                }
            }
        }
    }
}
