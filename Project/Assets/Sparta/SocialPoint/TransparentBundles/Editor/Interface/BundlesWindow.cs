using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class BundlesWindow: EditorWindow
    {
        public static BundlesWindow Window;
        private static EditorClientController _controller;
        private static string _filter;
        private static List<Bundle> _bundleList;
        private static List<Bundle> _chosenList;
        private static bool _selectAllToggle;
        private static bool _allSelected;
        private static Dictionary<string, Bundle> _selectedList;
        private static BundleSortingMode _sorting;
        private static Vector2 _scrollPos;
        private const int _iconSize = 33;
        private static GUIContent[] _actionButons;
        private static float _updateFilterTime;
        private static bool _toSearch = false;
        private const float _searchDelay = 0.5f;
        private static bool _bundlesInServerShown = true;
        private static bool _bundlesInBuildShown = true;
        private const int _bundleRowHeight = 30;
        private const int _visibleRows = 50;
        private static int _bundlesInBuild = 0;

        public static GUIStyle HeaderStyle, HeaderStyle2, BodyStyle, BodyTextStyle, BodyTextBoldStyle, BodyLinkStyle, BodySpecialLinkStyle, BodySelectedLinkStyle, NoButtonStyle;
        private static float[] _columnsSize;

        private static void Init()
        {
            _controller = EditorClientController.GetInstance();
            _filter = "";
            _sorting = BundleSortingMode.NameAsc;
            SearchBundles(_filter);
            _chosenList = new List<Bundle>();
            _selectedList = new Dictionary<string, Bundle>();
            _selectAllToggle = false;
            _allSelected = false;
            
            ChangeSorting(_sorting);
            _scrollPos = Vector2.zero;

            _actionButons = new GUIContent[] 
            {
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "update.png"), "Update Bundle"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "remove.png"), "Remove bundle"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "in_build.png"), "Add bundle into the Build"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "out_build.png"), "Remove bundle from the Build")
            };

            _updateFilterTime = 0f;
            _toSearch = false;

            _columnsSize = new float[] { 20f, 20f, 50f, 100f };
            _controller.FlushCache();
        }

        [MenuItem("Social Point/Bundles")]
        public static void OpenWindow()
        {
            Window = (BundlesWindow)EditorWindow.GetWindow(typeof(BundlesWindow));
            Window.titleContent.text = "Bundles";
            Init();
        }

        public static void InitStyles()
        {
            if (HeaderStyle == null)
            {
                HeaderStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.25f, 1f));
                tex.Apply();
                HeaderStyle.normal.background = tex;
                HeaderStyle.normal.textColor = Color.black;
                HeaderStyle.alignment = TextAnchor.MiddleLeft;
                HeaderStyle.margin = new RectOffset(1, 1, 1, 1);
                HeaderStyle.border = new RectOffset(0, 0, 0, 0);
            }

            if (HeaderStyle2 == null)
            {
                HeaderStyle2 = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.33f, 0.33f, 0.33f, 1f));
                tex.Apply();
                HeaderStyle2.normal.background = tex;
                HeaderStyle2.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                HeaderStyle2.alignment = TextAnchor.LowerLeft;
                HeaderStyle2.margin = new RectOffset(0, 0, 0, 5);
                HeaderStyle2.border = new RectOffset(0, 0, 0, 0);
            }

            if (BodyStyle == null)
            {
                BodyStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.45f, 0.45f, 0.45f, 1f));
                tex.Apply();
                BodyStyle.alignment = TextAnchor.MiddleLeft;
                BodyStyle.normal.background = tex;
                BodyStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyStyle.border = BodyStyle.margin;
            }

            if (BodyTextStyle == null)
            {
                BodyTextStyle = new GUIStyle(GUI.skin.label);
                BodyTextStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyTextStyle.alignment = TextAnchor.MiddleLeft;
                BodyTextStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyTextStyle.border = BodyTextStyle.margin;
                BodyTextStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            }
            
            if (BodyTextBoldStyle == null)
            {
                BodyTextBoldStyle = new GUIStyle(GUI.skin.label);
                BodyTextBoldStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyTextBoldStyle.alignment = TextAnchor.MiddleLeft;
                BodyTextBoldStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyTextBoldStyle.border = BodyTextStyle.margin;
                BodyTextBoldStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                BodyTextBoldStyle.fontStyle = FontStyle.Bold;
            }

            if (BodyLinkStyle == null)
            {
                BodyLinkStyle = new GUIStyle(GUI.skin.label);
                BodyLinkStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyLinkStyle.normal.background = Texture2D.blackTexture;
                BodyLinkStyle.alignment = TextAnchor.MiddleLeft;
                BodyLinkStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyLinkStyle.border = BodyLinkStyle.margin;
                BodyLinkStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                BodyLinkStyle.hover.background = Texture2D.blackTexture;
            }

            if (BodySpecialLinkStyle == null)
            {
                BodySpecialLinkStyle = new GUIStyle(GUI.skin.label);
                BodySpecialLinkStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
                BodySpecialLinkStyle.normal.background = Texture2D.blackTexture;
                BodySpecialLinkStyle.alignment = TextAnchor.MiddleLeft;
                BodySpecialLinkStyle.margin = new RectOffset(0, 0, 0, 0);
                BodySpecialLinkStyle.border = BodySpecialLinkStyle.margin;
                BodySpecialLinkStyle.fontStyle = FontStyle.Bold;
            }

            if (BodySelectedLinkStyle == null)
            {
                BodySelectedLinkStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.24f, 0.37f, 0.58f, 1f));
                tex.Apply();
                BodySelectedLinkStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                BodySelectedLinkStyle.normal.background = tex;
                BodySelectedLinkStyle.alignment = TextAnchor.MiddleLeft;
                BodySelectedLinkStyle.margin = new RectOffset(0, 0, 0, 0);
                BodySelectedLinkStyle.border = BodySelectedLinkStyle.margin;
            }

            if (NoButtonStyle == null)
            {
                NoButtonStyle = new GUIStyle(GUI.skin.label);
                NoButtonStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                NoButtonStyle.active.textColor = new Color(0.6f, 0.6f, 0.6f);
                NoButtonStyle.hover.textColor = new Color(1f, 1f, 1f);
                NoButtonStyle.richText = true;
                NoButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                NoButtonStyle.border = NoButtonStyle.margin;
            }
        }

        void OnGUI()
        {
            if (Window == null)
                OpenWindow();

            else if (_controller == null)
                Init();

            InitStyles();

            EditorGUILayout.BeginVertical();

            GUILayout.Label("", GUILayout.Height(10));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            string previousFilter = _filter;
            _filter = EditorGUILayout.TextField(_filter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(200), GUILayout.Height(20));
            if (previousFilter != _filter)
            {
                _updateFilterTime = Time.realtimeSinceStartup;
                _toSearch = true;
            }
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                EditorGUI.FocusTextInControl("");
                _filter = "";
                SearchBundles(_filter);
            }
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(3));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.BeginVertical(BodyStyle, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical(HeaderStyle, GUILayout.Height(25));
            EditorGUILayout.BeginHorizontal();
            _selectAllToggle = GUILayout.Toggle(_selectAllToggle,"", GUILayout.Width(_columnsSize[0]-5f), GUILayout.Height(_columnsSize[0] - 5f));
            if (_selectAllToggle && !_allSelected)
            {
                _chosenList = new List<Bundle>(_bundleList);
                _allSelected = true;
            }
            else if(!_selectAllToggle && _allSelected)
            {
                for(int i = 0; i < _bundleList.Count; i++)
                    _chosenList.Remove(_bundleList[i]);
                _allSelected = false;
            }
            string sortingSymbol = "";
            if (_sorting == BundleSortingMode.TypeAsc)
                sortingSymbol = "▾";
            else if (_sorting == BundleSortingMode.TypeDesc)
                sortingSymbol = "▴";
            if (GUILayout.Button("type"+ sortingSymbol, NoButtonStyle, GUILayout.Width(40)))
            {
                if (_sorting == BundleSortingMode.TypeAsc)
                    ChangeSorting(BundleSortingMode.TypeDesc);
                else
                    ChangeSorting(BundleSortingMode.TypeAsc);
            }

            sortingSymbol = "";
            if (_sorting == BundleSortingMode.NameAsc)
                sortingSymbol = "▾";
            else if (_sorting == BundleSortingMode.NameDesc)
                sortingSymbol = "▴";
            if (GUILayout.Button("name"+ sortingSymbol, NoButtonStyle, GUILayout.ExpandWidth(true)))
            {
                if (_sorting == BundleSortingMode.NameAsc)
                    ChangeSorting(BundleSortingMode.NameDesc);
                else
                    ChangeSorting(BundleSortingMode.NameAsc);
            }

            sortingSymbol = "";
            if (_sorting == BundleSortingMode.SizeAsc)
                sortingSymbol = "▾";
            else if (_sorting == BundleSortingMode.SizeDesc)
                sortingSymbol = "▴";
            if (GUILayout.Button("size"+ sortingSymbol, NoButtonStyle, GUILayout.Width(_columnsSize[2])))
            {
                if (_sorting == BundleSortingMode.SizeAsc)
                    ChangeSorting(BundleSortingMode.SizeDesc);
                else
                    ChangeSorting(BundleSortingMode.SizeAsc);
            }
            GUILayout.Label("", GUILayout.Width(_columnsSize[3]));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, BodyStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical(HeaderStyle2, GUILayout.Height(25));
            EditorGUILayout.BeginHorizontal();
            string collapsed = "▼";
            if (!_bundlesInServerShown)
                collapsed = "►";
            if (GUILayout.Button(collapsed, HeaderStyle2, GUILayout.ExpandWidth(false)))
                _bundlesInServerShown = !_bundlesInServerShown;
            Rect iconRect = GUILayoutUtility.GetRect(17, 17, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(iconRect, _controller.DownloadImage(Config.IconsPath + "in_server.png"));
            if (GUILayout.Button(" Bundles in Server", HeaderStyle2, GUILayout.ExpandWidth(false)))
                _bundlesInServerShown = !_bundlesInServerShown;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.Label("total size: " + _controller.GetServerBundlesTotalSize() + " MB", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (_bundlesInServerShown)
            {
                int firstIndex = Mathf.Max((int)(_scrollPos.y / _bundleRowHeight) - 5, 0);
                firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, (_bundleList.Count - _bundlesInBuild) - _visibleRows));
                float firstSpace = firstIndex;
                GUILayout.Space(firstSpace * _bundleRowHeight);
                
                for (int i = firstIndex; i < Mathf.Min(_bundleList.Count, firstIndex + _visibleRows + _bundlesInBuild); i++)
                {
                    Bundle bundle = _bundleList[i];
                    if (!bundle.IsLocal)
                        DisplayBundleRow(bundle);
                }
                GUILayout.Label("", GUILayout.Height(30));

                float lastSpace = (_bundleList.Count - _bundlesInBuild) - firstIndex - _visibleRows;
                GUILayout.Space(Mathf.Max(0, lastSpace * _bundleRowHeight));
            }

            EditorGUILayout.BeginVertical(HeaderStyle2, GUILayout.Height(25));
            EditorGUILayout.BeginHorizontal();
            collapsed = "▼";
            if (!_bundlesInBuildShown)
                collapsed = "►";
            if(GUILayout.Button(collapsed, HeaderStyle2, GUILayout.ExpandWidth(false)))
                _bundlesInBuildShown = !_bundlesInBuildShown;
            iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(iconRect, _controller.DownloadImage(Config.IconsPath + "in_build.png"));
            if (GUILayout.Button(" Bundles in Build", HeaderStyle2,  GUILayout.ExpandWidth(false)))
                _bundlesInBuildShown = !_bundlesInBuildShown;
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.Label("total size: " + _controller.GetLocalBundlesTotalSize() + " MB", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (_bundlesInBuildShown)
            {
                for (int i = 0; i < _bundleList.Count; i++)
                {
                    Bundle bundle = _bundleList[i];
                    if (bundle.IsLocal)
                        DisplayBundleRow(bundle);
                }
                GUILayout.Label("", GUILayout.Height(20));
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.BeginVertical(BodyStyle, GUILayout.ExpandWidth(true), GUILayout.Height(55));
            GUILayout.Label("", GUILayout.Height(5));
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("", GUILayout.Width(10));

            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(5));
            GUILayout.Label(_chosenList.Count+" Assets selected", BodyTextBoldStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(5));
            float totalSelectedSize = 0f;
            for (int i = 0; i < _chosenList.Count; i++)
                totalSelectedSize += _chosenList[i].Size;
            GUILayout.Label(totalSelectedSize + " MB", BodyTextStyle, GUILayout.Width(50));
            EditorGUILayout.EndVertical();

            if (GUILayout.Button(_actionButons[0], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                for (int i = 0; i < _chosenList.Count; i++)
                    _controller.CreateOrUpdateBundle(_chosenList[i].Asset);
                SearchBundles(_filter);
            }
            if (GUILayout.Button(_actionButons[1], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                string bundlesListString = "\n\n";

                int removeListLimit = 10;
                for (int i = 0; i < _chosenList.Count && i < removeListLimit; i++)
                    bundlesListString += _chosenList[i].Asset.Name+"\n";
                if(_chosenList.Count > removeListLimit)
                    bundlesListString +=  "... ("+ (_chosenList.Count - removeListLimit ).ToString()+ " more)\n";

                if (EditorUtility.DisplayDialog("Removing Bundle",
                "You are about to remove "+ _chosenList.Count + " bundles from the server."+ bundlesListString
                + "\nKeep in mind that this operation cannot be undone. Are you sure?",
                "Remove", "Cancel"))
                {
                    for (int i = 0; i < _chosenList.Count; i++)
                        _controller.RemoveBundle(_chosenList[i].Asset);
                }
                SearchBundles(_filter);
            }
            if (GUILayout.Button(_actionButons[2], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                for (int i = 0; i < _chosenList.Count; i++)
                    _controller.BundleIntoBuild(_chosenList[i].Asset);
                SearchBundles(_filter);
            }
            if (GUILayout.Button(_actionButons[3], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                for (int i = 0; i < _chosenList.Count; i++)
                    _controller.BundleOutsideBuild(_chosenList[i].Asset);
                SearchBundles(_filter);
            }
            GUILayout.Label("", GUILayout.Width(3));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(2));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(30));
            if (GUILayout.Button("Contact", GUILayout.Width(100)))
                Application.OpenURL(Config.ContactUrl);

            EditorGUILayout.BeginVertical(GUILayout.Width(20), GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Height(2));
            if (GUILayout.Button(_controller.DownloadImage(Config.IconsPath + "help.png"), NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                Application.OpenURL(Config.HelpUrl);
            EditorGUILayout.EndVertical();
            GUILayout.Label("", GUILayout.Width(2));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            ManageKeyInputs();

            ManageAutoSearch();
        }

        


        private void DisplayBundleRow(Bundle bundle)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(_bundleRowHeight-10));
            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[0]));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(_columnsSize[0]));
            bool bundleChosen = _chosenList.Contains(bundle);
            bool updatedBundleChosen = GUILayout.Toggle(bundleChosen, "", GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));

            if (bundleChosen && !updatedBundleChosen)
            {
                if (_selectedList.ContainsKey(bundle.Name))
                {
                    var enumerator = _selectedList.GetEnumerator();
                    while (enumerator.MoveNext())
                        _chosenList.Remove(enumerator.Current.Value);
                }
                else
                    _chosenList.Remove(bundle);
            }

            else if (!bundleChosen && updatedBundleChosen)
            {
                if (_selectedList.ContainsKey(bundle.Name))
                {
                    var enumerator = _selectedList.GetEnumerator();
                    while (enumerator.MoveNext())
                        _chosenList.Add(enumerator.Current.Value);
                }
                else
                    _chosenList.Add(bundle);
            }
                
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[1]));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(_columnsSize[1]));
            GUILayout.Label(AssetPreview.GetMiniThumbnail(bundle.Asset.GetAssetObject()), GUILayout.Width(_columnsSize[1]), GUILayout.Height(_columnsSize[1]));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUIStyle bundleStyle = BodyTextStyle;
            if (_selectedList.ContainsKey(bundle.Name))
                bundleStyle = BodySelectedLinkStyle;
            if (GUILayout.Button(bundle.Asset.Name, bundleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(_bundleRowHeight)))
            {
                if (Event.current.control || Event.current.command)
                {
                    if (_selectedList.ContainsKey(bundle.Name))
                        _selectedList.Remove(bundle.Name);
                    else
                        _selectedList.Add(bundle.Name, bundle);
                }
                    
                else if (Event.current.shift)
                {
                    if(_selectedList.Count > 0)
                    {
                        var enumerator = _selectedList.GetEnumerator();
                        enumerator.MoveNext();
                        Bundle firstSelectedBundle = enumerator.Current.Value;
                        int firstIndex = _bundleList.IndexOf(firstSelectedBundle);
                        enumerator.Dispose();
                        int finalIndex = _bundleList.IndexOf(bundle);
                        _selectedList = new Dictionary<string, Bundle>();
                         
                        if (firstIndex < finalIndex)
                        {
                            for (int i = firstIndex; i <= finalIndex; i++)
                            {
                                Bundle bundleToSelect = _bundleList[i];
                                if(bundleToSelect.IsLocal == firstSelectedBundle.IsLocal)
                                    _selectedList.Add(bundleToSelect.Name, bundleToSelect);
                            }
                        }
                        else
                        {
                            for (int i = firstIndex; i >= finalIndex; i--)
                            {
                                Bundle bundleToSelect = _bundleList[i];
                                if (bundleToSelect.IsLocal == firstSelectedBundle.IsLocal)
                                    _selectedList.Add(bundleToSelect.Name, bundleToSelect);
                            }
                        }
                    }
                }
                else
                {
                    _selectedList = new Dictionary<string, Bundle>();
                    _selectedList.Add(bundle.Name, bundle);
                }
                    
                InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                inspectorDummy.SelectedAsset = bundle.Asset;
                Selection.activeObject = inspectorDummy;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[2]));
            GUILayout.Label("", GUILayout.Height(2));
            GUILayout.Label(bundle.Size + " MB", BodyTextStyle, GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("↧ Download", GUILayout.Width(_columnsSize[3]), GUILayout.Height(22)))
                _controller.InstanciateBundle(bundle);

            EditorGUILayout.EndHorizontal();
        }

        private static void SearchBundles(string filter)
        {
            _bundleList = _controller.GetBundles(filter);
            ChangeSorting(_sorting);
            for (int i = 0; i < _bundleList.Count; i++)
            {
                Bundle bundle = _bundleList[i];
                if (bundle.IsLocal)
                    _bundlesInBuild++;
            }
            Window.Repaint();
        }

        private static void ChangeSorting(BundleSortingMode mode)
        {
            _sorting = mode;
            _controller.SortBundles(_sorting, _bundleList);
        }

        private void ManageKeyInputs()
        {
            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                        SearchBundles(_filter);
                        break;

                    case KeyCode.A:
                        if (Event.current.command || Event.current.control)
                        {
                            _selectedList = new Dictionary<string, Bundle>();
                            for (int i = 0; i < _bundleList.Count; i++)
                                _selectedList.Add(_bundleList[i].Name, _bundleList[i]);
                        }
                        Window.Repaint();
                        break;
                }
            }
        }

        private void ManageAutoSearch()
        {
            if(_toSearch && _updateFilterTime + _searchDelay > Time.realtimeSinceStartup)
            {
                _updateFilterTime = 0f;
                _toSearch = false;
                SearchBundles(_filter);
            }
        }

        /*void OnInspectorUpdate()
        {
            Repaint();
        }*/
    }
}
