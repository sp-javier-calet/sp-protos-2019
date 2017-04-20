using UnityEngine;
using UnityEditor;
using System.IO;
using AssetBundleGraph;
using System.Collections.Generic;
using System.Linq;
using System;

[InitializeOnLoad]
public static class FolderPainter
{
    static Texture2D texture;
    static Texture2D texture2;
    static Texture2D textureMini;
    static Texture2D texture2Mini;
    static List<int> markedObjects;
    static bool perfectMatch = false;

    static FolderPainter()
    {
        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_FOLDER_TEX);
        texture2 = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_INHERITED_FOLDER_TEX);
        textureMini = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_FOLDER_TEX_MINI);
        texture2Mini = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_INHERITED_FOLDER_TEX_MINI);
        EditorApplication.projectWindowItemOnGUI += HierarchyItemCB;
    }

    static void HierarchyItemCB(string guid, Rect r)
    {
        if(IsFolderUserByAssetGraph(AssetDatabase.GUIDToAssetPath(guid), out perfectMatch))
        {
            Texture2D drawTexture = null;
            Rect drawRect = new Rect(r);
            if(Mathf.Approximately(r.height, 16f))
            {
                drawRect.x += 2;
                drawTexture = perfectMatch ? textureMini : texture2Mini;
                drawRect.width = drawRect.height;
            }
            else
            {
                drawTexture = perfectMatch ? texture : texture2;
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

    private static bool IsFolderUserByAssetGraph(string path, out bool isPerfectMatch)
    {
        bool isUsed = false;
        isPerfectMatch = false;

        if(Path.GetExtension(path) == string.Empty)
        {
            if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
            {
                return false;
            }
            LoaderSaveData loaderSaveData = LoaderSaveData.LoadFromDisk();
            var loader = loaderSaveData.GetBestLoaderData(path);

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

    private string path = null;
    private LoaderSaveData.LoaderData loader = null;

    private bool IsValid
    {
        get
        {

            bool shouldPaintInspector = false;
            var currentPath = AssetDatabase.GetAssetPath(target);
            if(Directory.Exists(currentPath))
            {
                if(currentPath != path)
                {
                    path = currentPath;
                    CheckForLoader();
                }

                shouldPaintInspector = !(path + "/").Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH);
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
        loader = loaderSaveData.GetBestLoaderData(path);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(IsValid)
        {
            GUI.enabled = true;
            bool perfectMatch = false;

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

                perfectMatch = folderConfigured == path;

                string message = perfectMatch ? "This folder is configured to use the Graph Importer" : "This folder is inheriting a Graph Importer configuration from " + folderConfigured;
                string buttonMsg = perfectMatch ? "Open Folder Graph" : "Open Inherited graph from " + folderConfigured;

                EditorGUILayout.HelpBox(message, MessageType.Info);
                EditorGUILayout.Space();


                if(GUILayout.Button(buttonMsg))
                {
                    AssetBundleGraphEditorWindow.SelectAllRelatedTree(new string[] { loader.id });
                }

                if(GUILayout.Button("Run this Subgraph"))
                {
                    var nodeIDs = new List<string>();
                    nodeIDs.Add(loader.id);
                    AssetBundleGraphEditorWindow.OpenAndRunSelected(new string[] { loader.id });
                }

            }

            if(!perfectMatch)
            {
                if(GUILayout.Button("Setup Graph Loader for this folder"))
                {
                    AssetBundleGraphEditorWindow.OpenAndCreateLoader(path);
                    CheckForLoader();
                }
            }
        }
    }
}
