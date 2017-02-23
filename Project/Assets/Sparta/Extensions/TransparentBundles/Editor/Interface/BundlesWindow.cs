using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.TransparentBundles
{
    public class BundlesWindow : EditorWindow
    {
        public static BundlesWindow Window;
        private static EditorClientController _controller;
        private static string _filter;
        private static List<Bundle> _bundleList;
        private static Dictionary<string, Bundle> _chosenList;
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
        private static float iconsProcessCurrentSize = 1f;
        public static BundlePlaform CurrentPlatform;
        private const int _updateBundleDataDelay = 10;
        private static float _lastUpdateTime = 0f;
        private static Scene _previousScene;

        public static GUIStyle HeaderStyle, HeaderStyle2,
            BodyStyle, BodyTextStyle, BodyTextStyleProcessing, BodyTextStyleWarning, BodyTextStyleError,
            BodyTextBoldStyle, BodyLinkStyle, BodySpecialLinkStyle,
            BodySelectedLinkStyle, BodySelectedLinkStyleWarning, BodySelectedLinkStyleError,
            NoButtonStyle;
        private static float[] _columnsSize;

        private static void Init()
        {
            _controller = EditorClientController.GetInstance();
            _filter = "";
            _sorting = BundleSortingMode.NameAsc;
            SearchBundles(_filter);
            _chosenList = new Dictionary<string, Bundle>();
            _selectedList = new Dictionary<string, Bundle>();
            _selectAllToggle = false;
            _allSelected = false;

            ChangeSorting(_sorting);
            _scrollPos = Vector2.zero;

            _actionButons = new GUIContent[] {
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "update.png"), "Update Bundle"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "remove.png"), "Remove bundle"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "in_build.png"), "Add bundle into the Build"),
                new GUIContent(_controller.DownloadImage(Config.IconsPath + "out_build.png"), "Remove bundle from the Build")
            };

            _updateFilterTime = 0f;
            _toSearch = false;

            _columnsSize = new float[] { 20f, 20f, 50f, 100f };
            _controller.FlushCache();

            _previousScene = SceneManager.GetActiveScene();

            UpdateBundleData();

            //TODO: Get ios/android bundle platform from build platform
            CurrentPlatform = BundlePlaform.android_etc;
        }



        [MenuItem("Social Point/Bundles")]
        public static void OpenWindow()
        {
            Window = (BundlesWindow)EditorWindow.GetWindow(typeof(BundlesWindow));
            Window.titleContent.text = "Bundles";
            Init();
        }

        public static void ResetStyles()
        {
            HeaderStyle = null;
            HeaderStyle2 = null;
            BodyStyle = null;
            BodyTextStyle = null;
            BodyTextStyleProcessing = null;
            BodyTextStyleWarning = null;
            BodyTextStyleError = null;
            BodyTextBoldStyle = null;
            BodyLinkStyle = null;
            BodySpecialLinkStyle = null;
            BodySelectedLinkStyle = null;
            BodySelectedLinkStyleWarning = null;
            BodySelectedLinkStyleError = null;
            NoButtonStyle = null;
        }

        public static void InitStyles()
        {
            if(HeaderStyle == null)
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

            if(HeaderStyle2 == null)
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

            if(BodyStyle == null)
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

            if(BodyTextStyle == null)
            {
                BodyTextStyle = new GUIStyle(GUI.skin.label);
                BodyTextStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyTextStyle.alignment = TextAnchor.MiddleLeft;
                BodyTextStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyTextStyle.border = BodyTextStyle.margin;
                BodyTextStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            }

            if(BodyTextStyleProcessing == null)
            {
                BodyTextStyleProcessing = new GUIStyle(BodyTextStyle);
                BodyTextStyleProcessing.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            }

            if(BodyTextStyleWarning == null)
            {
                BodyTextStyleWarning = new GUIStyle(BodyTextStyle);
                BodyTextStyleWarning.normal.textColor = new Color(0.82f, 0.72f, 0f, 1f);
            }

            if(BodyTextStyleError == null)
            {
                BodyTextStyleError = new GUIStyle(BodyTextStyle);
                BodyTextStyleError.normal.textColor = new Color(0.55f, 0.05f, 0.05f, 1f);
            }

            if(BodyTextBoldStyle == null)
            {
                BodyTextBoldStyle = new GUIStyle(GUI.skin.label);
                BodyTextBoldStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                BodyTextBoldStyle.alignment = TextAnchor.MiddleLeft;
                BodyTextBoldStyle.margin = new RectOffset(0, 0, 0, 0);
                BodyTextBoldStyle.border = BodyTextStyle.margin;
                BodyTextBoldStyle.hover.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                BodyTextBoldStyle.fontStyle = FontStyle.Bold;
            }

            if(BodyLinkStyle == null)
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

            if(BodySpecialLinkStyle == null)
            {
                BodySpecialLinkStyle = new GUIStyle(GUI.skin.label);
                BodySpecialLinkStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
                BodySpecialLinkStyle.normal.background = Texture2D.blackTexture;
                BodySpecialLinkStyle.alignment = TextAnchor.MiddleLeft;
                BodySpecialLinkStyle.margin = new RectOffset(0, 0, 0, 0);
                BodySpecialLinkStyle.border = BodySpecialLinkStyle.margin;
                BodySpecialLinkStyle.fontStyle = FontStyle.Bold;
            }

            if(BodySelectedLinkStyle == null)
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

            if(BodySelectedLinkStyleWarning == null)
            {
                BodySelectedLinkStyleWarning = new GUIStyle(BodySelectedLinkStyle);
                BodySelectedLinkStyleWarning.normal.textColor = new Color(0.82f, 0.72f, 0f, 1f);
            }

            if(BodySelectedLinkStyleError == null)
            {
                BodySelectedLinkStyleError = new GUIStyle(BodySelectedLinkStyle);
                BodySelectedLinkStyleError.normal.textColor = new Color(0.55f, 0.05f, 0.05f, 1f);
            }

            if(NoButtonStyle == null)
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
            if(Window == null)
            {
                OpenWindow();
            }
            else if(_controller == null)
            {
                Init();
            }

            InitStyles();

            EditorGUILayout.BeginVertical();

            GUILayout.Label("", GUILayout.Height(10));
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("", GUILayout.Width(7));

            GUIContent buttonContent = null;
            if(_controller.ServerInfo.Status == ServerStatus.Warning)
            {
                Rect serverIconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(serverIconRect, _controller.DownloadImage(Config.IconsPath + "server_db.png"));
                GUILayout.Label(" Server Warning", BodyTextStyleWarning, GUILayout.ExpandWidth(false));
                buttonContent = new GUIContent(_controller.DownloadImage(Config.IconsPath + "warning.png"), "Warning");
            }
            else if(_controller.ServerInfo.Status == ServerStatus.Error)
            {
                Rect serverIconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(serverIconRect, _controller.DownloadImage(Config.IconsPath + "server_db.png"));
                GUILayout.Label(" Server Error", BodyTextStyleError, GUILayout.ExpandWidth(false));
                buttonContent = new GUIContent(_controller.DownloadImage(Config.IconsPath + "error.png"), "Error");
            }
            if(buttonContent != null && GUILayout.Button(buttonContent, NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                EditorUtility.DisplayDialog("Transparent Bundles " + _controller.ServerInfo.Status.ToString(), _controller.ServerInfo.Status.ToString() + "\n\n" + _controller.ServerInfo.Log, "Close");
            }
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            string previousFilter = _filter;
            _filter = EditorGUILayout.TextField(_filter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(200), GUILayout.Height(20));
            if(previousFilter != _filter)
            {
                _updateFilterTime = Time.realtimeSinceStartup;
                _toSearch = true;
            }
            if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
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
            _selectAllToggle = GUILayout.Toggle(_selectAllToggle, "", GUILayout.Width(_columnsSize[0] - 5f), GUILayout.Height(_columnsSize[0] - 5f));
            if(_selectAllToggle && !_allSelected)
            {
                for(int i = 0; i < _bundleList.Count; i++)
                {
                    Bundle bundle = _bundleList[i];
                    if(!_chosenList.ContainsKey(bundle.Name))
                    {
                        _chosenList.Add(bundle.Name, bundle);
                    }
                }
                _allSelected = true;
            }
            else if(!_selectAllToggle && _allSelected)
            {
                for(int i = 0; i < _bundleList.Count; i++)
                {
                    _chosenList.Remove(_bundleList[i].Name);
                }
                _allSelected = false;
            }
            string sortingSymbol = "";
            if(_sorting == BundleSortingMode.TypeAsc)
            {
                sortingSymbol = "▾";
            }
            else if(_sorting == BundleSortingMode.TypeDesc)
            {
                sortingSymbol = "▴";
            }
            if(GUILayout.Button("type" + sortingSymbol, NoButtonStyle, GUILayout.Width(40)))
            {
                if(_sorting == BundleSortingMode.TypeAsc)
                {
                    ChangeSorting(BundleSortingMode.TypeDesc);
                }
                else
                {
                    ChangeSorting(BundleSortingMode.TypeAsc);
                }
            }

            sortingSymbol = "";
            if(_sorting == BundleSortingMode.NameAsc)
            {
                sortingSymbol = "▾";
            }
            else if(_sorting == BundleSortingMode.NameDesc)
            {
                sortingSymbol = "▴";
            }
            if(GUILayout.Button("name" + sortingSymbol, NoButtonStyle, GUILayout.ExpandWidth(true)))
            {
                if(_sorting == BundleSortingMode.NameAsc)
                {
                    ChangeSorting(BundleSortingMode.NameDesc);
                }
                else
                {
                    ChangeSorting(BundleSortingMode.NameAsc);
                }
            }

            sortingSymbol = "";
            if(_sorting == BundleSortingMode.SizeAsc)
            {
                sortingSymbol = "▾";
            }
            else if(_sorting == BundleSortingMode.SizeDesc)
            {
                sortingSymbol = "▴";
            }
            if(GUILayout.Button("size" + sortingSymbol, NoButtonStyle, GUILayout.Width(_columnsSize[2])))
            {
                if(_sorting == BundleSortingMode.SizeAsc)
                {
                    ChangeSorting(BundleSortingMode.SizeDesc);
                }
                else
                {
                    ChangeSorting(BundleSortingMode.SizeAsc);
                }
            }
            GUILayout.Label("", GUILayout.Width(_columnsSize[3]));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, BodyStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginVertical(HeaderStyle2, GUILayout.Height(25));
            EditorGUILayout.BeginHorizontal();
            string collapsed = "▼";
            if(!_bundlesInServerShown)
            {
                collapsed = "►";
            }
            if(GUILayout.Button(collapsed, HeaderStyle2, GUILayout.ExpandWidth(false)))
            {
                _bundlesInServerShown = !_bundlesInServerShown;
            }
            Rect iconRect = GUILayoutUtility.GetRect(17, 17, GUILayout.ExpandWidth(false));
            Texture2D inServerIcon = _controller.DownloadImage(Config.IconsPath + "in_server.png");
            if(inServerIcon != null)
            {
                GUI.DrawTexture(iconRect, inServerIcon);
            }

            if(GUILayout.Button(" Bundles in Server", HeaderStyle2, GUILayout.ExpandWidth(false)))
            {
                _bundlesInServerShown = !_bundlesInServerShown;
            }
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.Label("total size: " + PrintProperSize(_controller.GetServerBundlesTotalSize(CurrentPlatform)), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if(_bundlesInServerShown)
            {
                int firstIndex = Mathf.Max((int)(_scrollPos.y / _bundleRowHeight) - 5, 0);
                firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, (_bundleList.Count - _bundlesInBuild) - _visibleRows));
                float firstSpace = firstIndex;
                GUILayout.Space(firstSpace * _bundleRowHeight);

                for(int i = firstIndex; i < Mathf.Min(_bundleList.Count, firstIndex + _visibleRows + _bundlesInBuild); i++)
                {
                    Bundle bundle = _bundleList[i];
                    if(!bundle.IsLocal)
                    {
                        DisplayBundleRow(bundle);
                    }
                }
                GUILayout.Label("", GUILayout.Height(30));

                float lastSpace = (_bundleList.Count - _bundlesInBuild) - firstIndex - _visibleRows;
                GUILayout.Space(Mathf.Max(0, lastSpace * _bundleRowHeight));
            }

            EditorGUILayout.BeginVertical(HeaderStyle2, GUILayout.Height(25));
            EditorGUILayout.BeginHorizontal();
            collapsed = "▼";
            if(!_bundlesInBuildShown)
            {
                collapsed = "►";
            }
            if(GUILayout.Button(collapsed, HeaderStyle2, GUILayout.ExpandWidth(false)))
            {
                _bundlesInBuildShown = !_bundlesInBuildShown;
            }
            iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            Texture2D inBuildIcon = _controller.DownloadImage(Config.IconsPath + "in_build.png");
            if(inBuildIcon != null)
            {
                GUI.DrawTexture(iconRect, _controller.DownloadImage(Config.IconsPath + "in_build.png"));
            }
            if(GUILayout.Button(" Bundles in Build", HeaderStyle2, GUILayout.ExpandWidth(false)))
            {
                _bundlesInBuildShown = !_bundlesInBuildShown;
            }
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.Label("total size: " + PrintProperSize(_controller.GetLocalBundlesTotalSize(CurrentPlatform)), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if(_bundlesInBuildShown)
            {
                for(int i = 0; i < _bundleList.Count; i++)
                {
                    Bundle bundle = _bundleList[i];
                    if(bundle.IsLocal)
                    {
                        DisplayBundleRow(bundle);
                    }
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
            GUILayout.Label(_chosenList.Count + " Assets selected", BodyTextBoldStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(5));
            int totalSelectedSize = 0;
            var chosenEnum = _chosenList.GetEnumerator();
            while(chosenEnum.MoveNext())
            {
                totalSelectedSize += chosenEnum.Current.Value.Size[CurrentPlatform];
            }
            GUILayout.Label(PrintProperSize(totalSelectedSize), BodyTextStyle, GUILayout.Width(70));
            EditorGUILayout.EndVertical();

            if(GUILayout.Button(_actionButons[0], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                chosenEnum = _chosenList.GetEnumerator();
                while(chosenEnum.MoveNext())
                {
                    _controller.CreateOrUpdateBundle(chosenEnum.Current.Value.Asset);
                }
                SearchBundles(_filter);
            }
            if(GUILayout.Button(_actionButons[1], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                string bundlesListString = "\n\n";

                int removeListLimit = 10;
                chosenEnum = _chosenList.GetEnumerator();
                for(int i = 0; chosenEnum.MoveNext() && i < removeListLimit; i++)
                {
                    bundlesListString += chosenEnum.Current.Value.Asset.Name + "\n";
                }

                if(_chosenList.Count > removeListLimit)
                {
                    bundlesListString += "... (" + (_chosenList.Count - removeListLimit).ToString() + " more)\n";
                }
                if(EditorUtility.DisplayDialog("Removing Bundle",
                        "You are about to remove " + _chosenList.Count + " bundles from the server." + bundlesListString
                        + "\nKeep in mind that this operation cannot be undone. Are you sure?",
                        "Remove", "Cancel"))
                {
                    chosenEnum = _chosenList.GetEnumerator();
                    while(chosenEnum.MoveNext())
                    {
                        _controller.RemoveBundle(chosenEnum.Current.Value.Asset);
                    }
                }
                SearchBundles(_filter);
            }
            if(GUILayout.Button(_actionButons[2], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                chosenEnum = _chosenList.GetEnumerator();
                while(chosenEnum.MoveNext())
                {
                    _controller.BundleIntoBuild(chosenEnum.Current.Value.Asset);
                }
                SearchBundles(_filter);
            }
            if(GUILayout.Button(_actionButons[3], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)))
            {
                chosenEnum = _chosenList.GetEnumerator();
                while(chosenEnum.MoveNext())
                {
                    _controller.BundleOutsideBuild(chosenEnum.Current.Value.Asset);
                }
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
            if(GUILayout.Button("Contact", GUILayout.Width(100)))
            {
                Application.OpenURL(Config.ContactUrl);
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(20), GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Height(2));
            if(GUILayout.Button(_controller.DownloadImage(Config.IconsPath + "help.png"), NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                Application.OpenURL(Config.HelpUrl);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Label("", GUILayout.Width(2));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            ManageKeyInputs();

            ManageAutoSearch();
        }




        private void DisplayBundleRow(Bundle bundle)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(_bundleRowHeight - 10));
            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[0]));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(_columnsSize[0]));
            bool bundleChosen = _chosenList.ContainsKey(bundle.Name);
            bool updatedBundleChosen = GUILayout.Toggle(bundleChosen, "", GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));

            if(bundleChosen && !updatedBundleChosen)
            {
                if(_selectedList.ContainsKey(bundle.Name))
                {
                    var enumerator = _selectedList.GetEnumerator();
                    while(enumerator.MoveNext())
                    {
                        _chosenList.Remove(enumerator.Current.Key);
                    }
                }
                else
                {
                    _chosenList.Remove(bundle.Name);
                }
            }
            else if(!bundleChosen && updatedBundleChosen)
            {
                if(_selectedList.ContainsKey(bundle.Name))
                {
                    var enumerator = _selectedList.GetEnumerator();
                    while(enumerator.MoveNext())
                    {
                        if(!_chosenList.ContainsKey(enumerator.Current.Key))
                        {
                            _chosenList.Add(enumerator.Current.Key, enumerator.Current.Value);
                        }
                    }
                }
                else
                {
                    if(!_chosenList.ContainsKey(bundle.Name))
                    {
                        _chosenList.Add(bundle.Name, bundle);
                    }
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[1]));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(_columnsSize[1]));
            GUILayout.Label(AssetPreview.GetMiniThumbnail(bundle.Asset.GetAssetObject()), GUILayout.Width(_columnsSize[1]), GUILayout.Height(_columnsSize[1]));
            EditorGUILayout.EndVertical();


            if(bundle.Status == BundleStatus.Warning || bundle.Status == BundleStatus.Error)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Label("", GUILayout.Height(3));
                GUIContent errorIcon = null;
                if(bundle.Status == BundleStatus.Warning)
                {
                    errorIcon = new GUIContent(_controller.DownloadImage(Config.IconsPath + "warning.png"), "Warning");
                }
                else
                {
                    errorIcon = new GUIContent(_controller.DownloadImage(Config.IconsPath + "error.png"), "Error");
                }
                if(GUILayout.Button(errorIcon, NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    EditorUtility.DisplayDialog("Transparent Bundles " + bundle.Status.ToString(), bundle.Status.ToString() + "!\n\n " + bundle.Log, "Close");
                }
                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUIStyle bundleStyle = BodyTextStyle;
            if(_selectedList.ContainsKey(bundle.Name))
            {
                if(bundle.Status == BundleStatus.Warning)
                {
                    bundleStyle = BodySelectedLinkStyleWarning;
                }
                else if(bundle.Status == BundleStatus.Error)
                {
                    bundleStyle = BodySelectedLinkStyleError;
                }
                else
                {
                    bundleStyle = BodySelectedLinkStyle;
                }
            }
            else
            {
                if(bundle.Status == BundleStatus.Processing)
                {
                    bundleStyle = BodyTextStyleProcessing;
                }
                else if(bundle.Status == BundleStatus.Warning)
                {
                    bundleStyle = BodyTextStyleWarning;
                }
                else if(bundle.Status == BundleStatus.Error)
                {
                    bundleStyle = BodyTextStyleError;
                }

            }
            if(GUILayout.Button(bundle.Asset.Name, bundleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(_bundleRowHeight)))
            {
                if(Event.current.control || Event.current.command)
                {
                    if(_selectedList.ContainsKey(bundle.Name))
                    {
                        _selectedList.Remove(bundle.Name);
                    }
                    else
                    {
                        _selectedList.Add(bundle.Name, bundle);
                    }
                }
                else if(Event.current.shift)
                {
                    if(_selectedList.Count > 0)
                    {
                        var enumerator = _selectedList.GetEnumerator();
                        enumerator.MoveNext();
                        Bundle firstSelectedBundle = enumerator.Current.Value;
                        int firstIndex = IndexOfBundle(_bundleList, firstSelectedBundle);
                        enumerator.Dispose();
                        int finalIndex = IndexOfBundle(_bundleList, bundle);
                        _selectedList = new Dictionary<string, Bundle>();

                        if(firstIndex < finalIndex)
                        {
                            for(int i = firstIndex; i <= finalIndex; i++)
                            {
                                Bundle bundleToSelect = _bundleList[i];
                                if(bundleToSelect.IsLocal == firstSelectedBundle.IsLocal)
                                {
                                    if(!_selectedList.ContainsKey(bundleToSelect.Name))
                                    {
                                        _selectedList.Add(bundleToSelect.Name, bundleToSelect);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for(int i = firstIndex; i >= finalIndex; i--)
                            {
                                Bundle bundleToSelect = _bundleList[i];
                                if(bundleToSelect.IsLocal == firstSelectedBundle.IsLocal)
                                {
                                    if(!_selectedList.ContainsKey(bundleToSelect.Name))
                                    {
                                        _selectedList.Add(bundleToSelect.Name, bundleToSelect);
                                    }
                                }
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

            if(bundle.Status == BundleStatus.Processing || bundle.Status == BundleStatus.Queued)
            {
                var operationEnumerator = bundle.OperationQueue.GetEnumerator();
                for(int i = 0; operationEnumerator.MoveNext(); i++)
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(25));
                    DrawOperationIcon(operationEnumerator.Current.Value, (bundle.Status == BundleStatus.Processing && i == 0) /*TEMPORARY*/ || _controller.NewBundles.ContainsKey(bundle.Name));
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[2]));
            GUILayout.Label("", GUILayout.Height(2));
            GUILayout.Label(PrintProperSize(bundle.Size[CurrentPlatform]), BodyTextStyle, GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            if(GUILayout.Button("↧ Download", GUILayout.Width(_columnsSize[3]), GUILayout.Height(22)))
            {
                _controller.DownloadBundle(bundle, CurrentPlatform);
            }

            EditorGUILayout.EndHorizontal();
        }

        private int IndexOfBundle(List<Bundle> bundleList, Bundle bundle)
        {
            int index = -1;

            for(int i = 0; i < bundleList.Count && index < 0; i++)
            {
                if(bundleList[i].Name == bundle.Name)
                {
                    index = i;
                }
            }

            return index;
        }

        private Texture2D DrawOperationIcon(BundleOperation operation, bool processing)
        {
            Texture2D icon = null;

            switch(operation)
            {
                case BundleOperation.create_asset_bundles:
                    if(processing)
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "update.png");
                    }
                    else
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "update_queued.png");
                    }
                    break;

                case BundleOperation.Remove:
                    if(processing)
                        icon = _controller.DownloadImage(Config.IconsPath + "remove.png");
                    else
                        icon = _controller.DownloadImage(Config.IconsPath + "remove_queued.png");
                    break;

                case BundleOperation.AddToBuild:
                    if(processing)
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "in_build.png");
                    }
                    else
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "in_build_queued.png");
                    }
                    break;

                case BundleOperation.RemoveFromBuild:
                    if(processing)
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "out_build.png");
                    }
                    else
                    {
                        icon = _controller.DownloadImage(Config.IconsPath + "out_build_queued.png");
                    }
                    break;

            }

            GUIContent iconContent = new GUIContent(icon, operation.ToString());

            float iconSize = 23;
            if(processing)
            {
                iconSize = iconSize * iconsProcessCurrentSize;
            }
            GUILayout.Label(iconContent, GUILayout.Height(iconSize), GUILayout.Width(iconSize));

            return icon;
        }

        private static void UpdateBundleData()
        {
            _lastUpdateTime = Time.realtimeSinceStartup;
            _controller.LoadBundleDataFromServer(() => SearchBundles(_filter));
        }

        private static void SearchBundles(string filter)
        {
            _bundleList = _controller.GetBundles(filter);
            ChangeSorting(_sorting);
            for(int i = 0; i < _bundleList.Count; i++)
            {
                Bundle bundle = _bundleList[i];
                if(bundle.IsLocal)
                {
                    _bundlesInBuild++;
                }
            }
            Window.Repaint();
        }

        private static void ChangeSorting(BundleSortingMode mode)
        {
            _sorting = mode;
            _controller.SortBundles(_sorting, _bundleList, CurrentPlatform);
        }

        private void ManageKeyInputs()
        {
            if(Event.current.isKey)
            {
                switch(Event.current.keyCode)
                {
                    case KeyCode.Return:
                        SearchBundles(_filter);
                        break;

                    case KeyCode.A:
                        if(Event.current.command || Event.current.control)
                        {
                            _selectedList = new Dictionary<string, Bundle>();
                            for(int i = 0; i < _bundleList.Count; i++)
                            {
                                _selectedList.Add(_bundleList[i].Name, _bundleList[i]);
                            }
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

        public static string PrintProperSize(int bytes)
        {
            string units = "MB";
            double converted = BytesToMegabytes(bytes);
            if(converted < 1)
            {
                units = "KB";
                converted = BytesToKilobytes(bytes);
            }

            converted = Math.Round(converted, 2);

            return converted.ToString() + " " + units;
        }

        public static double BytesToMegabytes(int bytes)
        {
            return BytesToKilobytes(bytes) / 1024d;
        }

        public static double BytesToKilobytes(int bytes)
        {
            return bytes / 1024d;
        }

        void Update()
        {
            if(SceneManager.GetActiveScene() != _previousScene)
            {
                if(_controller != null)
                {
                    _controller.FlushImagesCache();
                    Window.Close();
                    OpenWindow();
                    ResetStyles();
                    _previousScene = SceneManager.GetActiveScene();
                }
                return;
            }

            iconsProcessCurrentSize = 0.9f + (Mathf.Max(Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.1f, 0f));
            if((_lastUpdateTime + _updateBundleDataDelay) < Time.realtimeSinceStartup)
            {
                UpdateBundleData();
            }
            Repaint();
        }
    }
}
