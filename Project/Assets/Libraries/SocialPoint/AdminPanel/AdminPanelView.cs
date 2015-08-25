using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelView : MonoBehaviour
    {
        private Dictionary<string, AdminPanelGUILayout> categories;

        void Awake()
        {
            categories = new Dictionary<string, AdminPanelGUILayout>();
        }

        bool inflated = false;
        void Update()
        {
            if(!inflated && Input.GetKeyDown("a"))
            {
                inflated = true;
                InflateGUI();
            }
        }

        void InflateGUI()
        {
            // Load Layout data through handler
            AdminPanelHandler.InitializeHandler(new AdminPanelHandler(categories));

            // Create GUI base
            GameObject canvasObject = new GameObject("AdminPanel - Canvas");
            RectTransform canvasRectTransform = canvasObject.AddComponent<RectTransform>();
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();


            using(AdminPanelLayout mainLayout = new HorizontalLayout(canvasRectTransform))
            {
                /*GameObject panelObject = new GameObject("AdminPanel - Category Panel");
                var image = panelObject.AddComponent<Image>();
                panelObject.transform.SetParent(mainLayout.Parent);
                image.rectTransform.anchorMin = Vector2.zero;
                image.rectTransform.anchorMax = Vector2.one;
                image.rectTransform.pivot = new Vector2(0.0f, 1.0f);
                image.rectTransform.anchoredPosition = Vector3.zero;
                image.rectTransform.offsetMin = Vector2.zero;
                image.rectTransform.offsetMax = Vector2.zero;
                image.rectTransform.sizeDelta = new Vector2(-canvas.pixelRect.width * 0.7f, 0.0f);
                image.color = new Color(1f, .3f, .3f, .5f);*/

                RectTransform panelTransform = AdminPanelGUIUtils.CreatePanel(mainLayout, new Vector2(0.3f, 0.0f));

                using(AdminPanelLayout mainVerticalLayout = new VerticalLayout(panelTransform))
                {
                    AdminPanelGUIUtils.CreateLabel(mainVerticalLayout, "Admin Panel");
                    AdminPanelGUIUtils.CreateMargin(mainVerticalLayout);

                    using(AdminPanelLayout layout = new VerticalScrollLayout(mainVerticalLayout))
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(categories);
                        rootPanel.OnCreateGUI(layout);

                        AdminPanelGUIUtils.CreateMargin(layout);
                        AdminPanelGUIUtils.CreateButton(layout, "Close", () => { Close(); });
                    }
                }



                panelTransform = AdminPanelGUIUtils.CreatePanel(mainLayout, new Vector2(0.7f, 0.0f));
                using(AdminPanelLayout mainVerticalLayout = new VerticalLayout(panelTransform))
                {
                    AdminPanelGUIUtils.CreateLabel(mainVerticalLayout, "Admin Panel");
                    AdminPanelGUIUtils.CreateMargin(mainVerticalLayout);
                    
                    using(AdminPanelLayout layout = new VerticalScrollLayout(mainVerticalLayout))
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(categories);
                        rootPanel.OnCreateGUI(layout);
                        
                        AdminPanelGUIUtils.CreateMargin(layout);
                        AdminPanelGUIUtils.CreateButton(layout, "Close", () => { Close(); });
                    }
                }
            }
        }

        private void Close()
        {
            Debug.Log("Closing Admin Panel");
        }

        private class AdminPanelCategoriesGUI : AdminPanelGUI
        {
            private Dictionary<string, AdminPanelGUILayout> _categories;
            public AdminPanelCategoriesGUI(Dictionary<string, AdminPanelGUILayout> categories)
            {
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
                    Debug.Log("Opening category " + categoryLabel);
                });

                // Test
                panelLayout.OnCreateGUI(layout);
            }
        }
    }
}
