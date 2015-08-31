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
                            using(var scrollLayout = panel.CreateVerticalScrollLayout(out _consoleScroll))
                            {
                                scrollLayout.CreateLabel("Console");
                                scrollLayout.CreateTextArea("Texto", out _consoleText);
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
