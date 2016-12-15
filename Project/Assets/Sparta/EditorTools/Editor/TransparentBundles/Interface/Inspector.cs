using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    [CustomEditor(typeof(InspectorDummy))]
    public class Inspector : UnityEditor.Editor
    {
        private Asset _selectedAsset, _selectedDependency;
        private InspectorAsset _inspectorAsset, _inspectorDependency;
        private EditorClientController _controller;
        private InspectorDummy _dummy;
        private float[] _columnsSize;
        private Vector2 _scrollPos, _scrollPos2;

        void OnEnable()
        {
            EditorApplication.update += Update;

            if (target != null)
            {
                _dummy = (InspectorDummy)target;
                _selectedAsset = _dummy.SelectedAsset;
                _selectedDependency = _dummy.SelectedDependency;
            }

            if (_selectedAsset != null)
            {
                _controller = EditorClientController.GetInstance();
                _columnsSize = new float[] { 20f, 50f, 50f, 100f };
                _inspectorAsset = new InspectorAsset(_selectedAsset, _controller, _columnsSize);
                if (_selectedDependency != null)
                    _inspectorDependency = new InspectorAsset(_selectedDependency, _controller, _columnsSize);
            }

            _scrollPos = Vector2.zero;
            _scrollPos2 = Vector2.zero;
        }

        void OnDisable() { EditorApplication.update -= Update; }

        protected override void OnHeaderGUI(){ }

        public override void OnInspectorGUI()
        {
            if(_selectedAsset != null)
            {
                BundlesWindow.InitStyles();

                EditorGUILayout.BeginVertical();

                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(Screen.height/2 - 30));
                _inspectorAsset.PrintAssetView();
                EditorGUILayout.EndScrollView();

                if (_selectedDependency != null)
                {
                    GUILayout.Label(" "+_selectedAsset.Name +" > "+ _selectedDependency.Name);

                    _scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2, GUILayout.Height(Screen.height / 2 - 30));
                    _inspectorDependency.PrintAssetView();
                    EditorGUILayout.EndScrollView();
                }
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
            private List<Asset> _dependencies;
            private List<Asset> _references;
            private Texture2D _preview;
            private List<string> _shownHierarchy;

            public InspectorAsset(Asset selectedAsset, EditorClientController controller, float[] columnsSize)
            {
                _selectedAsset = selectedAsset;
                _controller = controller;
                _columnsSize = columnsSize;

                _dependencies = GetAssetDependencies(_selectedAsset);

                _controller.SortAssets(AssetSortingMode.TypeAsc, _dependencies);

                _references = GetAssetReferences(_selectedAsset);
                _controller.SortAssets(AssetSortingMode.TypeAsc, _references);

                Object assetObject = _selectedAsset.GetAssetObject();
                _preview = AssetPreview.GetAssetPreview(assetObject);
                for (int counter = 0; _preview == null && counter < 10; counter++)
                {
                    _preview = AssetPreview.GetAssetPreview(assetObject);
                    System.Threading.Thread.Sleep(20);
                }
                if (_preview == null)
                    _preview = AssetPreview.GetMiniThumbnail(assetObject);

                _shownHierarchy = new List<string>();
            }


            public void PrintAssetView()
            {
                EditorGUILayout.BeginVertical();

                GUILayout.Label("", GUILayout.Height(15));

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(1));

                Rect previewRect = GUILayoutUtility.GetRect(150, 150, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(previewRect, _preview);
                GUILayout.Label("", GUILayout.Width(10));

                EditorGUILayout.BeginHorizontal(BundlesWindow.BodyStyle, GUILayout.ExpandWidth(true), GUILayout.Height(150));
                GUILayout.Label("", GUILayout.Width(10));
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(10));
                GUILayout.Label(_selectedAsset.Name, BundlesWindow.BodyTextStyle);
                string size = "";
                Bundle bundle = _controller.GetBundleFromAsset(_selectedAsset);
                if (bundle != null)
                    size = "size:  " + bundle.Size + " MB";
                GUILayout.Label(size, BundlesWindow.BodyTextStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.ExpandHeight(true));

                EditorGUILayout.BeginHorizontal();
                string inBuild = "";
                if (bundle != null && bundle.IsLocal)
                {
                    Rect Rec = GUILayoutUtility.GetRect(20, 20, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(Rec, _controller.DownloadImage(Config.IconsPath + "in_build.png"));
                    inBuild = "Asset In Build";
                }

                GUILayout.Label(inBuild, BundlesWindow.BodyTextStyle, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true), GUILayout.Height(20));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayout.Width(110));
                GUILayout.Label("", GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    EditorGUIUtility.PingObject(_selectedAsset.GetAssetObject());
                if (bundle == null)
                    GUILayout.Label("", GUILayout.Height(22), GUILayout.Width(_columnsSize[3]));
                else if (GUILayout.Button("↧ Download", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    _controller.InstanciateBundle(bundle);
                GUILayout.Label("", GUILayout.Height(10));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Label("", GUILayout.Width(10));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Width(10));
                EditorGUILayout.EndHorizontal();

                if (_dependencies.Count > 0)
                {
                    GUILayout.Label("", GUILayout.Height(20));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(1));
                    EditorGUILayout.BeginVertical(BundlesWindow.BodyStyle);
                    EditorGUILayout.BeginHorizontal(BundlesWindow.HeaderStyle2);
                    GUILayout.Label("", GUILayout.Width(_columnsSize[0]));
                    GUILayout.Label("Dependencies", GUILayout.ExpandWidth(true));
                    GUILayout.Label("size", GUILayout.Width(_columnsSize[1]));
                    GUILayout.Label("shared", GUILayout.Width(_columnsSize[2]));
                    GUILayout.Label("", GUILayout.Width(_columnsSize[3]));
                    EditorGUILayout.EndHorizontal();

                    PrintHierarchy(_selectedAsset, _dependencies, InspectorAssetType.Dependency);

                    GUILayout.Label("", GUILayout.Height(20));
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("", GUILayout.Width(10));
                    EditorGUILayout.EndHorizontal();
                }

                if (_references.Count > 0)
                {
                    GUILayout.Label("", GUILayout.Height(20));

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(1));
                    EditorGUILayout.BeginVertical(BundlesWindow.BodyStyle);
                    EditorGUILayout.BeginHorizontal(BundlesWindow.HeaderStyle2);
                    GUILayout.Label("", GUILayout.Width(_columnsSize[0]));
                    GUILayout.Label("Used in other Bundles", GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    /*for (int i = 0; i < _references.Count; i++)
                        PrintReference(_references[i], _selectedAsset);*/
                    PrintHierarchy(_selectedAsset, _references, InspectorAssetType.Reference);

                    GUILayout.Label("", GUILayout.Height(20));
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("", GUILayout.Width(10));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }


            private void PrintHierarchy(Asset selectedAsset, List<Asset> assets, InspectorAssetType type, int margin = 0)
            {
                if (assets.Contains(selectedAsset))
                    return;

                for (int i = 0; i < assets.Count; i++)
                {
                    switch (type)
                    {
                        case InspectorAssetType.Dependency:
                            PrintDependency(assets[i], selectedAsset, margin);
                            break;
                        case InspectorAssetType.Reference:
                            PrintReference(assets[i], selectedAsset, margin);
                            break;
                    }
                    
                    if (_shownHierarchy.Contains(assets[i].Name))
                    {
                        List<Asset> dependencies = GetAssetDependencies(assets[i]);
                        PrintHierarchy(selectedAsset, dependencies, type, margin + 15);
                    }
                }
            }



            private void PrintDependency(Asset dependency, Asset parent, int margin = 0)
            {
                if (dependency.Guid == parent.Guid)
                    return;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(5 + margin));
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(3));
                EditorGUILayout.BeginHorizontal();

                PrintCollapseButton(dependency, parent);

                Bundle bundle = _controller.GetBundleFromAsset(dependency);
                GUILayout.Label(AssetPreview.GetMiniThumbnail(dependency.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
                if (GUILayout.Button(dependency.Name, BundlesWindow.BodyLinkStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                    inspectorDummy.SelectedAsset = parent;
                    inspectorDummy.SelectedDependency = dependency;
                    Selection.activeObject = inspectorDummy;
                }
                string bundleSize = "";
                if (bundle != null)
                    bundleSize = bundle.Size.ToString()+ " MB";
                GUILayout.Label(bundleSize, BundlesWindow.BodyTextStyle, GUILayout.Width(_columnsSize[1]));
                if (IsAssetShared(dependency))
                {
                    if (GUILayout.Button(_controller.DownloadImage(Config.IconsPath + "shared.png"), BundlesWindow.BodyLinkStyle, GUILayout.Width(_columnsSize[2]), GUILayout.Height(20)))
                    {
                        InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                        inspectorDummy.SelectedAsset = parent;
                        inspectorDummy.SelectedDependency = dependency;
                        Selection.activeObject = inspectorDummy;
                    }
                }
                else
                    GUILayout.Label("", GUILayout.Width(_columnsSize[2]), GUILayout.Height(20));

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    EditorGUIUtility.PingObject(dependency.GetAssetObject());

                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(1));
            }




            private void PrintReference(Asset reference, Asset child, int margin = 0)
            {
                if (reference.Guid == child.Guid)
                    return;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(5 + margin));
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(3));
                EditorGUILayout.BeginHorizontal();

                PrintCollapseButton(reference, child);

                GUILayout.Label(AssetPreview.GetMiniThumbnail(reference.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
                if (GUILayout.Button(reference.Name, BundlesWindow.BodyLinkStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                    inspectorDummy.SelectedAsset = reference;
                    inspectorDummy.SelectedDependency = child;
                    Selection.activeObject = inspectorDummy;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                    EditorGUIUtility.PingObject(reference.GetAssetObject());

                GUILayout.Label("", GUILayout.Width(5));
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.Height(1));
            }

            private void PrintCollapseButton(Asset asset, Asset assetToSkip = null)
            {
                int dependencyCount = GetAssetDependencyCount(asset, assetToSkip);
                string collapseSymbol = "▼";
                if (dependencyCount == 0)
                    GUILayout.Label("", GUILayout.Width(20), GUILayout.Height(20));
                else
                {
                    bool collapsed = !_shownHierarchy.Contains(asset.Name);
                    if (collapsed)
                        collapseSymbol = "►";
                    if (GUILayout.Button(collapseSymbol, BundlesWindow.NoButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
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
                    assetName = System.IO.Path.GetFileNameWithoutExtension(path);
                }
                else
                {
                    Debug.Log("Error Asset not found");
                    return null;
                }

                Object[] allObjects = GetAllObjectsInBundles();
                foreach (Object objectToCheck in allObjects)
                {
                    if (assetName != objectToCheck.name)
                    {
                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { objectToCheck });
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            Object dependency = dependencies[i];
                            if (matches.Count == searchLimit)
                            {
                                return new List<Asset>(matches.Values);
                            }
                            else if (dependency != null && dependency.name == assetName)
                            {
                                Asset parent = _controller.GetAssetFromObject(objectToCheck);
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

            private Object[] GetAllObjectsInBundles()
            {
                List<Object> filteredArray = new List<Object>();
                Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(Object));
                foreach (Object objectToCheck in allObjects)
                {
                    if (_controller.IsBundle(objectToCheck))
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
