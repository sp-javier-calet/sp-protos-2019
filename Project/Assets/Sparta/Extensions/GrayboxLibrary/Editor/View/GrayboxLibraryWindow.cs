using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWindow : EditorWindow
    {
        private static ArrayList _currentAssetList;
        private static ArrayList _currentGUIContent;
        public static GrayboxLibraryController Tool;
        private static List<GrayboxAsset> _toInstantiate;
        private static List<GrayboxAsset> _toDownload;
        private static Dictionary<string, Vector3> _instantiatePositions;
        private static Dictionary<string, Transform> _instantiateParents;

        private static string[] _categories;
        public static string Filter = "";
        public static List<string> Filters = new List<string>();
        private static float _timeFilterUpdated = Time.realtimeSinceStartup;
        private static bool _filterUpdated = false;
        private static bool _displayFilterOptions = false;
        private static string _currentSelectedOption = "";
        private static string[] _tagList = new string[0];
        private static float _timeToSearchTags = 0.1f;
        private static int _currentCategory = 0;
        private static int _currentPage = 0;
        private static int _maxPage = 0;
        private Vector2 _scrollPos;
        private static GUIStyle _buttonStyle, _buttonAreaStyle, _simpleButtonStyle, _upperMenuStyle, _bottomMenuStyle, _bottomMenuTextStyle, _bottomMenuTextBoldStyle, _searchOptionStyle, _searchSelectedOptionStyle, _separatorStyle;
        public static float ThumbWidth = 512;
        public static float ThumbHeight = 512;
        public static float AnimatedThumbWidth = 256;
        public static float AnimatedThumbHeight = 256;
        private float _thumbSizeMultiplier = 0.3f;
        private const float _thumbMinSize = 0.1f;
        private const float _thumbMaxSize = 0.5f;
        private const int _assetsPerPage = 20;

        public static GrayboxLibraryWindow Window;

        private static float _timeKeyPressed = Time.realtimeSinceStartup;
        private const float _keyDelay = 0.2f;
        private static int _focusChangeDelay = 0;

        public static GrayboxAsset AssetChosen = null;
        public static GrayboxAsset AssetDragged = null;
        private static bool _dragging = false;
        private static string _currentDraggedAsset = "";

        private static bool _secondGUIDraw = false;
        private static bool _wasInactive = false;
        private static Scene _previousScene = SceneManager.GetActiveScene();

        private static GrayboxLibraryInspectorDummy _inspectorDummyA, _inspectorDummyB;
        private static int _currentInspectorDummy = 0;

        public static void LaunchClient()
        {
            Tool = new GrayboxLibraryController();
            Tool.FlushImageCache();
            _currentPage = 0;

            _currentAssetList = Tool.GetAssets(Filters.ToArray(), (GrayboxAssetCategory)_currentCategory, _currentPage * _assetsPerPage, _assetsPerPage);
            _currentGUIContent = new ArrayList();
            _toInstantiate = new List<GrayboxAsset>();
            _toDownload = new List<GrayboxAsset>();
            _instantiatePositions = new Dictionary<string, Vector3>();
            _instantiateParents = new Dictionary<string, Transform>();

            _categories = Enum.GetNames(typeof(GrayboxAssetCategory));
            Filter = "";
            Filters = new List<string>();
            _timeFilterUpdated = Time.realtimeSinceStartup;
            _filterUpdated = false;
            _displayFilterOptions = false;
            _currentSelectedOption = "";
            _tagList = new string[0];
            _maxPage = (int)Math.Ceiling(Tool.GetAssetCount(Filters.ToArray(), (GrayboxAssetCategory)_currentCategory) / (float)_assetsPerPage);
            _buttonStyle = null;
            _buttonAreaStyle = null;
            _simpleButtonStyle = null;
            _upperMenuStyle = null;
            _bottomMenuStyle = null;
            _bottomMenuTextStyle = null;
            _bottomMenuTextBoldStyle = null;
            _searchOptionStyle = null;
            _searchSelectedOptionStyle = null;
            _separatorStyle = null;

            _timeKeyPressed = Time.realtimeSinceStartup;
            _focusChangeDelay = 0;
            AssetChosen = null;
            AssetDragged = null;
            _dragging = false;
            _currentDraggedAsset = "";
            _secondGUIDraw = false;
            _wasInactive = false;
            _previousScene = SceneManager.GetActiveScene();
            _inspectorDummyA = null;
            _inspectorDummyB = null;
            _currentInspectorDummy = 0;

            LoadThumbnails();

            Window = (GrayboxLibraryWindow)EditorWindow.GetWindow(typeof(GrayboxLibraryWindow));
            Window.titleContent.text = "Library";
        }

        [MenuItem("Social Point/Graybox Library/Buildings")]
        public static void LaunchBuldingsClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Buildings;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Props")]
        public static void LaunchPropsClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Props;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Decos")]
        public static void LaunchDecosClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Decos;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Fx")]
        public static void LaunchFxClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Fx;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Characters")]
        public static void LaunchCharactersClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Characters;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Vehicles")]
        public static void LaunchVehiclesClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.Vehicles;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/UI")]
        public static void LaunchUIClient()
        {
            _currentCategory = (int)GrayboxAssetCategory.UI;
            LaunchClient();
        }




        void OnGUI()
        {
            if(EditorApplication.isCompiling)
            {
                _wasInactive = true;
                if(Tool != null)
                    Tool.Disconnect();
                GUILayout.Label("Project compiling, please wait...");
                return;
            }
            else if(EditorApplication.isPlaying)
            {
                _wasInactive = true;
                if(Tool != null)
                    Tool.Disconnect();
                GUILayout.Label("Project playing, please stop the game before importing using this tool.");
                return;
            }
            else if(SceneManager.GetActiveScene() != _previousScene && !_wasInactive)
            {
                _wasInactive = true;
                if(Tool != null)
                    Tool.Disconnect();
            }
            else if(_wasInactive && Event.current.type == EventType.Repaint)
            {
                _wasInactive = false;
                LaunchClient();
                return;
            }
            else if(_wasInactive)
                return;

            if(Tool == null)
            {
                LaunchClient();
                return;
            }

            ManageDragAndDrop();

            if(Event.current.clickCount == 2 && AssetChosen != null && _secondGUIDraw)
                InstantiateAsset();

            if(_simpleButtonStyle == null)
            {
                _simpleButtonStyle = new GUIStyle(GUI.skin.label);
                _simpleButtonStyle.border = new RectOffset(0, 0, 0, 0);
                _simpleButtonStyle.margin = _simpleButtonStyle.border;
            }
            if(_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.label);
                _buttonStyle.border = new RectOffset(0, 0, 0, 0);
                _buttonStyle.margin = new RectOffset(10, 10, 10, 0);
            }
            if(_buttonAreaStyle == null)
            {
                _buttonAreaStyle = new GUIStyle(GUI.skin.label);
                Texture2D texH = new Texture2D(1, 1);
                texH.SetPixel(0, 0, new Color(0f, 0.2f, 0.5f, 0.5f));
                texH.Apply();
                _buttonAreaStyle.active.background = Texture2D.blackTexture;
                _buttonAreaStyle.hover.background = texH;

                _buttonAreaStyle.border = new RectOffset(0, 0, 0, 0);
                _buttonAreaStyle.margin = new RectOffset(0, 0, 0, 10);
            }
            if(_upperMenuStyle == null)
            {
                _upperMenuStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
                tex.Apply();
                _upperMenuStyle.normal.background = tex;
            }
            if(_bottomMenuStyle == null)
            {
                _bottomMenuStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.25f, 1f));
                tex.Apply();
                _bottomMenuStyle.normal.background = tex;
            }
            if(_separatorStyle == null)
            {
                _separatorStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 1f));
                tex.Apply();
                _separatorStyle.normal.background = tex;
                _separatorStyle.border = new RectOffset(0, 0, 0, 0);
                _separatorStyle.margin = _separatorStyle.border;
            }
            if(_bottomMenuTextStyle == null)
            {
                _bottomMenuTextStyle = new GUIStyle(GUI.skin.label);
                _bottomMenuTextStyle.fontSize = 12;
            }
            if(_bottomMenuTextBoldStyle == null)
            {
                _bottomMenuTextBoldStyle = new GUIStyle(_bottomMenuTextStyle);
                _bottomMenuTextBoldStyle.fontStyle = FontStyle.Bold;
            }
            if(_searchOptionStyle == null)
            {
                _searchOptionStyle = new GUIStyle(GUI.skin.label);
                _searchOptionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                _searchOptionStyle.alignment = TextAnchor.UpperLeft;
                _searchOptionStyle.active.textColor = new Color(0.6f, 0.6f, 0.6f);
                _searchOptionStyle.hover.textColor = new Color(1f, 1f, 1f);
                Texture2D texH = new Texture2D(1, 1);
                texH.SetPixel(0, 0, new Color(0f, 0.2f, 0.5f, 0.5f));
                texH.Apply();
                _searchOptionStyle.active.background = Texture2D.blackTexture;
                _searchOptionStyle.hover.background = texH;
                _searchOptionStyle.border = new RectOffset(0, 0, 0, 0);
                _searchOptionStyle.margin = _searchOptionStyle.border;
            }
            if(_searchSelectedOptionStyle == null)
            {
                _searchSelectedOptionStyle = new GUIStyle(_searchOptionStyle);
                _searchSelectedOptionStyle.normal = _searchSelectedOptionStyle.hover;
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(_upperMenuStyle);
            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(7));
            int previousCategory = _currentCategory;
            _currentCategory = EditorGUILayout.Popup(_currentCategory, _categories, GUILayout.Width(120), GUILayout.Height(20));
            if(previousCategory != _currentCategory)
            {
                Filters = new List<string>();
                _displayFilterOptions = false;
                Search(Filters);
            }

            GUILayout.EndVertical();

            GUILayout.Label("", GUILayout.Width(100));

            DisplaySearchBar();

            GUILayout.EndHorizontal();
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(false));
            if(_thumbSizeMultiplier == _thumbMinSize)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                for(int i = 0; i < _currentAssetList.Count; i++)
                {
                    GrayboxAsset asset = (GrayboxAsset)_currentAssetList[i];

                    if(Event.current.clickCount < 2 && GUILayout.Button(asset.Name, _searchOptionStyle))
                    {
                        AssetChosen = asset;
                        DisplayInspector();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("", GUILayout.Height(15));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                int column = 0;
                for(int i = 0; i < _currentAssetList.Count; i++)
                {
                    float buttonHeight = ThumbHeight * _thumbSizeMultiplier;
                    float buttonWidth = ThumbWidth * _thumbSizeMultiplier;

                    if(((column + 1) * (buttonWidth + 30)) > position.width)
                    {
                        column = 0;
                        GUILayout.Label("", GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    }

                    GrayboxAsset asset = (GrayboxAsset)_currentAssetList[i];

                    GUILayout.BeginVertical(_buttonAreaStyle, GUILayout.MaxWidth(buttonWidth));

                    if(Event.current.clickCount < 2 && GUILayout.Button((GUIContent)_currentGUIContent[i], _buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight), GUILayout.ExpandWidth(false)))
                    {
                        AssetChosen = asset;
                        DisplayInspector();
                    }

                    GUILayout.Label(asset.Name, GUILayout.Width(buttonWidth));
                    GUILayout.EndVertical();

                    column++;
                }
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));

            GUILayout.BeginHorizontal(_bottomMenuStyle, GUILayout.Height(25));
            GUILayout.Label("Page " + (_currentPage + 1) + "/" + _maxPage, _bottomMenuTextStyle, GUILayout.Width(80));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if(_currentPage > 0)
            {
                if(GUILayout.Button("<<", _bottomMenuTextStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    changePage(0);

                if(GUILayout.Button("<", _bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    changePage(_currentPage - 1);
            }
            else
                GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(20));

            if(_maxPage > 1)
            {
                for(int i = 1; i <= _maxPage && i < 10; i++)
                {
                    if(_currentPage == (i - 1))
                        GUILayout.Button(i.ToString(), _bottomMenuTextBoldStyle, GUILayout.Width(15), GUILayout.Height(20));
                    else
                    {
                        if(GUILayout.Button(i.ToString(), _bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                            changePage(i - 1);
                    }
                }
            }

            if(_currentPage < (_maxPage - 1))
            {
                if(GUILayout.Button(">", _bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    changePage(_currentPage + 1);

                if(GUILayout.Button(">>", _bottomMenuTextStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    changePage(_maxPage - 1);
            }
            else
                GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(20));

            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            _thumbSizeMultiplier = GUILayout.HorizontalSlider(_thumbSizeMultiplier, _thumbMinSize, _thumbMaxSize, GUILayout.Width(50));

            if(GUILayout.Button("Contact", GUILayout.Width(100)))
                Application.OpenURL(GrayboxLibraryConfig.ContactUrl);

            if(GUILayout.Button(Tool.DownloadImage(GrayboxLibraryConfig.IconsPath + "help.png"), _simpleButtonStyle, GUILayout.Width(23), GUILayout.Height(23)))
                Application.OpenURL(GrayboxLibraryConfig.HelpUrl);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if(_focusChangeDelay > 0)
                _focusChangeDelay--;
            else if(_focusChangeDelay == 0)
            {
                _focusChangeDelay = -1;
                EditorGUI.FocusTextInControl("");
                EditorGUI.FocusTextInControl("SearchBar");
            }

            if(GUI.tooltip.Length > 0)
                _currentSelectedOption = GUI.tooltip;

            if(_secondGUIDraw)
            {
                _secondGUIDraw = false;
                _currentDraggedAsset = GUI.tooltip;
            }
            else
                _secondGUIDraw = true;

            if(_dragging && AssetDragged != null)
            {
                GUI.DrawTexture(new Rect(Event.current.mousePosition, new Vector2(ThumbWidth / 1.5f, ThumbHeight / 1.5f)), AssetDragged.Thumbnail);
                EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, position.size), MouseCursor.MoveArrow);
            }

            ManageKeyInputs();
        }



        public void changePage(int pageIndex)
        {
            _currentPage = pageIndex;
            Search(Filters, false, false);
        }





        public static void DisplayInspector()
        {
            if(_inspectorDummyA == null)
            {
                _inspectorDummyA = ScriptableObject.CreateInstance<GrayboxLibraryInspectorDummy>();
                _inspectorDummyA.hideFlags = HideFlags.DontSave;
            }
            if(_inspectorDummyB == null)
            {
                _inspectorDummyB = ScriptableObject.CreateInstance<GrayboxLibraryInspectorDummy>();
                _inspectorDummyB.hideFlags = HideFlags.DontSave;
            }

            if(_currentInspectorDummy == 1)
            {
                Selection.activeObject = _inspectorDummyA;
                _currentInspectorDummy = 0;
            }
            else
            {
                Selection.activeObject = _inspectorDummyB;
                _currentInspectorDummy = 1;
            }
        }




        private void DisplaySearchBar()
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("", GUILayout.Height(3));

            EditorGUILayout.BeginHorizontal();
            int previousFilterCount = Filters.Count;
            DisplayTags();
            if(previousFilterCount != Filters.Count)
                DisplayTags();

            string previousFilter = Filter;
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(2));

            EditorGUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("SearchBar");
            Filter = EditorGUILayout.TextField(Filter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true), GUILayout.Height(20));
            if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                Filter = "";
                Filters = new List<string>();
                _displayFilterOptions = false;
                Search(Filters);
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if(previousFilter != Filter && Filter.Length > 0)
            {
                _filterUpdated = true;
                _timeFilterUpdated = Time.realtimeSinceStartup;
            }

            if(_filterUpdated && _timeFilterUpdated + _timeToSearchTags < Time.realtimeSinceStartup)
            {
                _filterUpdated = false;
                _displayFilterOptions = false;
                _tagList = Tool.GetTagsInCategoryAsText(Filter, (GrayboxAssetCategory)_currentCategory, 0, 10);
                if(_tagList.Length > 0 && Filter.Length > 0)
                    _displayFilterOptions = true;
            }

            EditorGUILayout.EndHorizontal();

            if(_displayFilterOptions && Filter.Length > 0)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                for(int i = 0; i < _tagList.Length; i++)
                {
                    string option = _tagList[i];

                    if(_currentSelectedOption == option)
                    {
                        if(GUILayout.Button(new GUIContent(option, option), _searchSelectedOptionStyle))
                            AddTag(option);
                    }
                    else
                    {
                        if(GUILayout.Button(new GUIContent(option, option), _searchOptionStyle))
                            AddTag(option);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();

        }


        private void ManageDragAndDrop()
        {
            Event evt = Event.current;

            if(evt.clickCount > 0 && _dragging)
            {
                if(AssetDragged != null && !position.Contains(evt.mousePosition + position.position))
                {
                    if(AssetDragged.Category != GrayboxAssetCategory.UI && SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.position.Contains(evt.mousePosition + position.position))
                    {
                        Vector2 mouseScreendPos = evt.mousePosition + position.position - SceneView.lastActiveSceneView.position.position;
                        Ray _raycast = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(mouseScreendPos.x, Screen.height - (mouseScreendPos.y + position.height - SceneView.lastActiveSceneView.camera.pixelHeight), 0));
                        Plane ground = new Plane(new Vector3(0, 1, 0), Vector3.zero);
                        float distanceToHit = 0;
                        ground.Raycast(_raycast, out distanceToHit);
                        InstantiateAssetAtPosition(_raycast.GetPoint(distanceToHit), true);
                    }
                    else
                        InstantiateAsset(true);
                }
            }

            switch(evt.type)
            {
            case EventType.mouseDown:
                if(Event.current.clickCount == 1 && AssetDragged == null && _currentDraggedAsset.Length > 0)
                    AssetDragged = Tool.GetAsset(_currentDraggedAsset);
                break;
            case EventType.mouseUp:
                _dragging = false;
                AssetDragged = null;
                break;
            case EventType.MouseDrag:
                _dragging = true;
                break;
            }
        }


        private void ManageKeyInputs()
        {
            if(Event.current.isKey)
            {
                switch(Event.current.keyCode)
                {
                case KeyCode.Return:

                    if(_timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        if(_currentSelectedOption.Length == 0)
                            Search(Filters);
                        else if(_displayFilterOptions)
                            AddTag(_currentSelectedOption);

                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;

                case KeyCode.Escape:

                    if(_timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        Filter = "";
                        _currentSelectedOption = "";
                        _displayFilterOptions = false;
                        _focusChangeDelay = 1;

                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;

                case KeyCode.DownArrow:

                    if(_displayFilterOptions && _timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        int currentIndex = ArrayUtility.IndexOf(_tagList, _currentSelectedOption);
                        if(currentIndex < _tagList.Length - 1)
                            _currentSelectedOption = _tagList[currentIndex + 1];
                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;

                case KeyCode.UpArrow:
                    if(_displayFilterOptions && _timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        int currentIndex = ArrayUtility.IndexOf(_tagList, _currentSelectedOption);
                        if(currentIndex > 0)
                            _currentSelectedOption = _tagList[currentIndex - 1];
                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;
                }
            }
        }

        private void DisplayTags()
        {
            for(int i = 0; i < Filters.Count; i++)
            {
                string tag = Filters[i];

                if(GUILayout.Button("x  " + tag, GUILayout.ExpandWidth(false)))
                {
                    RemoveTag(tag);
                    break;
                }
            }
        }

        private static void Search(List<string> filters, bool resetPage = true, bool updateMaxPage = true)
        {
            if(resetPage)
                _currentPage = 0;
            if(updateMaxPage)
                _maxPage = (int)Math.Ceiling(Tool.GetAssetCount(filters.ToArray(), (GrayboxAssetCategory)_currentCategory) / (float)_assetsPerPage);
            _currentGUIContent = new ArrayList();
            _currentAssetList = Tool.GetAssets(filters.ToArray(), (GrayboxAssetCategory)_currentCategory, _currentPage * _assetsPerPage, _assetsPerPage);
            LoadThumbnails();
        }

        private static void LoadThumbnails()
        {
            for(int i = 0; i < _currentAssetList.Count; i++)
            {
                GrayboxAsset asset = (GrayboxAsset)_currentAssetList[i];
                _currentGUIContent.Add(new GUIContent(asset.Thumbnail, asset.Name));
            }
        }

        public static void AddTag(string tag)
        {
            Filters.Add(tag);
            Filter = "";
            _currentSelectedOption = "";
            _displayFilterOptions = false;
            _focusChangeDelay = 1;
            Search(Filters);
        }

        public static void RemoveTag(string tag)
        {
            Filters.Remove(tag);
            Search(Filters);
        }

        public static GrayboxAsset InstantiateAsset(bool dragAndDrop = false)
        {
            GrayboxAsset asset = null;

            if(dragAndDrop)
            {
                if(AssetDragged != null)
                {
                    asset = AssetDragged;
                    _toDownload.Add(AssetDragged);
                }
            }
            else if(AssetChosen != null)
            {
                asset = AssetChosen;
                _toDownload.Add(AssetChosen);
            }

            _dragging = false;
            AssetDragged = null;
            _focusChangeDelay = 0;

            return asset;
        }


        public static void InstantiateAssetAtPosition(Vector3 position, bool dragAndDrop = false)
        {
            GrayboxAsset asset = InstantiateAsset(dragAndDrop);
            if(!_instantiatePositions.ContainsKey(asset.Name) && asset.MainAssetPath.Length > 0)
                _instantiatePositions.Add(asset.Name, position);
        }


        public static void DownloadAsset(GrayboxAsset asset, Transform parent = null)
        {
            Tool.DownloadAsset(asset);
            if(parent != null && !_instantiateParents.ContainsKey(asset.Name) && asset.MainAssetPath.Length > 0)
                _instantiateParents.Add(asset.Name, parent);
            _toInstantiate.Add(asset);
            ClearDownload(asset);
        }

        public static void ClearDownload(GrayboxAsset asset)
        {
            if(_toDownload.Contains(asset))
                _toDownload.Remove(asset);
        }


        void Update()
        {
            if(_toDownload != null)
            {
                for(int i = 0; i < _toDownload.Count; i++)
                {
                    if(_toDownload[i].Category == GrayboxAssetCategory.UI)
                    {
                        UnityEngine.Object[] canvasList = GameObject.FindObjectsOfType(typeof(Canvas));
                        if(canvasList.Length == 0)
                        {
                            EditorUtility.DisplayDialog("Graybox Library", "You need to have exactly one Canvas in your current scene in order to download the UI graybox asset. The asset will not be downloaded.", "Close");
                            ClearDownload(_toDownload[i]);
                        }
                        else if(canvasList.Length > 1)
                        {
                            GrayboxLibraryCanvasWindow.Launch(canvasList, _toDownload[i]);
                            ClearDownload(_toDownload[i]);
                        }
                        else
                            DownloadAsset(_toDownload[i]);
                    }
                    else
                    {
                        if(_toDownload[i].MainAssetPath.Length > 0)
                        {
                            if(AssetDatabase.LoadMainAssetAtPath(_toDownload[i].MainAssetPath) != null)
                                AssetDatabase.DeleteAsset(_toDownload[i].MainAssetPath);
                            _toInstantiate.Add(_toDownload[i]);
                        }

                        Tool.DownloadAsset(_toDownload[i]);
                        _toDownload.RemoveAt(i);
                    }
                }
            }

            if(_toInstantiate != null)
            {
                for(int i = 0; i < _toInstantiate.Count; i++)
                {
                    if(AssetDatabase.LoadMainAssetAtPath(_toInstantiate[i].MainAssetPath) != null)
                    {
                        GameObject instance;
                        if(_instantiateParents.ContainsKey(_toInstantiate[i].Name))
                        {
                            instance = Tool.InstantiateAsset(_toInstantiate[i], _instantiateParents[_toInstantiate[i].Name]);
                            _instantiateParents.Remove(_toInstantiate[i].Name);
                        }
                        else
                            instance = Tool.InstantiateAsset(_toInstantiate[i]);

                        if(_instantiatePositions.ContainsKey(_toInstantiate[i].Name))
                        {
                            instance.transform.position = _instantiatePositions[_toInstantiate[i].Name];
                            _instantiatePositions.Remove(_toInstantiate[i].Name);
                        }
                        _toInstantiate.RemoveAt(i);
                    }
                }
            }
        }


        void OnDestroy()
        {
            Tool.Disconnect();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}