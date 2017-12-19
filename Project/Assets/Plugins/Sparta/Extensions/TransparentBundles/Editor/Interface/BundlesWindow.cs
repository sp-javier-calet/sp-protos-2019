using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.TransparentBundles
{
    public class BundlesWindow : EditorWindow
    {
        public static BundlesWindow Window;
        static EditorClientController _controller;
        static string _filter;
        public static List<Bundle> BundleList;
        static Dictionary<string, Bundle> _chosenList;
        static bool _selectAllToggle;
        static bool _allSelected;
        static Dictionary<string, Bundle> _selectedList;
        static BundleSortingMode _sorting;
        static Vector2 _scrollPos;
        const int _iconSize = 33;
        static GUIContent[] _actionButons;
        const float _searchDelay = 0.5f;
        static bool _bundlesInServerShown = true;
        static bool _bundlesInBuildShown = true;
        const int _bundleRowHeight = 30;
        const int _visibleRows = 50;
        static int _bundlesInBuild;
        static float iconsProcessCurrentSize = 1f;
        public static BundlePlaform CurrentPlatform;
        const int _updateBundleDataDelay = 10;
        static float _lastUpdateTime;
        static Scene _previousScene;
        static bool _isPlaying;

        public static GUIStyle HeaderStyle, HeaderStyle2,
            BodyStyle, BodyTextStyle, BodyTextStyleProcessing, BodyTextStyleWarning, BodyTextStyleError,
            BodyTextBoldStyle, BodyLinkStyle, BodySpecialLinkStyle,
            BodySelectedLinkStyle, BodySelectedLinkStyleWarning, BodySelectedLinkStyleError,
            NoButtonStyle;
        static float[] _columnsSize;

        static void Init()
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

            _controller.CheckInBranchUpdated();

            if(_controller.BranchUpdated)
            {
                _actionButons = new[] {
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.UpdateImageName), "Update Bundle"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.RemoveImageName), "Remove bundle"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.InBuildImageName), "Add bundle into the Build"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.OutBuildImageName), "Remove bundle from the Build")
                };
            }
            else
            {
                _actionButons = new[] {
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.UpdateQueuedImageName), "Update Bundle"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.RemoveQueuedImageName), "Remove bundle"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.InBuildQueuedImageName), "Add bundle into the Build"),
                    new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.OutBuildQueuedImageName), "Remove bundle from the Build")
                };
            }

            _columnsSize = new[] { 20f, 20f, 70f, 100f };
            _controller.FlushCache();

            _previousScene = SceneManager.GetActiveScene();

            UpdateBundleData();

            CurrentPlatform = BundlePlaform.android_etc;


        }



        [MenuItem("Sparta/Bundles", false, 500)]
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
                var tex = new Texture2D(1, 1);
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
                var tex = new Texture2D(1, 1);
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
                var tex = new Texture2D(1, 1);
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
                var tex = new Texture2D(1, 1);
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
            Rect serverIconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(serverIconRect, _controller.DownloadImage(Config.IconsPath + Config.ServerDbImageName));

            if(_controller.ServerInfo.ProcessingQueue.Count > 0)
            {
                if(GUILayout.Button(new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.UpdateImageName), "Processing"), NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    ServerInfoWindow.OpenWindow();
                }
            }
            else
            {
                GUILayout.Label(_controller.DownloadImage(Config.IconsPath + Config.SleepImageName), GUILayout.Width(20), GUILayout.Height(20));
            }

            if(_controller.ServerInfo.Status == ServerStatus.Warning)
            {
                GUILayout.Label(" System Warning", BodyTextStyleWarning, GUILayout.ExpandWidth(false));
                buttonContent = new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.WarningImageName), "Warning");
            }
            else if(_controller.ServerInfo.Status == ServerStatus.Error)
            {
                GUILayout.Label(" System Error", BodyTextStyleError, GUILayout.ExpandWidth(false));
                buttonContent = new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.ErrorImageName), "Error");
            }

            if(buttonContent != null && GUILayout.Button(buttonContent, NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                EditorUtility.DisplayDialog("Transparent Bundles " + _controller.ServerInfo.Status, _controller.ServerInfo.Status + "\n\n" + _controller.ServerInfo.Log, "Close");
            }
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            _filter = EditorGUILayout.TextField(_filter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(200), GUILayout.Height(20));
            if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                EditorGUI.FocusTextInControl("");
                _filter = "";
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
                for(int i = 0; i < BundleList.Count; i++)
                {
                    Bundle bundle = BundleList[i];
                    if(!_chosenList.ContainsKey(bundle.Name))
                    {
                        _chosenList.Add(bundle.Name, bundle);
                    }
                }
                _allSelected = true;
            }
            else if(!_selectAllToggle && _allSelected)
            {
                for(int i = 0; i < BundleList.Count; i++)
                {
                    _chosenList.Remove(BundleList[i].Name);
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
            Texture2D inServerIcon = _controller.DownloadImage(Config.IconsPath + Config.InServerImageName);
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
                firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, (BundleList.Count - _bundlesInBuild) - _visibleRows));
                float firstSpace = firstIndex;
                GUILayout.Space(firstSpace * _bundleRowHeight);

                for(int i = firstIndex; i < Mathf.Min(BundleList.Count, firstIndex + _visibleRows + _bundlesInBuild); i++)
                {
                    Bundle bundle = BundleList[i];
                    if(!bundle.IsLocal)
                    {
                        DisplayBundleRow(bundle);
                    }
                }
                GUILayout.Label("", GUILayout.Height(30));

                float lastSpace = (BundleList.Count - _bundlesInBuild) - firstIndex - _visibleRows;
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
            Texture2D inBuildIcon = _controller.DownloadImage(Config.IconsPath + Config.InBuildImageName);
            if(inBuildIcon != null)
            {
                GUI.DrawTexture(iconRect, _controller.DownloadImage(Config.IconsPath + Config.InBuildImageName));
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
                for(int i = 0; i < BundleList.Count; i++)
                {
                    Bundle bundle = BundleList[i];
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
            chosenEnum.Dispose();
            GUILayout.Label(PrintProperSize(totalSelectedSize), BodyTextStyle, GUILayout.Width(70));
            EditorGUILayout.EndVertical();

            if(GUILayout.Button(_actionButons[0], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)) && _controller.BranchUpdated && _chosenList.Count > 0)
            {
                chosenEnum = _chosenList.GetEnumerator();
                var assetList = new List<Asset>();
                while(chosenEnum.MoveNext())
                {
                    assetList.Add(chosenEnum.Current.Value.Asset);
                }
                _controller.CreateOrUpdateBundles(assetList);
                chosenEnum.Dispose();
            }
            if(GUILayout.Button(_actionButons[1], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)) && _controller.BranchUpdated && _chosenList.Count > 0)
            {
                string bundlesListString = "\n\n";

                const int removeListLimit = 10;
                chosenEnum = _chosenList.GetEnumerator();
                for(int i = 0; chosenEnum.MoveNext() && i < removeListLimit; i++)
                {
                    bundlesListString += chosenEnum.Current.Value.Asset.Name + "\n";
                }
                chosenEnum.Dispose();

                if(_chosenList.Count > removeListLimit)
                {
                    bundlesListString += "... (" + (_chosenList.Count - removeListLimit) + " more)\n";
                }
                if(EditorUtility.DisplayDialog("Removing Bundle",
                        "You are about to remove " + _chosenList.Count + " bundles from the server." + bundlesListString
                        + "\nKeep in mind that this operation cannot be undone. Are you sure?",
                        "Remove", "Cancel"))
                {
                    chosenEnum = _chosenList.GetEnumerator();
                    var assetList = new List<Asset>();
                    while(chosenEnum.MoveNext())
                    {
                        assetList.Add(chosenEnum.Current.Value.Asset);
                    }
                    _controller.PerfomBundleOperation(assetList, BundleOperation.remove_asset_bundles);
                    chosenEnum.Dispose();
                }
            }
            if(GUILayout.Button(_actionButons[2], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)) && _controller.BranchUpdated && _chosenList.Count > 0)
            {
                chosenEnum = _chosenList.GetEnumerator();
                var assetList = new List<Asset>();
                while(chosenEnum.MoveNext())
                {
                    assetList.Add(chosenEnum.Current.Value.Asset);
                }
                _controller.PerfomBundleOperation(assetList, BundleOperation.create_local_asset_bundles);
                chosenEnum.Dispose();
            }
            if(GUILayout.Button(_actionButons[3], GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)) && _controller.BranchUpdated && _chosenList.Count > 0)
            {
                chosenEnum = _chosenList.GetEnumerator();
                var assetList = new List<Asset>();
                while(chosenEnum.MoveNext())
                {
                    assetList.Add(chosenEnum.Current.Value.Asset);
                }
                _controller.PerfomBundleOperation(assetList, BundleOperation.remove_local_asset_bundles);
                chosenEnum.Dispose();
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
            if(GUILayout.Button(_controller.DownloadImage(Config.IconsPath + Config.HelpImageName), NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                Application.OpenURL(Config.HelpUrl);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Label("", GUILayout.Width(2));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            ManageKeyInputs();
        }

        static void DisplayBundleRow(Bundle bundle)
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
                    enumerator.Dispose();
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
                    enumerator.Dispose();
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
            var assetObject = bundle.Asset.GetAssetObject();
            GUILayout.Label(assetObject == null ? _controller.DownloadImage(Config.IconsPath + Config.MissingFileImageName) : AssetPreview.GetMiniThumbnail(assetObject), GUILayout.Width(_columnsSize[1]), GUILayout.Height(_columnsSize[1]));
            EditorGUILayout.EndVertical();

            if(bundle.Status == BundleStatus.Warning || bundle.Status == BundleStatus.Failed)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Label("", GUILayout.Height(3));
                GUIContent errorIcon;
                errorIcon = bundle.Status == BundleStatus.Warning ? new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.WarningImageName), "Warning") : new GUIContent(_controller.DownloadImage(Config.IconsPath + Config.ErrorImageName), "Error");
                if(GUILayout.Button(errorIcon, NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    EditorUtility.DisplayDialog("Transparent Bundles " + bundle.Status, bundle.Status + "!\n\n " + bundle.Log, "Close");
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
                else if(bundle.Status == BundleStatus.Failed)
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
                else if(bundle.Status == BundleStatus.Failed)
                {
                    bundleStyle = BodyTextStyleError;
                }

            }

            Rect bundleButtonRec = GUILayoutUtility.GetRect(0, _bundleRowHeight, bundleStyle, GUILayout.ExpandWidth(true));

            if(bundle.Status == BundleStatus.Processing)
            {
                Rect progressRect = new Rect(bundleButtonRec.position.x - 3, bundleButtonRec.position.y + 2, (bundleButtonRec.width + 3) * _controller.ServerInfo.Progress, bundleButtonRec.height);
                Rect backgroundRec = new Rect(bundleButtonRec.position.x - 3, bundleButtonRec.position.y + 2, bundleButtonRec.width + 3, bundleButtonRec.height);
                GUI.DrawTexture(backgroundRec, _controller.DownloadImage(Config.IconsPath + Config.ProgressBarBkgImageName));
                GUI.DrawTexture(progressRect, _controller.DownloadImage(Config.IconsPath + Config.ProgressBarImageName));
            }

            string buttonText = bundle.Asset.Name.Length == 0 ? bundle.Name.Substring(0, bundle.Name.LastIndexOf('_')) : bundle.Asset.Name;
            if(GUI.Button(bundleButtonRec, buttonText, bundleStyle))
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
                        int firstIndex = IndexOfBundle(BundleList, firstSelectedBundle);
                        enumerator.Dispose();
                        int finalIndex = IndexOfBundle(BundleList, bundle);
                        _selectedList = new Dictionary<string, Bundle>();

                        if(firstIndex < finalIndex)
                        {
                            for(int i = firstIndex; i <= finalIndex; i++)
                            {
                                Bundle bundleToSelect = BundleList[i];
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
                                Bundle bundleToSelect = BundleList[i];
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
                inspectorDummy.SelectedBundle = bundle;
                Selection.activeObject = inspectorDummy;
            }
            EditorGUILayout.EndVertical();

            if(bundle.Status == BundleStatus.Processing || bundle.Status == BundleStatus.Queued)
            {
                var operationEnumerator = bundle.OperationQueue.GetEnumerator();
                for(int i = 0; operationEnumerator.MoveNext(); i++)
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(22));
                    GUILayout.Label("", GUILayout.Height(3));
                    DrawOperationIcon(operationEnumerator.Current.Value, (bundle.Status == BundleStatus.Processing && i == 0));
                    EditorGUILayout.EndVertical();
                }
                operationEnumerator.Dispose();
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[2]));
            GUILayout.Label("", GUILayout.Height(2));
            GUILayout.Label(PrintProperSize(bundle.Size[CurrentPlatform]), BodyTextStyle, GUILayout.Height(20), GUILayout.Width(_columnsSize[2]));
            EditorGUILayout.EndVertical();

            if(GUILayout.Button("↧ Download", GUILayout.Width(_columnsSize[3]), GUILayout.Height(22)))
            {
                _controller.DownloadBundle(bundle, CurrentPlatform);
            }

            EditorGUILayout.EndHorizontal();
        }

        static int IndexOfBundle(List<Bundle> bundleList, Bundle bundle)
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

        public static Texture2D DrawOperationIcon(BundleOperation operation, bool processing)
        {
            Texture2D icon = null;

            switch(operation)
            {
                case BundleOperation.create_asset_bundles:
                    icon = processing ? _controller.DownloadImage(Config.IconsPath + Config.UpdateImageName) : _controller.DownloadImage(Config.IconsPath + Config.UpdateQueuedImageName);
                    break;

                case BundleOperation.remove_asset_bundles:
                    icon = processing ? _controller.DownloadImage(Config.IconsPath + Config.RemoveImageName) : _controller.DownloadImage(Config.IconsPath + Config.RemoveQueuedImageName);
                    break;

                case BundleOperation.create_local_asset_bundles:
                    icon = processing ? _controller.DownloadImage(Config.IconsPath + Config.InBuildImageName) : _controller.DownloadImage(Config.IconsPath + Config.InBuildQueuedImageName);
                    break;

                case BundleOperation.remove_local_asset_bundles:
                    icon = processing ? _controller.DownloadImage(Config.IconsPath + Config.OutBuildImageName) : _controller.DownloadImage(Config.IconsPath + Config.OutBuildQueuedImageName);
                    break;

                case BundleOperation.deploy_to_production:
                    icon = processing ? _controller.DownloadImage(Config.IconsPath + Config.UpdateImageName) : _controller.DownloadImage(Config.IconsPath + Config.UpdateQueuedImageName);
                    break;
            }

            var iconContent = new GUIContent(icon, operation.ToString());

            float iconSize = 20;
            if(processing)
            {
                iconSize = iconSize * iconsProcessCurrentSize;
            }
            GUILayout.Label(iconContent, GUILayout.Height(iconSize), GUILayout.Width(iconSize));

            return icon;
        }

        static void UpdateBundleData()
        {
            _lastUpdateTime = Time.realtimeSinceStartup;
            _controller.LoadBundleDataFromServer();
        }

        static void CleanBundleLists()
        {
            CleanBundleList(ref _selectedList);
            CleanBundleList(ref _chosenList);
        }

        static void CleanBundleList(ref Dictionary<string, Bundle> list)
        {
            if(list != null && list.Count > 0)
            {
                var bundlesToRemoveFromList = new List<string>();
                var listEnum = list.GetEnumerator();
                while(listEnum.MoveNext())
                {
                    bool bundleExist = false;
                    for(int i = 0; i < BundleList.Count && !bundleExist; i++)
                    {
                        if(BundleList[i].Name == listEnum.Current.Key)
                        {
                            bundleExist = true;
                        }
                    }
                    if(!bundleExist)
                    {
                        bundlesToRemoveFromList.Add(listEnum.Current.Key);
                    }
                }
                listEnum.Dispose();
                for(int i = 0; i < bundlesToRemoveFromList.Count; i++)
                {
                    list.Remove(bundlesToRemoveFromList[i]);
                }
            }
        }

        static void SearchBundles(string filter)
        {
            BundleList = _controller.GetBundles(filter);
            ChangeSorting(_sorting);
            _bundlesInBuild = 0;
            for(int i = 0; i < BundleList.Count; i++)
            {
                Bundle bundle = BundleList[i];
                if(bundle.IsLocal)
                {
                    _bundlesInBuild++;
                }
            }
            CleanBundleLists();
            Window.Repaint();
        }

        static void ChangeSorting(BundleSortingMode mode)
        {
            _sorting = mode;
            _controller.SortBundles(_sorting, BundleList, CurrentPlatform);
        }

        static void ManageKeyInputs()
        {
            if(Event.current.isKey)
            {
                switch(Event.current.keyCode)
                {
                    case KeyCode.A:
                        if(Event.current.command || Event.current.control)
                        {
                            _selectedList = new Dictionary<string, Bundle>();
                            for(int i = 0; i < BundleList.Count; i++)
                            {
                                _selectedList.Add(BundleList[i].Name, BundleList[i]);
                            }
                        }
                        Window.Repaint();
                        break;
                }
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

            return converted + " " + units;
        }

        public static double BytesToMegabytes(int bytes)
        {
            return BytesToKilobytes(bytes) / 1024d;
        }

        public static double BytesToKilobytes(int bytes)
        {
            return bytes / 1024d;
        }

        private void ResetWindow()
        {
            if(_controller != null)
            {
                _controller.FlushImagesCache();
                OpenWindow();
                ResetStyles();
            }
        }

        void Update()
        {
            if(SceneManager.GetActiveScene() != _previousScene)
            {
                ResetWindow();
                _previousScene = SceneManager.GetActiveScene();
                return;
            }
            else if(EditorApplication.isPlaying)
            {
                _isPlaying = true;
            }
            else if(_isPlaying)
            {
                _isPlaying = false;
                ResetWindow();
                return;
            }

            iconsProcessCurrentSize = 0.9f + (Mathf.Max(Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.1f, 0f));
            if((_lastUpdateTime + _updateBundleDataDelay) < Time.realtimeSinceStartup)
            {
                UpdateBundleData();
            }
            SearchBundles(_filter);
            Repaint();
        }
    }
}
