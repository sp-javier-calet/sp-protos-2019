using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWindowAdmin : EditorWindow
    {
        private static GrayboxLibraryController _tool;
        private static GrayboxAsset _asset;
        private static int _assetToLoad = 0;
        private static string _newTag = "";
        private static List<string> _assetTags;
        private static List<string> _unassignedAssetTags;
        private string[] _tagList = new string[0];
        private const float _timeToSeacrhTags = 0.1f;
        private float _timeKeyPressed = Time.realtimeSinceStartup;
        private const float _keyDelay = 0.2f;
        private static int _focusChangeDelay = 0;
        private float _timeFilterUpdated = Time.realtimeSinceStartup;
        private bool _filterUpdated = true;
        private static bool _displayFilterOptions = false;
        private static string _currentSelectedOption = "";

        private UnityEngine.Object _mainAsset;
        private Rect _assetRect = new Rect();
        private Rect _packageRect = new Rect();
        private Rect _thumbRect = new Rect();
        private Rect _animThumbRect = new Rect();
        private static Texture2D _assetPreview;
        private static Texture2D _packagePreview;
        private static Texture2D _thumbPreview;
        private static Texture2D _animThumbPreview;

        private string _previousAsset = "";
        private bool _assetDrop = false;
        private string _previousPkg = "";
        private bool _pkgDrop = false;
        private string _previousThumb = "";
        private bool _thumbDrop = false;
        private string _previousAnimThumb = "";
        private bool _animThumbDrop = false;

        private static List<string> _existingAssets;
        private string _assetFilter = "";
        private float _timeAssetFilterUpdated = Time.realtimeSinceStartup;
        private static bool _assetFilterUpdated = true;
        private static bool _displayAssetFilterOptions = false;
        private static string _currentSelectedAssetOption = "";

        private GUIStyle _dropAreaStyle, _dropAreaTextStyle, _searchOptionStyle, _searchSelectedOptionStyle, _separatorStyle;


        [MenuItem("Social Point/Graybox Library Admin")]
        public static void Launch()
        {
            GrayboxLibraryWindowAdmin window = (GrayboxLibraryWindowAdmin)EditorWindow.GetWindow(typeof(GrayboxLibraryWindowAdmin));
            window.titleContent.text = "Graybox Admin";
            window.minSize = new Vector2(350, 750);
            _tool = new GrayboxLibraryController();
            _asset = new GrayboxAsset();
            _assetTags = new List<string>();
            _unassignedAssetTags = new List<string>();
            _existingAssets = new List<string>();

            _thumbPreview = new Texture2D(1, 1);
            _thumbPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _thumbPreview.Apply();

            _animThumbPreview = _tool.DownloadImage(GrayboxLibraryConfig.IconsPath + "animatedThumb.png");

            _assetPreview = new Texture2D(1, 1);
            _assetPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _assetPreview.Apply();

            _packagePreview = _tool.DownloadImage(GrayboxLibraryConfig.IconsPath + "package.png");
        }

        void OnGUI()
        {
            if(_tool == null)
                Launch();

            if(_dropAreaStyle == null)
            {
                _dropAreaStyle = new GUIStyle(GUI.skin.button);
                _dropAreaStyle.alignment = TextAnchor.MiddleCenter;
                _dropAreaStyle.wordWrap = true;
                _dropAreaStyle.hover.background = _dropAreaStyle.active.background;
            }
            if(_dropAreaTextStyle == null)
            {
                _dropAreaTextStyle = new GUIStyle(GUI.skin.label);
                _dropAreaTextStyle.alignment = TextAnchor.LowerCenter;
                _dropAreaTextStyle.wordWrap = true;
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

            GUILayout.BeginVertical();
            GUILayout.Label("Add new asset to Graybox Library", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2f - 25f));
            GUILayout.Label("Category", GUILayout.Width(60));
            Type grayboxAssetCategoryType = typeof(GrayboxAssetCategory);
            _asset.Category = (GrayboxAssetCategory)EditorGUILayout.Popup((int)_asset.Category, Enum.GetNames(grayboxAssetCategoryType), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Width(20));

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2f - 15f));
            GUILayout.Label("Name", GUILayout.Width(40));
            _asset.Name = EditorGUILayout.TextField(_asset.Name, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(20));
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Main Asset", GUILayout.Width(position.width - 120));
            _assetRect = GUILayoutUtility.GetRect(100, 100);
            if(GUI.Button(_assetRect, _asset.MainAssetPath.Length > 0 ? _assetPreview : Texture2D.blackTexture, _dropAreaStyle))
            {
                EditorGUIUtility.ShowObjectPicker<UnityEngine.Object>(null, false, "", 1);
            }
            if(EditorGUIUtility.GetObjectPickerControlID() == 1)
            {
                _mainAsset = EditorGUIUtility.GetObjectPickerObject();
                if(_mainAsset != null)
                {
                    string path = AssetDatabase.GetAssetPath(_mainAsset);
                    _asset.MainAssetPath = path.Length == 0 ? _asset.MainAssetPath : path;
                    LoadPreview(DragAndDropItemType.asset);
                }
            }
            Rect assetLabelRect = new Rect(0, _assetRect.y + 50, position.width - 120, 50);
            GUI.Label(assetLabelRect, _asset.MainAssetPath.Length == 0 ? "" : _asset.MainAssetPath.Substring(_asset.MainAssetPath.LastIndexOf("/") + 1), _dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Package", GUILayout.Width(position.width - 120));
            _packageRect = GUILayoutUtility.GetRect(100, 100);
            if(GUI.Button(_packageRect, _asset.PackagePath.Length > 0 ? _packagePreview : Texture2D.blackTexture, _dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Package", _asset.PackagePath.Length == 0 ? GrayboxLibraryConfig.PkgDefaultFolder : _asset.PackagePath, "unitypackage");
                _asset.PackagePath = path.Length == 0 ? _asset.PackagePath : path;
                if(_asset.Name.Length == 0)
                {
                    _asset.Name = Path.GetFileNameWithoutExtension(_asset.PackagePath);
                    _asset.Name = _asset.Name.Replace("[GRAYBOX]_", "");
                }
            }
            Rect labelRect = new Rect(0, _packageRect.y + 50, position.width - 120, 50);
            GUI.Label(labelRect, _asset.PackagePath.Length == 0 ? "" : _asset.PackagePath.Substring(_asset.PackagePath.LastIndexOf("/") + 1), _dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Thumbnail", GUILayout.Width(position.width - 120));
            _thumbRect = GUILayoutUtility.GetRect(100, 100);
            if(GUI.Button(_thumbRect, _asset.ThumbnailPath.Length > 0 ? _thumbPreview : Texture2D.blackTexture, _dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Thumbnail", _asset.ThumbnailPath.Length == 0 ? GrayboxLibraryConfig.PkgDefaultFolder : _asset.ThumbnailPath, "");
                _asset.ThumbnailPath = path.Length == 0 ? _asset.ThumbnailPath : path;
                LoadPreview(DragAndDropItemType.thumb);
            }
            Rect thumbLabelRect = new Rect(0, _thumbRect.y + 50, position.width - 120, 50);
            GUI.Label(thumbLabelRect, _asset.ThumbnailPath.Length == 0 ? "" : _asset.ThumbnailPath.Substring(_asset.ThumbnailPath.LastIndexOf("/") + 1), _dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Animated Thumbnail", GUILayout.Width(position.width - 120));
            _animThumbRect = GUILayoutUtility.GetRect(100, 100);
            if(GUI.Button(_animThumbRect, _asset.AnimatedThumbnailPath.Length > 0 ? _animThumbPreview : Texture2D.blackTexture, _dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Animated Thumbnail", _asset.AnimatedThumbnailPath.Length == 0 ? GrayboxLibraryConfig.PkgDefaultFolder : _asset.AnimatedThumbnailPath, "gif");
                _asset.AnimatedThumbnailPath = path.Length == 0 ? _asset.AnimatedThumbnailPath : path;
            }
            Rect animThumbLabelRect = new Rect(0, _animThumbRect.y + 50, position.width - 120, 50);
            GUI.Label(animThumbLabelRect, _asset.AnimatedThumbnailPath.Length == 0 ? "" : _asset.AnimatedThumbnailPath.Substring(_asset.AnimatedThumbnailPath.LastIndexOf("/") + 1), _dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", _separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tags", GUILayout.Width(70));
            DisplaySearchBar();
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                _asset = _tool.GetAsset(_asset.Name);
                if(_asset != null && EditorUtility.DisplayDialog("Remove asset: " + _asset.Name, "WARNING! You are about to remove the asset " + _asset.Name + " from the database. Keep in mind that you will not be able to rollback this operation. Do you want to delete it?", "REMOVE", "Cancel"))
                {
                    _tool.RemoveAsset(_asset);
                    _asset = new GrayboxAsset();
                }
            }

            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if(GUILayout.Button("Clear", GUILayout.Width(100)))
            {
                _asset = new GrayboxAsset();
            }
            if(GUILayout.Button("Save Asset", GUILayout.Width(100)))
            {
                _tool.RegisterAsset(_asset);
                _asset = _tool.GetAsset(_asset.Name);

                foreach(string tag in _unassignedAssetTags)
                {
                    _tool.UnassignTag(_asset, _tool.GetTag(tag));
                }

                foreach(string tag in _assetTags)
                {
                    _tool.AssignTag(_asset, _tool.GetTag(tag));
                }

                GrayboxAssetCategory category = _asset.Category;

                _asset = new GrayboxAsset();

                _asset.Category = category;

                EditorUtility.DisplayDialog("Graybox Library", "The asset has been succesfully registered to the Graybox Library", "Close");
            }
            GUILayout.EndVertical();

            GUILayout.Label("", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            GUILayout.Label("Load asset", GUILayout.Width(70));
            DisplayAssetSearchBar();
            if(GUILayout.Button("Load", GUILayout.Width(70)))
                LoadAsset();
            GUILayout.Label("", GUILayout.Width(10));
            GUILayout.EndHorizontal();
            GUILayout.Label("", GUILayout.Height(10));
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
            {
                if(_displayFilterOptions)
                    _currentSelectedOption = GUI.tooltip;
                else if(_displayAssetFilterOptions)
                    _currentSelectedAssetOption = GUI.tooltip;
            }

            ManageKeyInputs();
            ManageDragAndDrop();
        }

        private void ManageDragAndDrop()
        {
            Event evt = Event.current;

            if(DragAndDrop.paths.Length > 0)
            {
                if(_animThumbRect.width > 1 && _thumbRect.width > 1 && _packageRect.width > 1 && _assetRect.width > 1 && position.Contains(evt.mousePosition + position.position))
                {
                    string[] paths = DragAndDrop.paths;
                    if(_assetRect.Contains(evt.mousePosition) && DragAndDrop.objectReferences.Length > 0)
                    {
                        _mainAsset = DragAndDrop.objectReferences[0];
                        if(!_assetDrop && (_mainAsset == null || AssetDatabase.Contains(_mainAsset)))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            _assetDrop = true;
                            _previousAsset = _asset.MainAssetPath;
                            _asset.MainAssetPath = paths[0];
                            LoadPreview(DragAndDropItemType.asset);
                        }
                    }
                    else if(_packageRect.Contains(evt.mousePosition))
                    {
                        if(!_pkgDrop && DragAndDrop.paths[0].Contains(".unitypackage"))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            _pkgDrop = true;
                            _previousPkg = _asset.PackagePath;
                            _asset.PackagePath = paths[0];
                            if(_asset.Name.Length == 0)
                            {
                                _asset.Name = Path.GetFileNameWithoutExtension(_asset.PackagePath);
                                _asset.Name = _asset.Name.Replace("[GRAYBOX]_", "");
                            }
                        }
                    }
                    else if(_thumbRect.Contains(evt.mousePosition))
                    {
                        if(!_thumbDrop && (DragAndDrop.paths[0].Contains(".png") || DragAndDrop.paths[0].Contains(".jpg") || DragAndDrop.paths[0].Contains(".tga")))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            _thumbDrop = true;
                            _previousThumb = _asset.ThumbnailPath;
                            _asset.ThumbnailPath = paths[0];
                            LoadPreview(DragAndDropItemType.thumb);
                        }
                    }
                    else if(_animThumbRect.Contains(evt.mousePosition))
                    {
                        if(!_animThumbDrop && DragAndDrop.paths[0].Contains(".gif"))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            _animThumbDrop = true;
                            _previousAnimThumb = _asset.AnimatedThumbnailPath;
                            _asset.AnimatedThumbnailPath = paths[0];
                        }
                    }
                    else
                    {
                        RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                    }
                }
            }
            else
            {
                _previousPkg = _asset.PackagePath;
                _previousThumb = _asset.ThumbnailPath;
                _previousAnimThumb = _asset.AnimatedThumbnailPath;

                if(_previousAsset != _asset.MainAssetPath && _mainAsset != null && _asset.MainAssetPath.Length > 0)
                {
                    if(_asset.PackagePath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PkgDefaultFolder, _mainAsset.name + ".unitypackage", SearchOption.AllDirectories);
                        if(search.Length > 0)
                            _asset.PackagePath = search[0].Replace("\\", "/");
                    }
                    if(_asset.ThumbnailPath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PkgDefaultFolder, _mainAsset.name + ".*", SearchOption.AllDirectories);
                        foreach(string found in search)
                        {
                            if(found.Contains(".png") || found.Contains(".jpg") || found.Contains(".tga"))
                            {
                                _asset.ThumbnailPath = found.Replace("\\", "/");
                                LoadPreview(DragAndDropItemType.thumb);
                            }
                        }
                    }
                    if(_asset.AnimatedThumbnailPath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PkgDefaultFolder, _mainAsset.name + ".*", SearchOption.AllDirectories);
                        foreach(string found in search)
                        {
                            if(found.Contains(".gif"))
                            {
                                _asset.AnimatedThumbnailPath = found.Replace("\\", "/");
                            }
                        }
                    }
                    if(_asset.Name.Length == 0 && _asset.PackagePath.Length > 0)
                    {
                        _asset.Name = Path.GetFileNameWithoutExtension(_asset.PackagePath);
                        _asset.Name = _asset.Name.Replace("[GRAYBOX]_", "");
                    }
                }

                _previousAsset = _asset.MainAssetPath;
            }
        }

        public enum DragAndDropItemType
        {
            asset,
            package,
            thumb,
            animatedThumb}

        ;

        private void LoadPreview(DragAndDropItemType type)
        {
            switch(type)
            {
            case DragAndDropItemType.asset:

                if(_asset.MainAssetPath.Length > 0)
                {
                    _assetPreview = AssetPreview.GetMiniThumbnail(_mainAsset);
                }
                else
                {
                    _assetPreview = new Texture2D(1, 1);
                    _assetPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    _assetPreview.Apply();
                }
                break;

            case DragAndDropItemType.thumb:
                if(_asset.ThumbnailPath.Length > 0 && (_asset.ThumbnailPath.Contains(".png") || _asset.ThumbnailPath.Contains(".jpg") || _asset.ThumbnailPath.Contains(".tga")))
                    _thumbPreview.LoadImage(File.ReadAllBytes(_asset.ThumbnailPath));
                else
                {
                    _thumbPreview = new Texture2D(1, 1);
                    _thumbPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    _thumbPreview.Apply();
                }
                break;
            }
        }

        private void RecoverLastItemDragAndDrop(DragAndDropItemType type)
        {
            switch(type)
            {
            case DragAndDropItemType.asset:
                if(_assetDrop)
                {
                    _assetDrop = false;
                    _asset.MainAssetPath = _previousAsset;
                    LoadPreview(DragAndDropItemType.asset);
                }
                break;
            case DragAndDropItemType.package:
                if(_pkgDrop)
                {
                    _pkgDrop = false;
                    _asset.PackagePath = _previousPkg;
                    if(_asset.Name.Length == 0)
                    {
                        _asset.Name = Path.GetFileNameWithoutExtension(_asset.PackagePath);
                        _asset.Name = _asset.Name.Replace("[GRAYBOX]_", "");
                    }
                }
                break;
            case DragAndDropItemType.thumb:
                if(_thumbDrop)
                {
                    _thumbDrop = false;
                    _asset.ThumbnailPath = _previousThumb;
                    LoadPreview(DragAndDropItemType.thumb);
                }
                break;
            case DragAndDropItemType.animatedThumb:
                if(_animThumbDrop)
                {
                    _animThumbDrop = false;
                    _asset.AnimatedThumbnailPath = _previousAnimThumb;
                }
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
                        if(_displayFilterOptions && _currentSelectedOption.Length != 0)
                            AddTag(_currentSelectedOption);
                        else if(_displayAssetFilterOptions)
                        {
                            _assetFilter = _currentSelectedAssetOption;
                            _displayAssetFilterOptions = false;
                            LoadAsset();
                        }

                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;

                    case KeyCode.Escape:

                        if (_timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                        {
                            if (_displayFilterOptions && _currentSelectedOption.Length != 0)
                            {
                                _newTag = "";
                                _currentSelectedOption = "";
                                _displayFilterOptions = false;
                                _focusChangeDelay = 1;
                            }
                            else if (_displayAssetFilterOptions)
                            {
                                _assetFilter = "";
                                _displayAssetFilterOptions = false;
                            }

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
                    else if(_displayAssetFilterOptions && _timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        int currentIndex = _existingAssets.IndexOf(_currentSelectedAssetOption);
                        if(currentIndex < _existingAssets.Count - 1)
                            _currentSelectedAssetOption = _existingAssets[currentIndex + 1];
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
                    else if(_displayAssetFilterOptions && _timeKeyPressed + _keyDelay < Time.realtimeSinceStartup)
                    {
                        int currentIndex = _existingAssets.IndexOf(_currentSelectedAssetOption);
                        if(currentIndex > 0)
                            _currentSelectedAssetOption = _existingAssets[currentIndex - 1];
                        _timeKeyPressed = Time.realtimeSinceStartup;
                    }
                    break;
                }
            }
        }

        private void DisplayTags()
        {
            for(int i = 0; i < _assetTags.Count; i++)
            {
                string tag = _assetTags[i];

                if(GUILayout.Button("x  " + tag, GUILayout.ExpandWidth(false)))
                {
                    _unassignedAssetTags.Add(tag);
                    _assetTags.Remove(tag);
                    break;
                }
            }
        }

        private void DisplaySearchBar()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            int previousFilterCount = _assetTags.Count;
            DisplayTags();
            if (previousFilterCount != _assetTags.Count)
                DisplayTags();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            string previousFilter = _newTag;

            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            GUI.SetNextControlName("SearchBar");
            _newTag = GUILayout.TextField(_newTag, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.EndVertical();

            if(previousFilter != _newTag && _newTag.Length > 0)
            {
                _filterUpdated = true;
                _timeFilterUpdated = Time.realtimeSinceStartup;
            }

            if(_filterUpdated && _timeFilterUpdated + _timeToSeacrhTags < Time.realtimeSinceStartup)
            {
                _filterUpdated = false;
                _displayFilterOptions = false;
                _tagList = _tool.GetTagsAsText(_newTag, 0, 10);
                if(_tagList.Length > 0 && _newTag.Length > 0)
                    _displayFilterOptions = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(22), GUILayout.Height(25));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(1));
            if(GUILayout.Button("+", GUILayout.Width(22), GUILayout.Height(22)))
            {
                if(_newTag.Length > 0 && EditorUtility.DisplayDialog("Create New Tag", "You are about to create a new tag. Are you sure?", "Yes, create it", "Cancel"))
                {
                    GrayboxTag tag = new GrayboxTag();
                    tag.Name = _newTag;
                    _tool.CreateTag(tag);
                    _assetTags.Add(tag.Name);
                    _newTag = "";
                }
            }
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if(_displayFilterOptions && _newTag.Length > 0)
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

        private void DisplayAssetSearchBar()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            string previousAssetFilter = _assetFilter;

            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            _assetFilter = GUILayout.TextField(_assetFilter, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.EndVertical();

            if(previousAssetFilter != _assetFilter && _assetFilter.Length > 0)
            {
                _assetFilterUpdated = true;
                _timeAssetFilterUpdated = Time.realtimeSinceStartup;
            }

            if(_assetFilterUpdated && _timeAssetFilterUpdated + _timeToSeacrhTags < Time.realtimeSinceStartup)
            {
                _assetFilterUpdated = false;
                _displayAssetFilterOptions = false;
                string[] filterSplited = _assetFilter.Split(' ');
                _existingAssets = new List<string>(_tool.GetAssetsByNameAsText(filterSplited, _asset.Category, 0, 10));

                if(_existingAssets.Count > 0 && _assetFilter.Length > 0)
                    _displayAssetFilterOptions = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(22), GUILayout.Height(25));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(1));
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if(_displayAssetFilterOptions && _assetFilter.Length > 0)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                for(int i = 0; i < _existingAssets.Count; i++)
                {
                    string option = _existingAssets[i];

                    if(_currentSelectedAssetOption == option)
                    {
                        if(GUILayout.Button(new GUIContent(option, option), _searchSelectedOptionStyle))
                        {
                            _assetFilter = option;
                            _displayAssetFilterOptions = false;
                            LoadAsset();
                        }
                    }
                    else
                    {
                        if(GUILayout.Button(new GUIContent(option, option), _searchOptionStyle))
                        {
                            _assetFilter = option;
                            _displayAssetFilterOptions = false;
                            LoadAsset();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();

        }

        public void LoadAsset()
        {
            _asset = _tool.GetAsset(_existingAssets[_assetToLoad]);
            _assetTags = new List<string>(_tool.GetAssetTagsAsText(_asset));
            _mainAsset = AssetDatabase.LoadMainAssetAtPath(_asset.MainAssetPath);
            LoadPreview(DragAndDropItemType.asset);
            LoadPreview(DragAndDropItemType.thumb);
        }

        public static void AddTag(string tag)
        {
            _assetTags.Add(tag);
            _newTag = "";
            _currentSelectedOption = "";
            _displayFilterOptions = false;
            _focusChangeDelay = 1;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnDestroy()
        {
            _tool.Disconnect();
        }
    }
}

