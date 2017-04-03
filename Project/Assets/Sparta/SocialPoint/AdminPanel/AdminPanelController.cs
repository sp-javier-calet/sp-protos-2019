#if ADMIN_PANEL

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanelController : BasePanelController
    {
        Stack<IAdminPanelGUI> _activePanels;

        Text _consoleText;
        AdminPanelRootLayout _root;
        ScrollRect _consoleScroll;

        AdminPanelLayout _mainPanel;
        AdminPanelLayout _mainPanelContent;
        AdminPanelLayout _categoriesPanelContent;
        AdminPanelLayout _consolePanel;

        bool _consoleEnabled;
        bool _mainPanelDirty;

        public AdminPanel AdminPanel;

        IAdminPanelGUI CurrentGUI
        {
            get
            {
                IAdminPanelGUI current = null;
                if(_activePanels.Count > 0)
                {
                    current = _activePanels.Peek();
                }
                return current;
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _activePanels = new Stack<IAdminPanelGUI>();
            _mainPanelDirty = false;

            if(AdminPanel == null)
            {
                throw new InvalidOperationException("Admin panel not set.");
            }

            AdminPanel.Console.OnContentChanged += OnContentChanged;
        }

        void InflateGUI()
        {
            _root = new AdminPanelRootLayout(this, transform);

            AdminPanelLayout horizontalLayout = _root.CreateHorizontalLayout();
            
            var panelLayout = horizontalLayout.CreatePanelLayout("Admin Panel", () => Hide());
            _categoriesPanelContent = panelLayout.CreateVerticalScrollLayout();

            CreateConsoleButton(panelLayout);

            var rightVerticalLayout = horizontalLayout.CreateVerticalLayout(4);

            // Content panel
            _mainPanel = rightVerticalLayout.CreatePanelLayout("Panel", ClosePanel, 2);
            _mainPanelContent = _mainPanel.CreateVerticalScrollLayout();
            _mainPanel.SetActive(false);

            // Console panel
            _consolePanel = rightVerticalLayout.CreatePanelLayout();
            var scrollLayout = _consolePanel.CreateVerticalScrollLayout(out _consoleScroll);
            scrollLayout.CreateLabel("Console");
            _consoleText = scrollLayout.CreateTextArea(AdminPanel.Console.Content);

            _consolePanel.SetActive(false);
        }

        void OnContentChanged()
        {
            if(_consoleText != null)
            {
                _consoleText.text = AdminPanel.Console.Content;
                if(AdminPanel.Console.FixedFocus)
                {
                    _consoleScroll.verticalNormalizedPosition = 0.0f;
                }
            }
        }

        void CreateConsoleButton(AdminPanelLayout layout)
        {
            var toggle = layout.CreateToggleButton("Console", _consoleEnabled, value => {
                _consoleEnabled = value;
                RefreshPanel(false);
            });

            // Add feedback component
            var consoleButtonObject = toggle.gameObject;
            var feedback = consoleButtonObject.AddComponent<ConsoleButtonFeedback>();
            feedback.ButtonImage = toggle.targetGraphic as Image;
            feedback.AdminPanel = AdminPanel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(_root == null)
            {
                InflateGUI();
            }
            OpenDefaultPanel();
            AdminPanel.OnAppearing();
        }

        void OpenDefaultPanel()
        {
            if(_activePanels.Count == 0 && AdminPanel.DefaultPanel != null)
            {
                OpenPanel(AdminPanel.DefaultPanel);
            }
            else
            {
                RefreshPanel(false);
            }
        }

        protected override void OnDisappeared()
        {
            base.OnDisappeared();
            AdminPanel.OnDisappeared();
        }

        public override void ReplacePanel(IAdminPanelGUI gui)
        {
            IAdminPanelGUI currentGUI = CurrentGUI;

            if(currentGUI != gui)
            {
                NotifyClosedPanel(currentGUI);
                _activePanels.Clear();

                OpenPanel(gui);
            }
        }

        public override void OpenPanel(IAdminPanelGUI panel)
        {
            if(!(panel is AdminPanelGUIGroup))
            {
                panel = new AdminPanelGUIGroup(panel);
            }

            var current = CurrentGUI;
            _activePanels.Push(panel);
            _mainPanelDirty = true;

            NotifyClosedPanel(current);
            NotifyOpenedPanel(panel);

            RefreshPanel(false);
        }

        public override void ClosePanel()
        {
            var current = _activePanels.Pop();
            _mainPanelDirty = true;

            NotifyClosedPanel(current);
            NotifyOpenedPanel(CurrentGUI);

            RefreshPanel(false);
        }

        public override void RefreshPanel()
        {
            RefreshPanel(true);
        }

        void RefreshPanel(bool force)
        {
            // Categories panel
            if(_categoriesPanelContent != null)
            {
                _categoriesPanelContent.Clear();
            }

            IAdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(AdminPanel.Categories);
            rootPanel.OnCreateGUI(_categoriesPanelContent);

            // Main panel content
            if(_mainPanelDirty || force)
            {
                // Destroy current content and hide main panel
                if(_mainPanelContent != null)
                {
                    _mainPanelContent.Clear();
                }
                _mainPanel.SetActive(false);

                // Inflate panel if needed
                var currentGui = CurrentGUI;
                if(currentGui != null)
                {
                    currentGui.OnCreateGUI(_mainPanelContent);
                    _mainPanel.SetActive(true);
                }

                _mainPanelDirty = false;
            }

            // Console
            _consolePanel.SetActive(_consoleEnabled);
        }

        // Categories Panel content
        class AdminPanelCategoriesGUI : IAdminPanelGUI
        {
            readonly Dictionary<string, IAdminPanelGUI> _categories;

            public AdminPanelCategoriesGUI(Dictionary<string, IAdminPanelGUI> categories)
            {
                _categories = categories;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                // Inflate categories panel
                var itr = _categories.GetEnumerator();
                while(itr.MoveNext())
                {
                    var category = itr.Current;
                    layout.CreateOpenPanelButton(category.Key, category.Value, true, true);
                }
                itr.Dispose();
            }
        }

        /// <summary>
        /// Feedback behaviour for Console Button
        /// </summary>
        class ConsoleButtonFeedback : MonoBehaviour
        {
            const float FeedbackTime = 0.5f;
            readonly Color FeedbackColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);

            float CurrentFeedbackTime;
            AdminPanel _adminPanel;
            Color _initialColor;
            Image _buttonImage;

            public AdminPanel AdminPanel
            {
                set
                {
                    _adminPanel = value;
                    _adminPanel.Console.OnContentChanged += OnContentChanged;
                }
            }

            public Image ButtonImage
            {
                set
                {
                    _buttonImage = value;
                    _initialColor = value.color;
                }
            }

            void OnDestroy()
            {
                if(_adminPanel != null)
                {
                    _adminPanel.Console.OnContentChanged -= OnContentChanged;
                }
            }

            void OnContentChanged()
            {
                CurrentFeedbackTime = FeedbackTime;
            }

            void Update()
            {
                var step = CurrentFeedbackTime / FeedbackTime;

                CurrentFeedbackTime -= Time.deltaTime;
                if(CurrentFeedbackTime < 0)
                {
                    CurrentFeedbackTime = 0;
                }

                if(_buttonImage != null)
                {
                    _buttonImage.color = Color.Lerp(_initialColor, FeedbackColor, step);
                }
            }
        }
    }
}

#endif
