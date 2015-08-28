﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelView : MonoBehaviour
    {
        private Dictionary<string, AdminPanelGUILayout> _categories;
        private Stack<AdminPanelGUILayout> _activePanels;
        private AdminPanelConsole _console;
        private Text _consoleText;
        private GameObject _canvasObject;
        private bool _consoleEnabled;

        void Awake()
        {
            _categories = new Dictionary<string, AdminPanelGUILayout>();
            _activePanels = new Stack<AdminPanelGUILayout>();

            // Move and set external console
            _console = new AdminPanelConsole();
            AdminPanelGUI.AdminPanelConsole = _console;
            _console.OnContentChanged += () => {
                if(_consoleText != null)
                {
                    _consoleText.text = _console.Content;
                }
            };
        }

        void Update()
        {
            // FIXME TEST
            if(Input.GetMouseButtonDown(0) || (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began))
            {
                Open();
            }
        }

        void InflateGUI()
        {
            AdminPanelLayout rootLayout = new AdminPanelRootLayout();
            _canvasObject = rootLayout.Parent.gameObject;

            using(AdminPanelLayout horizontalLayout = rootLayout.CreateHorizontalLayout())
            {
                var panelLayout = horizontalLayout.CreatePanelLayout();//new Vector2(0.25f, 1.0f), ()=>{Close();}); //0.25

                panelLayout.CreateLabel("Admin Panel");

                using(var scrollLayout = panelLayout.CreateVerticalScrollLayout())
                {
                    AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                    rootPanel.OnCreateGUI(scrollLayout);
                }

                panelLayout.CreateToggleButton("Console", _consoleEnabled, (value) => {
                    _consoleEnabled = value;
                    RefreshPanel();
                });

                using(var rightVerticalLayout = horizontalLayout.CreateVerticalLayout(4)) //new VerticalLayout(new AdminPanelLayout(categoriesPanel)))
                {
                    // Content panel
                    if(_activePanels.Count > 0)
                    {

                        using(var panel = rightVerticalLayout.CreatePanelLayout(3))
                        {
                            using(var scrollLayout = panel.CreateVerticalScrollLayout())
                            {
                                _activePanels.Peek().OnCreateGUI(scrollLayout);
                            }
                        }
                    }

                    // Console panel
                    if(_consoleEnabled)
                    {
                        // Console panel
                        using(var panel = rightVerticalLayout.CreatePanelLayout())
                        {
                            using(var scrollLayout = panel.CreateVerticalScrollLayout())
                            {
                                scrollLayout.CreateLabel("Console");
                            }
                        }
                    }

                }
                /*
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
                            RectTransform contentPanel = AdminPanelGUIUtils.CreatePanel(rightVerticalLayout, new Vector2(1.0f, 1.0f));// - mainPanelSize));
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
                                    rectTransform.sizeDelta = new Vector2(contentVerticalLayout.Parent.rect.width * 0.95f , contentVerticalLayout.Parent.rect.height );
                                    
                                    Vector2 margin = new Vector2(contentVerticalLayout.Parent.rect.width * 0.05f, -5.0f); // FIXME MARGINS
                                    rectTransform.anchoredPosition = contentVerticalLayout.Position + margin;

                                    var text = textAreaObject.AddComponent<Text>();
                                    text.text = _console.Content;
                                    text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
                                    text.fontSize = 10;
                                    text.color = Color.white;
                                    text.alignment = TextAnchor.UpperLeft;

                                    ContentSizeFitter fitter = textAreaObject.AddComponent<ContentSizeFitter>();
                                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;;

                                    // Set as current text field
                                    _consoleText = text;

                                }  

                                using(var promptLayout = new HorizontalLayout(consoleLayout))
                                {
                                    AdminPanelGUIUtils.CreateButton(promptLayout, "Input", () => {});
                                }
                            }
                        }
                    }
                }*/
            }
        }

        private void Open()
        {
            if(_canvasObject == null)
            {
                // Load Layout data through handler
                _categories = new Dictionary<string, AdminPanelGUILayout>();
                AdminPanelHandler.InitializeHandler(new AdminPanelHandler(_categories));
                InflateGUI();
            }
        }

        private void Close()
        {
            Destroy(_canvasObject);
            _canvasObject = null;
            _consoleText = null;
        }

        public void ReplacePanel(AdminPanelGUILayout panelLayout)
        {
            _activePanels.Clear();
            OpenPanel(panelLayout);
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
                    _view.ReplacePanel(panelLayout);
                });
            }
        }
    }
}
