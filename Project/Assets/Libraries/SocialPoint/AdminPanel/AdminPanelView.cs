using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelView : MonoBehaviour
    {
        private Dictionary<string, AdminPanelGUILayout> _categories;
        private Stack<AdminPanelGUILayout> _activePanels;
        private GameObject _canvasObject;
        private bool _consoleEnabled;
        void Awake()
        {
            _categories = new Dictionary<string, AdminPanelGUILayout>();
            _activePanels = new Stack<AdminPanelGUILayout>();
        }

        bool inflated = false;
        void Update()
        {
            if(!inflated && Input.GetKeyDown("a"))
            {
                inflated = true;
                // Load Layout data through handler
                _categories = new Dictionary<string, AdminPanelGUILayout>();
                AdminPanelHandler.InitializeHandler(new AdminPanelHandler(_categories));
                InflateGUI();
            }

            if(inflated && Input.GetKeyDown("q"))
            {
                Close();
            }
        }

        void InflateGUI()
        {
            // Create GUI base
            _canvasObject = new GameObject("AdminPanel - Canvas");
            RectTransform canvasRectTransform = _canvasObject.AddComponent<RectTransform>();
            Canvas canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasObject.AddComponent<CanvasScaler>();
            _canvasObject.AddComponent<GraphicRaycaster>();

            AdminPanelLayout rootLayout = new AdminPanelLayout(canvasRectTransform);
            using(AdminPanelLayout horizontalLayout = new HorizontalLayout(rootLayout))
            {
                RectTransform categoriesPanel = AdminPanelGUIUtils.CreatePanel(horizontalLayout, new Vector2(0.25f, 1.0f));

                // Categories panel
                using(AdminPanelLayout categoriesVerticalLayout = new VerticalLayout(new AdminPanelLayout(categoriesPanel)))
                {
                    AdminPanelGUIUtils.CreateLabel(categoriesVerticalLayout, "Admin Panel");
                    AdminPanelGUIUtils.CreateMargin(categoriesVerticalLayout);

                    using(AdminPanelLayout categoriesScrollLayout = new VerticalScrollLayout(categoriesVerticalLayout, new Vector2(1.0f, 0.5f)))
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                        rootPanel.OnCreateGUI(categoriesScrollLayout);
                    }

                    AdminPanelGUIUtils.CreateButton(categoriesVerticalLayout, 
                                                    "Console", () => { 
                                                        _consoleEnabled = !_consoleEnabled; RefreshPanel();
                                                    });

                    AdminPanelGUIUtils.CreateButton(categoriesVerticalLayout, "Close", () => { Close(); });
                }

                // Right side
                if(_activePanels.Count > 0 || _consoleEnabled )
                {
                    // Panel/Console sizes
                    float mainPanelSize = (_consoleEnabled)? 0.6f : 1.0f;
                    if(_activePanels.Count == 0)
                    {
                        mainPanelSize = 0.0f;
                    }

                    using(AdminPanelLayout rightVerticalLayout = new VerticalLayout(horizontalLayout))
                    {
                        if(_activePanels.Count > 0)
                        {
                            RectTransform contentPanel = AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, mainPanelSize));
                            using(VerticalScrollLayout contentVerticalLayout = new VerticalScrollLayout(new AdminPanelLayout(contentPanel)))
                            {
                                _activePanels.Peek().OnCreateGUI(contentVerticalLayout);
                            }
                        }

                        if(_consoleEnabled)
                        {
                            AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, 1.0f - mainPanelSize));
                        }
                    }
                }
            }
        }

        private void Close()
        {
            Destroy(_canvasObject);
            inflated = false;
        }

        public void OpenPanel(AdminPanelGUILayout panelLayout)
        {
            _activePanels.Push(panelLayout);
            RefreshPanel();
        }

        public void ClosePanel()
        {
            _activePanels.Pop();
            RefreshPanel();
        }

        private void RefreshPanel()
        {
            Close();
            InflateGUI();

            // Check if console active. Draw right panel and
            //_activePanels.Peek().OnCreateGUI();
        }

        private class AdminPanelCategoriesGUI : AdminPanelGUI
        {
            private Dictionary<string, AdminPanelGUILayout> _categories;
            private AdminPanelView _view;

            public AdminPanelCategoriesGUI(AdminPanelView view, Dictionary<string, AdminPanelGUILayout> categories)
            {
                _view = view;
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
                AdminPanelGUIUtils.CreateButton(layout, categoryLabel, () => {
                    _view.OpenPanel(panelLayout);
                });
            }
        }
    }
}
