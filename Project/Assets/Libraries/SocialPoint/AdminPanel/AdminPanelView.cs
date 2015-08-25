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
            _canvasObject.AddComponent<GraphicRaycaster>();
            var image = _canvasObject.AddComponent<Image>();
            image.color = new Color(0.1f, .1f, .1f, .5f);

            AdminPanelLayout rootLayout = new AdminPanelLayout(canvasRectTransform);
            using(AdminPanelLayout horizontalLayout = new HorizontalLayout(rootLayout))
            {
                RectTransform categoriesPanel = AdminPanelGUIUtils.CreatePanel(horizontalLayout, new Vector2(0.2f, 1.0f));

                // Categories panel
                using(AdminPanelLayout categoriesVerticalLayout = new VerticalLayout(new AdminPanelLayout(categoriesPanel)))
                {
                    AdminPanelGUIUtils.CreateLabel(categoriesVerticalLayout, "Admin Panel");

                    AdminPanelGUIUtils.CreateMargin(categoriesVerticalLayout);

                    using(AdminPanelLayout categoriesScrollLayout = new VerticalScrollLayout(categoriesVerticalLayout))
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                        rootPanel.OnCreateGUI(categoriesScrollLayout);
                        
                        AdminPanelGUIUtils.CreateMargin(categoriesScrollLayout);
                    }


                    AdminPanelGUIUtils.CreateMargin(categoriesVerticalLayout);
                    AdminPanelGUIUtils.CreateButton(categoriesVerticalLayout, "Close", () => { Close(); });
                }

                AdminPanelGUIUtils.CreateMargin(horizontalLayout);
                // Right side
                using(AdminPanelLayout rightVerticalLayout = new VerticalLayout(horizontalLayout))
                {
                    RectTransform contentPanel = AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, 0.6f));
                    using(AdminPanelLayout contentVerticalLayout = new VerticalLayout(new AdminPanelLayout(contentPanel)))
                    {
                        if(_activePanels.Count > 0 )
                        {
                            _activePanels.Peek().OnCreateGUI(contentVerticalLayout);
                        }
                    }

                    AdminPanelGUIUtils.CreateMargin(rightVerticalLayout);
                    AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, 0.4f));
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
