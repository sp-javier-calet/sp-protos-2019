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
                RectTransform categoriesPanel = AdminPanelGUIUtils.CreatePanel(horizontalLayout, new Vector2(0.25f, 1.0f), ()=>{Close();});

                // Categories panel
                using(AdminPanelLayout categoriesVerticalLayout = new VerticalLayout(new AdminPanelLayout(categoriesPanel)))
                {
                    AdminPanelGUIUtils.CreateLabel(categoriesVerticalLayout, "Admin Panel");
                    AdminPanelGUIUtils.CreateMargin(categoriesVerticalLayout);
                    AdminPanelGUIUtils.CreateToggleButton(categoriesVerticalLayout, "Console", _consoleEnabled, (value) => { 
                        _consoleEnabled = value; 
                        RefreshPanel();
                    });

                    AdminPanelGUIUtils.CreateMargin(categoriesVerticalLayout);
                    
                    using(AdminPanelLayout categoriesScrollLayout = new VerticalScrollLayout(categoriesVerticalLayout))
                    {
                        AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                        rootPanel.OnCreateGUI(categoriesScrollLayout);
                    }
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
                            RectTransform contentPanel = AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, mainPanelSize), ()=>{ ClosePanel();});
                            using(VerticalScrollLayout contentVerticalLayout = new VerticalScrollLayout(new AdminPanelLayout(contentPanel)))
                            {
                                _activePanels.Peek().OnCreateGUI(contentVerticalLayout);
                            }
                        }

                        if(_consoleEnabled)
                        {
                            RectTransform contentPanel = AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, 1.0f - mainPanelSize));
                            using(var consoleLayout = new VerticalLayout(new AdminPanelLayout(contentPanel), new Vector2(1.0f, 0.8f)))
                            {
                                using(var contentVerticalLayout = new VerticalScrollLayout(consoleLayout))
                                {
                                    // Console
                                    GameObject textAreaObject = new GameObject("AdminPanel - Text Area");
                                    
                                    var rectTransform = textAreaObject.AddComponent<RectTransform>();
                                    textAreaObject.transform.SetParent(contentVerticalLayout.Parent);
                                    
                                    rectTransform.offsetMin = Vector2.zero;
                                    rectTransform.offsetMax = Vector2.zero;
                                    rectTransform.pivot = Vector2.up;
                                    
                                    rectTransform.anchorMin = Vector2.up;
                                    rectTransform.anchorMax = Vector2.up;
                                    rectTransform.sizeDelta = new Vector2(contentVerticalLayout.Parent.rect.width * 0.95f /* relativeSize.x*/, contentVerticalLayout.Parent.rect.height /* relativeSize.y*/);
                                    
                                    Vector2 margin = new Vector2(contentVerticalLayout.Parent.rect.width * 0.05f, -5.0f); // FIXME MARGINS
                                    rectTransform.anchoredPosition = contentVerticalLayout.Position + margin;

                                    var text = textAreaObject.AddComponent<Text>();

                                    text.text = "A Scroll Rect is usually used to scroll a large image or panel of another UI element\n, such as a list of buttons or large block of text. The Scroll Rect is most often used with a mask element, and is designed to work seamlessly with scrollbars. To scroll content, the input must be received from inside the bounds of the ScrollRect, not on the content itself. The Scroll Rect is commonly used with a mask element. Add an image script for the mask to use, and then add a mask script. The mask elements will use the image to create its mask. A specific image is not needed on the image script, but one can be added for additional control over the shape of the mask. Take care when using Unrestricted scrolling movement as it is possible to lose control of the content in an irretrievable way. When using Elastic or Constrained movement it is best to position the content so that it starts within the bounds of the ScrollRect, or undesirable behaviour may occur as the RectTransform tries to bring the content back within its bounds.";
                                    text.text += "A Scroll Rect is usually used to scroll a large image or panel of another UI element\n, such as a list of buttons or large block of text. The Scroll Rect is most often used with a mask element, and is designed to work seamlessly with scrollbars. To scroll content, the input must be received from inside the bounds of the ScrollRect, not on the content itself. The Scroll Rect is commonly used with a mask element. Add an image script for the mask to use, and then add a mask script. The mask elements will use the image to create its mask. A specific image is not needed on the image script, but one can be added for additional control over the shape of the mask. Take care when using Unrestricted scrolling movement as it is possible to lose control of the content in an irretrievable way. When using Elastic or Constrained movement it is best to position the content so that it starts within the bounds of the ScrollRect, or undesirable behaviour may occur as the RectTransform tries to bring the content back within its bounds.";
                                    text.text += "A Scroll Rect is usually used to scroll a large image or panel of another UI element\n, such as a list of buttons or large block of text. The Scroll Rect is most often used with a mask element, and is designed to work seamlessly with scrollbars. To scroll content, the input must be received from inside the bounds of the ScrollRect, not on the content itself. The Scroll Rect is commonly used with a mask element. Add an image script for the mask to use, and then add a mask script. The mask elements will use the image to create its mask. A specific image is not needed on the image script, but one can be added for additional control over the shape of the mask. Take care when using Unrestricted scrolling movement as it is possible to lose control of the content in an irretrievable way. When using Elastic or Constrained movement it is best to position the content so that it starts within the bounds of the ScrollRect, or undesirable behaviour may occur as the RectTransform tries to bring the content back within its bounds.";
                                    text.text += "A Scroll Rect is usually used to scroll a large image or panel of another UI element\n, such as a list of buttons or large block of text. The Scroll Rect is most often used with a mask element, and is designed to work seamlessly with scrollbars. To scroll content, the input must be received from inside the bounds of the ScrollRect, not on the content itself. The Scroll Rect is commonly used with a mask element. Add an image script for the mask to use, and then add a mask script. The mask elements will use the image to create its mask. A specific image is not needed on the image script, but one can be added for additional control over the shape of the mask. Take care when using Unrestricted scrolling movement as it is possible to lose control of the content in an irretrievable way. When using Elastic or Constrained movement it is best to position the content so that it starts within the bounds of the ScrollRect, or undesirable behaviour may occur as the RectTransform tries to bring the content back within its bounds.";
                                    text.text += "A Scroll Rect is usually used to scroll a large image or panel of another UI element\n, such as a list of buttons or large block of text. The Scroll Rect is most often used with a mask element, and is designed to work seamlessly with scrollbars. To scroll content, the input must be received from inside the bounds of the ScrollRect, not on the content itself. The Scroll Rect is commonly used with a mask element. Add an image script for the mask to use, and then add a mask script. The mask elements will use the image to create its mask. A specific image is not needed on the image script, but one can be added for additional control over the shape of the mask. Take care when using Unrestricted scrolling movement as it is possible to lose control of the content in an irretrievable way. When using Elastic or Constrained movement it is best to position the content so that it starts within the bounds of the ScrollRect, or undesirable behaviour may occur as the RectTransform tries to bring the content back within its bounds.";

                                    text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
                                    text.fontSize = 15;
                                    text.color = Color.white;
                                    text.alignment = TextAnchor.UpperLeft;
                                    text.resizeTextForBestFit = true;

                                }

                                using(var promptLayout = new HorizontalLayout(consoleLayout))
                                {
                                    AdminPanelGUIUtils.CreateButton(promptLayout, "Input", () => {});
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Close()
        {
            //_activePanels.Clear();
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
