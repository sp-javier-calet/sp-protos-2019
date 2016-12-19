﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    [CustomEditor(typeof(InspectorDummy))]
    public class Inspector : UnityEditor.Editor
    {
        private Asset _selectedAsset;
        private InspectorAsset _inspectorAsset;
        private EditorClientController _controller;
        private InspectorDummy _dummy;
        private float[] _columnsSize;
        private Vector2 _scrollPos;

        void OnEnable()
        {
            EditorApplication.update += Update;

            if (target != null)
            {
                _dummy = (InspectorDummy)target;
                _selectedAsset = _dummy.SelectedAsset;
            }

            if (_selectedAsset != null)
            {
                _controller = EditorClientController.GetInstance();
                _columnsSize = new float[] { 20f, 50f, 50f, 100f };
                _inspectorAsset = new InspectorAsset(_selectedAsset, _controller, _columnsSize);
            }

            _scrollPos = Vector2.zero;
        }

        void OnDisable() { EditorApplication.update -= Update; }

        protected override void OnHeaderGUI(){ }

        public override void OnInspectorGUI()
        {
            if(_selectedAsset != null)
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



        private enum InspectorAssetType { Dependency, Reference}




        private class InspectorAsset
        {
            private Asset _selectedAsset;
            private EditorClientController _controller;
            private float[] _columnsSize;
            private List<Asset> _references;
            private Texture2D _preview;
            private Texture2D _typeIcon;
            private List<string> _shownHierarchy;

            public InspectorAsset(Asset selectedAsset, EditorClientController controller, float[] columnsSize)
            {
                _selectedAsset = selectedAsset;
                _controller = controller;
                _columnsSize = columnsSize;

                _references = GetAssetReferences(_selectedAsset);
                if(_references.Count == 0)
                    _references.Add(selectedAsset);
                _controller.SortAssets(AssetSortingMode.TypeAsc, _references);

                Object assetObject = _selectedAsset.GetAssetObject();
                _typeIcon = AssetPreview.GetMiniThumbnail(assetObject);
                _preview = AssetPreview.GetAssetPreview(assetObject);
                for (int counter = 0; _preview == null && counter < 10; counter++)
                {
                    _preview = AssetPreview.GetAssetPreview(assetObject);
                    System.Threading.Thread.Sleep(20);
                }
                if (_preview == null)
                    _preview = _typeIcon;

                _shownHierarchy = new List<string>();
            }


            public void PrintAssetView()
            {
                EditorGUILayout.BeginVertical();

                GUILayout.Label("", GUILayout.Height(9));

                EditorGUILayout.BeginHorizontal();

                Rect previewRect = GUILayoutUtility.GetRect(170, 170, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(previewRect, _preview);

                EditorGUILayout.BeginHorizontal(BundlesWindow.BodyStyle, GUILayout.ExpandWidth(true), GUILayout.Height(150));
                GUILayout.Label("", GUILayout.Width(10));
                
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(10));
                EditorGUILayout.BeginHorizontal();
                Rect Rec = GUILayoutUtility.GetRect(17, 17, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(Rec, _typeIcon);
                GUILayout.Label("", GUILayout.Width(5));
                GUILayout.Label(_selectedAsset.Name, BundlesWindow.BodyTextStyle);
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(5));
                EditorGUILayout.BeginHorizontal();
                Bundle bundle = _controller.GetBundleFromAsset(_selectedAsset);
                string inBuild = "";
                if (bundle == null)
                {
                    /*Rect RecIcon = */GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    inBuild = "Asset used by Bundle";
                }
                else if (bundle.IsLocal)
                {
                    Rect RecIcon = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(RecIcon, _controller.DownloadImage(Config.IconsPath + "in_build.png"));
                    inBuild = "Bundle In Build";
                }
                else
                {
                    Rect RecIcon = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(RecIcon, _controller.DownloadImage(Config.IconsPath + "in_server.png"));
                    inBuild = "Bundle In Server";
                }
                GUILayout.Label("", GUILayout.Width(5));
                GUILayout.Label(inBuild, BundlesWindow.BodyTextStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(5));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(25));
                string size = "";
                if (bundle != null)
                    size = "Bundle Size:  " + bundle.Size + " MB";
                GUILayout.Label(size, BundlesWindow.BodyTextStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                EditorGUILayout.BeginVertical(GUILayout.Width(_columnsSize[3]), GUILayout.ExpandWidth(false));
                GUILayout.Label("", GUILayout.ExpandHeight(true));
                if (bundle == null)
                    GUILayout.Label("", GUILayout.Height(22), GUILayout.Width(_columnsSize[3]));
                else if (GUILayout.Button("↧ Download", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    _controller.InstanciateBundle(bundle);
                if (GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    EditorGUIUtility.PingObject(_selectedAsset.GetAssetObject());
                GUILayout.Label("", GUILayout.Height(10));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                GUILayout.Label("", GUILayout.Width(10));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Width(2));
                EditorGUILayout.EndHorizontal();

                if (_references.Count > 0)
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


            private void PrintHierarchy(Asset selectedAsset, List<Asset> assets, int margin = 0)
            {
                for (int i = 0; i < assets.Count; i++)
                {
                    bool isChild = IsDependencyOf(selectedAsset, assets[i]) && assets[i].Guid != selectedAsset.Guid;
                    bool isParent = IsDependencyOf(assets[i], selectedAsset);

                    if (isChild || isParent)
                    {
                        PrintAsset(assets[i], selectedAsset, isChild, margin);

                        if (!isChild || _shownHierarchy.Contains(assets[i].Name))
                        {
                            List<Asset> dependencies = GetAssetDependencies(assets[i]);
                            _controller.SortAssets(AssetSortingMode.TypeAsc, dependencies);
                            PrintHierarchy(selectedAsset, dependencies, margin + 20);
                        }
                    }
                }
            }
            

            private void PrintAsset(Asset asset, Asset currentAsset, bool showCollapseButton, int margin = 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(5 + margin));
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(3));
                EditorGUILayout.BeginHorizontal();

                if(showCollapseButton)
                    PrintCollapseButton(asset);
                else
                    GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(15));

                GUILayout.Label(AssetPreview.GetMiniThumbnail(asset.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
                GUIStyle buttonStyle = BundlesWindow.BodyLinkStyle;
                if (asset.Guid == currentAsset.Guid)
                    buttonStyle = BundlesWindow.BodySpecialLinkStyle;
                if (GUILayout.Button(asset.Name, buttonStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                    inspectorDummy.SelectedAsset = asset;
                    Selection.activeObject = inspectorDummy;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(1));
            }
            

            private void PrintCollapseButton(Asset asset, Asset assetToSkip = null)
            {
                int dependencyCount = GetAssetDependencyCount(asset, assetToSkip);
                string collapseSymbol = "▼";
                if (dependencyCount == 0)
                    GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(15));
                else
                {
                    bool collapsed = !_shownHierarchy.Contains(asset.Name);
                    if (collapsed)
                        collapseSymbol = "►";
                    if (GUILayout.Button(collapseSymbol, BundlesWindow.NoButtonStyle, GUILayout.Width(15), GUILayout.Height(15)))
                    {
                        if (collapsed)
                            _shownHierarchy.Add(asset.Name);
                        else
                            _shownHierarchy.Remove(asset.Name);
                    }
                }
            }




            private List<Asset> GetAssetDependencies(Asset asset)
            {
                if (_controller.DependenciesCache.ContainsKey(asset.Name))
                    return _controller.DependenciesCache[asset.Name];

                List<Asset> dependencies = new List<Asset>();
                string selectedAssetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                string[] dependencesPaths = AssetDatabase.GetDependencies(selectedAssetPath, true);
                for (int i = 0; i < dependencesPaths.Length; i++)
                {
                    if (selectedAssetPath != dependencesPaths[i])
                    {
                        bool isSubDependency = IsDependency(dependencesPaths[i], dependencesPaths, selectedAssetPath);
                        if (!isSubDependency)
                            dependencies.Add(new Asset(AssetDatabase.AssetPathToGUID(dependencesPaths[i])));
                    }
                }
                _controller.DependenciesCache.Add(asset.Name, dependencies);

                return dependencies;
            }

            private int GetAssetDependencyCount(Asset asset, Asset assetToSkip = null)
            {
                List<Asset> dependencies = GetAssetDependencies(asset);
                int count = dependencies.Count;
                if (assetToSkip != null)
                {
                    for (int i = 0; i < dependencies.Count; i++)
                    {
                        if (dependencies[i].Guid == assetToSkip.Guid)
                            count--;
                    }
                }
                return count;
            }

            private bool IsDependency(string assetPath, string[] assetListPaths, string assetPathToSkip = "")
            {
                bool isDependency = false;
                for (int j = 0; j < assetListPaths.Length; j++)
                {
                    if (assetListPaths[j] != assetPath && assetListPaths[j] != assetPathToSkip)
                    {
                        string[] subDependencesPaths = AssetDatabase.GetDependencies(assetListPaths[j]);
                        if (ArrayUtility.Contains(subDependencesPaths, assetPath))
                            isDependency = true;
                    }
                }
                return isDependency;
            }

            private bool IsDependency(Asset asset, List<Asset> assetList, Asset assetToSkip = null)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                string[] assetListPaths = new string[assetList.Count];
                for (int i = 0; i < assetList.Count; i++)
                    assetListPaths[i] = AssetDatabase.GUIDToAssetPath(assetList[i].Guid);
                string assetPathToSkip = "";
                if (assetToSkip != null)
                    assetPathToSkip = AssetDatabase.GUIDToAssetPath(assetToSkip.Guid);

                return IsDependency(assetPath, assetListPaths, assetPathToSkip);
            }

            private bool IsDependencyOf(Asset asset, Asset dependency)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset.Guid);
                string dependencyPath = AssetDatabase.GUIDToAssetPath(dependency.Guid);

                string[] dependencesPaths = AssetDatabase.GetDependencies(assetPath, true);

                return ArrayUtility.Contains(dependencesPaths,dependencyPath);
            }



            private List<Asset> GetAssetReferences(Asset asset, int searchLimit = 100)
            {
                if (_controller.ReferencesCache.ContainsKey(asset.Name))
                    return _controller.ReferencesCache[asset.Name];

                string assetName = "";
                Dictionary<string, Asset> matches = new Dictionary<string, Asset>();
                Object assetObject = asset.GetAssetObject();
                if (AssetDatabase.IsMainAsset(assetObject))
                {
                    string path = AssetDatabase.GetAssetPath(assetObject);
                    assetName = Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    Debug.Log("Error Asset not found");
                    return null;
                }

                string[] allObjects = GetAllObjectsInBundles();
                foreach (string objectToCheck in allObjects)
                {
                    if (assetName != objectToCheck)
                    {
                        string[] dependencies = AssetDatabase.GetDependencies(objectToCheck);

                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            string dependencyPath = dependencies[i];
                            if (matches.Count == searchLimit)
                            {
                                return new List<Asset>(matches.Values);
                            }
                            else if (dependencyPath != null && dependencyPath.Length > 0 && Path.GetFileNameWithoutExtension(dependencyPath) == assetName)
                            {
                                Asset parent = _controller.GetAssetFromObject(AssetDatabase.LoadMainAssetAtPath(objectToCheck));
                                if (!matches.ContainsKey(parent.Name))
                                {
                                    matches.Add(parent.Name, parent);
                                }
                            }
                        }
                    }
                }

                List<Asset> references = GetRootParentsOnly(new List<Asset>(matches.Values), asset);
                _controller.ReferencesCache.Add(asset.Name, GetRootParentsOnly(new List<Asset>(matches.Values), asset));

                return references;
            }

            private string[] GetAllObjectsInBundles()
            {
                List<string> filteredArray = new List<string>();
                string[] allObjects = AssetDatabase.GetAllAssetPaths();
                foreach (string objectToCheck in allObjects)
                {
                    if (_controller.IsBundle(Path.GetFileNameWithoutExtension(objectToCheck)))
                        filteredArray.Add(objectToCheck);
                }
                return filteredArray.ToArray();
            }

            private bool IsAssetShared(Asset asset)
            {
                if (_controller.SharedDependenciesCache.ContainsKey(asset.Name))
                    return _controller.SharedDependenciesCache[asset.Name];

                bool shared = GetAssetReferences(asset, 2).Count > 1;
                _controller.SharedDependenciesCache.Add(asset.Name, shared);
                return shared;
            }

            private List<Asset> GetRootParentsOnly(List<Asset> originalList, Asset assetToSkip = null)
            {
                List<Asset> matchesFiltered = new List<Asset>();
                for (int i = 0; i < originalList.Count; i++)
                {
                    if (!IsDependency(originalList[i], originalList, assetToSkip))
                        matchesFiltered.Add(originalList[i]);
                }
                return matchesFiltered;
            }
        }
    }
}
