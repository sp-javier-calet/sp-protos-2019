using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    [InitializeOnLoad]
    public static class FolderPainter
    {
        static Texture2D _folder;
        static Texture2D _inherited_folder;
        static Texture2D _folder_mini;
        static Texture2D _inherited_folder_mini;
        static List<int> markedObjects;
        static bool _perfectMatch = false;
        static LoaderSaveData _loaderData = null;
        public static bool reloadLoaders = true;


        static FolderPainter()
        {
            _folder = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_FOLDER_TEX);
            _inherited_folder = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_INHERITED_FOLDER_TEX);
            _folder_mini = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_FOLDER_TEX_MINI);
            _inherited_folder_mini = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_INHERITED_FOLDER_TEX_MINI);
            EditorApplication.projectWindowItemOnGUI += HierarchyItemCB;
            LoaderSaveData.OnSave += () => reloadLoaders = true;
        }

        static void HierarchyItemCB(string guid, Rect r)
        {
            if(string.IsNullOrEmpty(guid))
                return;
            try
            {
                if(IsFolderUserByAssetGraph(AssetDatabase.GUIDToAssetPath(guid), out _perfectMatch))
                {
                    Texture2D drawTexture = null;
                    Rect drawRect = new Rect(r);
                    if(Mathf.Approximately(r.height, 16f))
                    {
                        drawRect.x += 2;
                        drawTexture = _perfectMatch ? _folder_mini : _inherited_folder_mini;
                        drawRect.width = drawRect.height;
                    }
                    else
                    {
                        drawTexture = _perfectMatch ? _folder : _inherited_folder;
                        drawRect.height = r.height * 0.8f;

                        var diffheight = drawRect.height - drawTexture.height;
                        var diffwidth = drawRect.width - drawTexture.width;

                        if(diffheight > 0)
                        {
                            drawRect.y += diffheight * 0.75f;
                            drawRect.height = drawTexture.height;
                        }
                        if(diffwidth > 0)
                        {
                            drawRect.x += diffwidth / 2;
                            drawRect.width = drawTexture.width;
                        }
                    }

                    if(drawTexture != null)
                    {
                        GUI.DrawTexture(drawRect, drawTexture);
                    }
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
            }
        }

        private static bool IsFolderUserByAssetGraph(string path, out bool isPerfectMatch)
        {
            bool isUsed = false;
            isPerfectMatch = false;

            if(string.IsNullOrEmpty(path))
                return isUsed;

            if(Path.GetExtension(path) == string.Empty)
            {
                if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
                {
                    return false;
                }
                if(reloadLoaders)
                {
                    _loaderData = LoaderSaveData.LoadFromDisk();
                    reloadLoaders = false;
                }

                var loader = _loaderData.GetBestLoaderData(path);

                if(loader != null)
                {
                    var folderConfigured = loader.paths.CurrentPlatformValue;
                    if(folderConfigured == string.Empty)
                    {
                        folderConfigured = "Global";
                    }
                    else
                    {
                        folderConfigured = "Assets/" + folderConfigured;
                    }

                    isPerfectMatch = folderConfigured == path;
                    isUsed = true;
                }

            }
            return isUsed;
        }
    }

    [CustomEditor(typeof(DefaultAsset))]
    public class FolderInspector : Editor
    {
        private string _path = null;
        private LoaderSaveData.LoaderData _loader = null;

        private bool IsValid
        {
            get
            {

                bool shouldPaintInspector = false;
                var currentPath = AssetDatabase.GetAssetPath(target);
                if(Directory.Exists(currentPath))
                {
                    if(currentPath != _path)
                    {
                        _path = currentPath;
                        CheckForLoader();
                    }

                    shouldPaintInspector = !(_path + "/").Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH);
                }
                return shouldPaintInspector;
            }
        }

        private void CheckForLoader()
        {
            if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
            {
                return;
            }
            LoaderSaveData loaderSaveData = LoaderSaveData.LoadFromDisk();
            _loader = loaderSaveData.GetBestLoaderData(_path);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(IsValid)
            {
                GUI.enabled = true;
                bool perfectMatch = false;

                if(_loader != null)
                {
                    var folderConfigured = _loader.paths.CurrentPlatformValue;
                    if(folderConfigured == string.Empty)
                    {
                        folderConfigured = "Global";
                    }
                    else
                    {
                        folderConfigured = "Assets/" + folderConfigured;
                    }

                    perfectMatch = folderConfigured == _path;

                    string message = perfectMatch ? "This folder is configured to use the AssetGraph Importer" : "This folder is inheriting an AssetGraph Importer configuration from " + folderConfigured;
                    string buttonMsg = perfectMatch ? "Open Folder Graph" : "Open Inherited graph from " + folderConfigured;

                    EditorGUILayout.HelpBox(message, MessageType.Info);
                    EditorGUILayout.Space();


                    if(GUILayout.Button(buttonMsg))
                    {
                        AssetBundleGraphEditorWindow.SelectAllRelatedTree(new string[] { _loader.id });
                    }

                    if(GUILayout.Button("Run this Subgraph"))
                    {
                        var nodeIDs = new List<string>();
                        nodeIDs.Add(_loader.id);
                        AssetBundleGraphEditorWindow.OpenAndRunSelected(new string[] { _loader.id });
                    }

                }

                if(!perfectMatch)
                {
                    if(GUILayout.Button("Setup Graph Loader for this folder"))
                    {
                        AssetBundleGraphEditorWindow.OpenAndCreateLoader(_path);
                        CheckForLoader();
                    }
                }
            }
        }
    }
}
