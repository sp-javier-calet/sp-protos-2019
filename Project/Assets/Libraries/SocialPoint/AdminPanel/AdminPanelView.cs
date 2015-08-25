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
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new GameObject("AdminPanel - Category Panel");
            var image = panelObject.AddComponent<Image>();
            panelObject.transform.SetParent(canvasObject.transform);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.pivot = new Vector2(0.5f, 1.0f);
            image.rectTransform.anchoredPosition = Vector3.zero;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            image.rectTransform.sizeDelta = new Vector2(0.3f, 1.0f);
            image.color = new Color(1f, .3f, .3f, .5f);

            GameObject scrollObject = new GameObject("AdminPanel - Scroll View");
            scrollObject.transform.SetParent(panelObject.transform);
            RectTransform scrollRectTrans = scrollObject.AddComponent<RectTransform>();
            scrollRectTrans.anchorMin = new Vector2(0.0f, 0.0f);
            scrollRectTrans.anchorMax = new Vector2(1.0f, 1.0f);
            scrollRectTrans.pivot = new Vector2(0.5f, 1.0f);
            scrollRectTrans.localPosition = Vector2.zero;
            scrollRectTrans.offsetMin = Vector2.zero;
            scrollRectTrans.offsetMax = Vector2.zero;
            ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;


            GameObject scrollContentObject = new GameObject("AdminPanel - Scroll Content");
            scrollContentObject.transform.SetParent(scrollObject.transform);
            RectTransform rectTrans = scrollContentObject.AddComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
            rectTrans.anchorMax = new Vector2(1.0f, 0.5f);
            rectTrans.offsetMin = new Vector2(0.0f, -50.0f);
            rectTrans.offsetMax = new Vector2(0.0f, 0.0f);
            rectTrans.pivot = new Vector2(0.5f, 1.0f);
            rectTrans.localPosition = Vector2.zero;
            scroll.content = rectTrans;



            using(AdminPanelLayout layout = new AdminPanelLayout(scrollContentObject.transform))
            {
                AdminPanelGUIUtils.CreateLabel(layout, "Admin Panel");
                AdminPanelGUIUtils.CreateMargin(layout);
                AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(categories);
                rootPanel.OnCreateGUI(layout);

                AdminPanelGUIUtils.CreateMargin(layout);
                AdminPanelGUIUtils.CreateButton(layout, "Close", () => { Close(); });
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
