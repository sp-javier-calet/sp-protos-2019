using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    [CustomEditor(typeof(InspectorDummy))]
    public class Inspector : UnityEditor.Editor
    {
        private Asset _selectedAsset;
        private EditorClientController _controller;
        private InspectorDummy _dummy;
        private float[] _columnsSize;
        private List<Asset> _dependencies;
        private Dictionary<string, bool> _sharedDependenciesCache;
        private List<Asset> _references;
        private Texture2D _preview;

        void OnEnable()
        {
            if (target != null)
            {
                _dummy = (InspectorDummy)target;
                _selectedAsset = _dummy.SelectedAsset;
            }

            if (_selectedAsset != null)
            {
                _controller = EditorClientController.GetInstance();
                _columnsSize = new float[] { 20f, 50f, 50f, 100f };
                _dependencies = new List<Asset>();
                string[] dependencesGuids = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(_selectedAsset.Guid));
                for (int i = 0; i < dependencesGuids.Length; i++)
                {
                    _dependencies.Add(new Asset(AssetDatabase.AssetPathToGUID(dependencesGuids[i])));
                }
                _controller.SortAssets(AssetSortingMode.TypeAsc, _dependencies);
                _sharedDependenciesCache = new Dictionary<string, bool>();

                _references = new List<Asset>(SearchReferences(_selectedAsset));
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
            }
        }

        protected override void OnHeaderGUI(){ }

        public override void OnInspectorGUI()
        {
            if(_selectedAsset != null)
            {
                BundlesWindow.InitStyles();

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
                float size = 0;
                Bundle bundle = _controller.GetBundleFromAsset(_selectedAsset);
                if (bundle != null)
                    size = bundle.Size;
                GUILayout.Label("size:  "+ size+" MB", BundlesWindow.BodyTextStyle);
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

                
                if (_dependencies.Count > 1)
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

                    for (int i = 0; i < _dependencies.Count; i++)
                        PrintDependency(_dependencies[i]);

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

                    for (int i = 0; i < _references.Count; i++)
                        PrintReference(_references[i]);

                    GUILayout.Label("", GUILayout.Height(20));
                    EditorGUILayout.EndVertical();
                    GUILayout.Label("", GUILayout.Width(10));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void PrintDependency(Asset dependency)
        {
            if (dependency.Guid == _selectedAsset.Guid)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            EditorGUILayout.BeginHorizontal();
            Bundle bundle = _controller.GetBundleFromAsset(dependency);
            GUILayout.Label(AssetPreview.GetMiniThumbnail(dependency.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
            if (GUILayout.Button(dependency.Name, BundlesWindow.BodyLinkStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                inspectorDummy.SelectedAsset = dependency;
                Selection.activeObject = inspectorDummy;
            }
            string bundleSize = "0";
            if (bundle != null)
                bundleSize = bundle.Size.ToString();
            GUILayout.Label(bundleSize+" MB", BundlesWindow.BodyTextStyle, GUILayout.Width(_columnsSize[1]));
            if (IsAssetShared(dependency))
            {
                if (GUILayout.Button(_controller.DownloadImage(Config.IconsPath + "shared.png"), BundlesWindow.BodyLinkStyle, GUILayout.Width(_columnsSize[2]), GUILayout.Height(20)))
                {
                    InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                    inspectorDummy.SelectedAsset = dependency;
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

            GUILayout.Label("", GUILayout.Height(5));
        }

        private void PrintReference(Asset reference)
        {
            if (reference.Guid == _selectedAsset.Guid)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(AssetPreview.GetMiniThumbnail(reference.GetAssetObject()), GUILayout.Width(_columnsSize[0]), GUILayout.Height(_columnsSize[0]));
            if (GUILayout.Button(reference.Name, BundlesWindow.BodyLinkStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                InspectorDummy inspectorDummy = ScriptableObject.CreateInstance<InspectorDummy>();
                inspectorDummy.SelectedAsset = reference;
                Selection.activeObject = inspectorDummy;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Find Asset", GUILayout.Height(22), GUILayout.Width(_columnsSize[3])))
                EditorGUIUtility.PingObject(reference.GetAssetObject());

            GUILayout.Label("", GUILayout.Width(5));
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(5));
        }

        private bool IsAssetShared(Asset asset)
        {
            if (_sharedDependenciesCache.ContainsKey(asset.Name))
                return _sharedDependenciesCache[asset.Name];

            bool shared = SearchReferences(asset, 2).Length > 1;
            _sharedDependenciesCache.Add(asset.Name, shared);
            return shared;
        }

        private Asset[] SearchReferences(Asset asset, int searchLimit = 100)
        {
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

            Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(Object));
            foreach (Object objectToCheck in allObjects)
            {
                if (_controller.IsBundle(objectToCheck) && assetName != objectToCheck.name)
                {
                    Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { objectToCheck });
                    for (int i= 0; i < dependencies.Length; i++)
                    {
                        Object dependency = dependencies[i];
                        if (matches.Count == searchLimit)
                            return new List<Asset>(matches.Values).ToArray();
                        else if (dependency != null && dependency.name == assetName)
                        {
                            Asset parent = _controller.GetAssetFromObject(objectToCheck);
                            if (!matches.ContainsKey(parent.Name))
                                matches.Add(parent.Name, parent);
                        }
                    }
                }
            }

            return new List<Asset>(matches.Values).ToArray();
        }
    }
}
