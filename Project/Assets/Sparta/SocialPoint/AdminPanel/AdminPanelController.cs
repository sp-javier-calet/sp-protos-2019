using System;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.AdminPanel
{
    public sealed class AdminPanelController : UIViewController, IAdminPanelController
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

        public AdminPanel AdminPanel{ get; set; }

        protected override void OnLoad()
        {
            base.OnLoad();

            _activePanels = new Stack<IAdminPanelGUI>();
            _mainPanelDirty = false;

            if(AdminPanel == null)
            {
                throw new InvalidOperationException("Admin panel not set.");
            }

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

        void OnLevelWasLoaded(int i)
        {
            Hide();
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
            using(var scrollLayout = _consolePanel.CreateVerticalScrollLayout(out _consoleScroll))
            {
                scrollLayout.CreateLabel("Console");
                _consoleText = scrollLayout.CreateTextArea(AdminPanel.Console.Content);
            }
            _consolePanel.SetActive(false);
        }

        void CreateConsoleButton(AdminPanelLayout layout)
        {
            var toggle = layout.CreateToggleButton("Console", _consoleEnabled, value => {
                _consoleEnabled = value;
                RefreshPanel();
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
            if(!(panel is AdminPanelGUIGroup))
            {
                panel = new AdminPanelGUIGroup(panel);
            }
            _activePanels.Push(panel);
            _mainPanelDirty = true;
            RefreshPanel();
        }

        public void ClosePanel()
        {
            _activePanels.Pop();
            _mainPanelDirty = true;
            RefreshPanel();
        }

        public void OpenFloatingPanel(IAdminPanelGUI panel, FloatingPanelOptions options)
        {
            var ctrl = Factory.Create<FloatingPanelController>();
            ctrl.Parent = this;
            ctrl.GUI = panel;
            ctrl.Size = options.Size;
            ctrl.Title = options.Title;
            ctrl.SetParent(transform.parent);
            ctrl.Show();
        }

        public void RefreshPanel(bool force = false)
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

        // Categories Panel content
        class AdminPanelCategoriesGUI : IAdminPanelGUI
        {
            Dictionary<string, IAdminPanelGUI> _categories;

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

    public struct FloatingPanelOptions
    {
        public Vector2 Size;
        public string Title;
    }

    public interface IAdminPanelController
    {
        void RefreshPanel(bool force = false);
        void OpenPanel(IAdminPanelGUI panel);
        void ReplacePanel(IAdminPanelGUI panel);
        void ClosePanel();
        void OpenFloatingPanel(IAdminPanelGUI panel, FloatingPanelOptions options);
        AdminPanel AdminPanel{ get; }
        void RegisterUpdateable(IUpdateable updateable);
        void UnregisterUpdateable(IUpdateable updateable);
    }
}
