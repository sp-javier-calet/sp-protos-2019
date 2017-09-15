using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    [CustomEditor(typeof(InspectorDummy))]
    public class Inspector : Editor
    {
        Asset _selectedAsset;
        Bundle _selectedBundle;
        InspectorAsset _inspectorAsset;
        EditorClientController _controller;
        InspectorDummy _dummy;
        float[] _columnsSize;
        Vector2 _scrollPos;
        string _errorText = "";

        void OnEnable()
        {
            EditorApplication.update += Update;

            if(target != null)
            {
                _dummy = (InspectorDummy)target;
                if(_dummy.SelectedAsset.GetAssetObject() == null)
                {
                    _errorText = ErrorDisplay.DisplayError(ErrorType.assetNotFoundInBundle, false, true, true, _dummy.SelectedBundle == null ? _dummy.SelectedAsset.Name : _dummy.SelectedBundle.Name, _dummy.SelectedAsset.Guid);
                }
                else
                {
                    _selectedAsset = _dummy.SelectedAsset;
                }
                _selectedBundle = _dummy.SelectedBundle;
            }

            _controller = EditorClientController.GetInstance();
            _columnsSize = new[] { 20f, 50f, 50f, 100f };
            _inspectorAsset = new InspectorAsset(_selectedAsset, _selectedBundle, _controller, _columnsSize);

            _scrollPos = Vector2.zero;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        protected override void OnHeaderGUI()
        {
        }

        public override void OnInspectorGUI()
        {
            if(_errorText.Length > 0)
            {
                BundlesWindow.InitStyles();

                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(20));
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                GUILayout.TextArea(_errorText, GUILayout.ExpandWidth(false));
                GUILayout.Label("", GUILayout.Height(10));
                _inspectorAsset.PrintAssetView();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else if(_selectedAsset != null)
            {
                BundlesWindow.InitStyles();

                EditorGUILayout.BeginVertical();
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                _inspectorAsset.PrintAssetView();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        void Update()
        {
            Repaint();
        }

        enum InspectorAssetType
        {
            Dependency,
            Reference

        }

        class InspectorAsset
        {
            Asset _selectedAsset;
            Bundle _selectedBundle;
            EditorClientController _controller;
            float[] _columnsSize;
            List<Asset> _references;
            Texture2D _preview;
            Texture2D _typeIcon;
            List<string> _shownHierarchy;

            public InspectorAsset(Asset selectedAsset, Bundle selectedBundle, EditorClientController controller, float[] columnsSize)
            {
                _selectedAsset = selectedAsset;
                _selectedBundle = selectedBundle;
                _controller = controller;
                _columnsSize = columnsSize;

                if(_selectedAsset != null)
                {
                    _references = GetAssetReferences(_selectedAsset);
                    if(_references.Count == 0)
                    {
                        _references.Add(selectedAsset);
                    }

                    _controller.SortAssets(AssetSortingMode.TypeAsc, _references);

                    Object assetObject = _selectedAsset.GetAssetObject();
                    _typeIcon = AssetPreview.GetMiniThumbnail(assetObject);

                    for(int i = 0; i < 10 && _preview == null; i++)
                    {
                        _preview = AssetPreview.GetAssetPreview(assetObject);
                        System.Threading.Thread.Sleep(10);
                    }
                    if(_preview == null)
                    {
                        _preview = _typeIcon;
                    }

                    _shownHierarchy = new List<string>();
                }
            }


            public void PrintAssetView()
            {
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

                GUILayout.Label("", GUILayout.Height(9));

                EditorGUILayout.BeginHorizontal();

                Rect previewRect = GUILayoutUtility.GetRect(170, 170, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(previewRect, _preview == null ? _controller.DownloadImage(Config.IconsPath + Config.MissingFileImageName) : _preview);

                EditorGUILayout.BeginHorizontal(BundlesWindow.BodyStyle, GUILayout.ExpandWidth(true), GUILayout.Height(150));
                GUILayout.Label("", GUILayout.Width(10));

                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(10));
                EditorGUILayout.BeginHorizontal();
                Rect Rec = GUILayoutUtility.GetRect(17, 17, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(Rec, _typeIcon == null ? _controller.DownloadImage(Config.IconsPath + Config.MissingFileImageName) : _typeIcon);

                GUILayout.Label("", GUILayout.Width(5));
                GUILayout.Label(_selectedAsset == null ? _selectedBundle.Name.Substring(0, _selectedBundle.Name.LastIndexOf("_")) : _selectedAsset.Name, BundlesWindow.BodyTextStyle);
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(5));
                EditorGUILayout.BeginHorizontal();
                Bundle bundle = _selectedBundle == null ? _controller.GetBundleFromAsset(_selectedAsset) : _selectedBundle;
                string inBuild;
                if(bundle == null)
                {
                    GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    inBuild = "Asset used by Bundle";
                }
                else if(bundle.IsLocal)
                {
                    Rect RecIcon = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(RecIcon, _controller.DownloadImage(Config.IconsPath + Config.InBuildImageName));
                    inBuild = "Bundle In Build";
                }
                else
                {
                    Rect RecIcon = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(RecIcon, _controller.DownloadImage(Config.IconsPath + Config.InServerImageName));
                    inBuild = "Bundle In Server";
                }
                GUILayout.Label("", GUILayout.Width(5));
                GUILayout.Label(inBuild, BundlesWindow.BodyTextStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(5));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(25));
                string size = "";
                if(bundle != null)
                {
                    size = "Bundle Size:  " + BundlesWindow.PrintProperSize(bundle.Size[BundlesWindow.CurrentPlatform]);
                }
                GUILayout.Label(size, BundlesWindow.BodyTextStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[3]), GUILayout.ExpandWidth(false));
                GUILayout.Label("", GUILayout.ExpandHeight(true));
                if(bundle == null)
                {
                    GUILayout.Label("", GUILayout.Height(22), GUILayout.Width(_columnsSize[3]));
                }
                else if(GUILayout.Button("↧ Download", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                {
                    _controller.DownloadBundle(bundle, BundlesWindow.CurrentPlatform);
                }
                if(GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])) && _selectedAsset != null)
                {
                    EditorGUIUtility.PingObject(_selectedAsset.GetAssetObject());
                }
                GUILayout.Label("", GUILayout.Height(10));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Label("", GUILayout.Width(10));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Width(2));
                EditorGUILayout.EndHorizontal();

                if(_selectedAsset != null && _references.Count > 0)
                {
                    GUILayout.Label("", GUILayout.Height(20));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(1));
                    EditorGUILayout.BeginVertical(BundlesWindow.BodyStyle);
                    EditorGUILayout.BeginHorizontal(BundlesWindow.HeaderStyle2);
                    GUILayout.Label("", GUILayout.Width(_columnsSize[0]));
                    GUILayout.Label("Dependency Tree", GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    PrintHierarchy(_selectedAsset, _references);

                    GUILayout.Label("", GUILayout.Height(20));
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("", GUILayout.Width(10));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            void PrintHierarchy(Asset selectedAsset, List<Asset> assets, int margin = 0)
            {
                for(int i = 0; i < assets.Count; i++)
                {
                    bool isChild = IsDependencyOf(selectedAsset, assets[i]) && assets[i].Guid != selectedAsset.Guid;
                    bool isParent = IsDependencyOf(assets[i], selectedAsset);

                    if(isChild || isParent)
                    {
                        PrintAsset(assets[i], selectedAsset, isChild, margin);

                        if(!isChild || _shownHierarchy.Contains(assets[i].Name))
                        {
                            List<Asset> dependencies = GetAssetDependencies(assets[i]);
                            for(int j = 0; j < dependencies.Count; j++)
                            {
                                if(assets.Contains(dependencies[j]))
                                {
                                    dependencies.RemoveAt(j);
                                }
                            }

                            _controller.SortAssets(AssetSortingMode.TypeAsc, dependencies);
                            PrintHierarchy(selectedAsset, dependencies, margin + 20);
                        }
                    }
                }
            }


            void PrintAsset(Asset asset, Asset currentAsset, bool showCollapseButton, int margin = 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(5 + margin));
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(3));
                EditorGUILayout.BeginHorizontal();

                if(showCollapseButton)
                {
                    PrintCollapseButton(asset);
                }
                else
                {
                    GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(15));
                }

                GUILayout.Label(AssetPreview.GetMiniThumbnail(asset.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
                GUIStyle buttonStyle = BundlesWindow.BodyLinkStyle;
                if(asset.Guid == currentAsset.Guid)
                {
                    buttonStyle = BundlesWindow.BodySpecialLinkStyle;
                }
                if(GUILayout.Button(asset.Name, buttonStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                    inspectorDummy.SelectedAsset = asset;
                    inspectorDummy.SelectedBundle = null;
                    Selection.activeObject = inspectorDummy;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(1));
            }


            void PrintCollapseButton(Asset asset, Asset assetToSkip = null)
            {
                int dependencyCount = GetAssetDependencyCount(asset, assetToSkip);
                string collapseSymbol = "▼";
                if(dependencyCount == 0)
                {
                    GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(15));
                }
                else
                {
                    bool collapsed = !_shownHierarchy.Contains(asset.Name);
                    if(collapsed)
                    {
                        collapseSymbol = "►";
                    }
                    if(GUILayout.Button(collapseSymbol, BundlesWindow.NoButtonStyle, GUILayout.Width(15), GUILayout.Height(15)))
                    {
                        if(collapsed)
                        {
                            _shownHierarchy.Add(asset.Name);
                        }
                        else
                        {
                            _shownHierarchy.Remove(asset.Name);
                        }
                    }
                }
            }




            List<Asset> GetAssetDependencies(Asset asset)
            {
                if(_controller.DependenciesCache.ContainsKey(asset.FullName))
                {
                    return _controller.DependenciesCache[asset.FullName];
                }

                var dependencies = new List<Asset>();
                string selectedAssetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                string[] dependencesPaths = AssetDatabase.GetDependencies(selectedAssetPath, true);
                for(int i = 0; i < dependencesPaths.Length; i++)
                {
                    if(selectedAssetPath != dependencesPaths[i])
                    {
                        bool isSubDependency = IsDependency(dependencesPaths[i], dependencesPaths, selectedAssetPath);
                        if(!isSubDependency)
                        {
                            dependencies.Add(new Asset(AssetDatabase.AssetPathToGUID(dependencesPaths[i])));
                        }
                    }
                }
                _controller.DependenciesCache.Add(asset.FullName, dependencies);

                return dependencies;
            }

            int GetAssetDependencyCount(Asset asset, Asset assetToSkip = null)
            {
                List<Asset> dependencies = GetAssetDependencies(asset);
                int count = dependencies.Count;
                if(assetToSkip != null)
                {
                    for(int i = 0; i < dependencies.Count; i++)
                    {
                        if(dependencies[i].Guid == assetToSkip.Guid)
                        {
                            count--;
                        }
                    }
                }
                return count;
            }

            static bool IsDependency(string assetPath, string[] assetListPaths, string assetPathToSkip = "")
            {
                bool isDependency = false;
                for(int j = 0; j < assetListPaths.Length; j++)
                {
                    if(assetListPaths[j] != assetPath && assetListPaths[j] != assetPathToSkip)
                    {
                        string[] subDependencesPaths = AssetDatabase.GetDependencies(assetListPaths[j]);
                        if(ArrayUtility.Contains(subDependencesPaths, assetPath))
                        {
                            isDependency = true;
                            break;
                        }
                    }
                }
                return isDependency;
            }

            bool IsDependency(Asset asset, List<Asset> assetList, Asset assetToSkip = null)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                var assetListPaths = new string[assetList.Count];
                for(int i = 0; i < assetList.Count; i++)
                {
                    assetListPaths[i] = AssetDatabase.GUIDToAssetPath(assetList[i].Guid);
                }
                string assetPathToSkip = "";
                if(assetToSkip != null)
                {
                    assetPathToSkip = AssetDatabase.GUIDToAssetPath(assetToSkip.Guid);
                }

                return IsDependency(assetPath, assetListPaths, assetPathToSkip);
            }

            static bool IsDependencyOf(Asset asset, Asset dependency)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                string dependencyPath = AssetDatabase.GUIDToAssetPath(dependency.Guid);

                string[] dependencesPaths = AssetDatabase.GetDependencies(assetPath, true);

                return ArrayUtility.Contains(dependencesPaths, dependencyPath);
            }



            List<Asset> GetAssetReferences(Asset asset, int searchLimit = 100)
            {
                if(_controller.ReferencesCache.ContainsKey(asset.FullName))
                {
                    return _controller.ReferencesCache[asset.FullName];
                }

                string assetName;
                var matches = new Dictionary<string, Asset>();
                Object assetObject = asset.GetAssetObject();
                if(AssetDatabase.IsMainAsset(assetObject))
                {
                    string path = AssetDatabase.GetAssetPath(assetObject);
                    assetName = Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    ErrorDisplay.DisplayError(ErrorType.assetNotFound, false, false, false, asset.Name, asset.Guid);
                    return null;
                }

                string[] allObjects = GetAllObjectsInBundles();
                foreach(string objectToCheck in allObjects)
                {
                    if(assetName != objectToCheck)
                    {
                        string[] dependencies = AssetDatabase.GetDependencies(objectToCheck);

                        for(int i = 0; i < dependencies.Length; i++)
                        {
                            string dependencyPath = dependencies[i];
                            if(matches.Count == searchLimit)
                            {
                                return new List<Asset>(matches.Values);
                            }
                            if(!string.IsNullOrEmpty(dependencyPath) && Path.GetFileNameWithoutExtension(dependencyPath) == assetName)
                            {
                                Asset parent = _controller.GetAssetFromObject(AssetDatabase.LoadMainAssetAtPath(objectToCheck));
                                if(!matches.ContainsKey(parent.Name))
                                {
                                    matches.Add(parent.Name, parent);
                                }
                            }
                        }
                    }
                }

                List<Asset> references = GetRootParentsOnly(new List<Asset>(matches.Values), asset);
                _controller.ReferencesCache.Add(asset.FullName, GetRootParentsOnly(new List<Asset>(matches.Values), asset));

                return references;
            }

            string[] GetAllObjectsInBundles()
            {
                var filteredArray = new List<string>();
                string[] allObjects = AssetDatabase.GetAllAssetPaths();
                for(int i = 0; i < allObjects.Length; i++)
                {
                    string objectToCheck = allObjects[i];
                    if(_controller.IsBundle(Path.GetFileName(objectToCheck)))
                    {
                        filteredArray.Add(objectToCheck);
                    }
                }
                return filteredArray.ToArray();
            }

            bool IsAssetShared(Asset asset)
            {
                if(_controller.SharedDependenciesCache.ContainsKey(asset.Name))
                {
                    return _controller.SharedDependenciesCache[asset.Name];
                }

                bool shared = GetAssetReferences(asset, 2).Count > 1;
                _controller.SharedDependenciesCache.Add(asset.Name, shared);
                return shared;
            }

            List<Asset> GetRootParentsOnly(List<Asset> originalList, Asset assetToSkip = null)
            {
                var matchesFiltered = new List<Asset>();
                for(int i = 0; i < originalList.Count; i++)
                {
                    if(!IsDependency(originalList[i], originalList, assetToSkip))
                    {
                        matchesFiltered.Add(originalList[i]);
                    }
                }
                return matchesFiltered;
            }
        }
    }
}
