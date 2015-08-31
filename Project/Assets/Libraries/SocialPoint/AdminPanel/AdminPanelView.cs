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
        private AdminPanelConsole _console;
        private ScrollRect _consoleScroll;
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
                    _consoleScroll.verticalNormalizedPosition = 0.0f;
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
                var panelLayout = horizontalLayout.CreatePanelLayout("Admin Panel", ()=>{ Close(); });

                using(var scrollLayout = panelLayout.CreateVerticalScrollLayout())
                {
                    AdminPanelGUI rootPanel = new AdminPanelCategoriesGUI(this, _categories);
                    rootPanel.OnCreateGUI(scrollLayout);
                }

                panelLayout.CreateToggleButton("Console", _consoleEnabled, (value) => {
                    _consoleEnabled = value;
                    RefreshPanel();
                });

                using(var rightVerticalLayout = horizontalLayout.CreateVerticalLayout(4))
                {
                    // Content panel
                    if(_activePanels.Count > 0)
                    {

                        using(var panel = rightVerticalLayout.CreatePanelLayout("Panel", () => { ClosePanel(); }, 2))
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
                                _consoleScroll = scrollLayout.Parent.gameObject.transform.parent.gameObject.GetComponent<ScrollRect>(); // FIXME
                                scrollLayout.CreateLabel("Console");

                                // Console
                                GameObject textAreaObject = new GameObject("AdminPanel - Text Area");
                                
                                var rectTransform = textAreaObject.AddComponent<RectTransform>();
                                textAreaObject.transform.SetParent(scrollLayout.Parent);
                                
                                rectTransform.offsetMin = Vector2.zero;
                                rectTransform.offsetMax = Vector2.zero;
                                rectTransform.pivot = Vector2.up;
                                
                                rectTransform.anchorMin = Vector2.up;
                                rectTransform.anchorMax = Vector2.up;
                                rectTransform.sizeDelta = new Vector2(scrollLayout.Parent.rect.width * 0.95f , scrollLayout.Parent.rect.height );
                                
                                Vector2 margin = new Vector2(scrollLayout.Parent.rect.width * 0.05f, -5.0f); // FIXME MARGINS
                                rectTransform.anchoredPosition = scrollLayout.Position + margin;
                                
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
                        }
                    }
                }
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
                layout.CreateButton(categoryLabel, () => {
                    _view.ReplacePanel(panelLayout);
                });
            }
        }
    }
}
