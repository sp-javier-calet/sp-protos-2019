using UnityEditor;
using System.Collections;
using UnityEngine;
using AssetBundleGraph;
using System.Collections.Generic;
using System;
using System.IO;

public class PreProcessor : AssetPostprocessor
{
    private static LoaderSaveData loaderSaveData = null;

    protected static LoaderSaveData LoaderData
    {
        get
        {
            if(loaderSaveData == null)
            {
                loaderSaveData = LoaderSaveData.LoadFromDisk();
            }
            return loaderSaveData;
        }
    }

    private static SaveData fullSaveData = null;

    protected static SaveData FullSaveData
    {
        get
        {
            if(fullSaveData == null)
            {
                fullSaveData = SaveData.LoadFromDisk();
            }
            return fullSaveData;
        }
    }

    private static bool hadErrors = false;
    private static bool wasMoving = false;
    public static bool isPreProcessing = false;

    public static HashSet<string> AssetsToPostProcess = new HashSet<string>();

    public static void MarkForReload()
    {
        fullSaveData = null;
        loaderSaveData = null;
    }

    void OnPreprocessTexture()
    {
        if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
        {
            return;
        }

        if(!wasMoving)
        {
            GenericProcess(assetPath);
        }
    }

    void OnPreprocessModel()
    {
        if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
        {
            return;
        }

        if(!wasMoving)
        {
            GenericProcess(assetPath);
        }

        if(assetPath.Contains(AssetGraphRelativePaths.ASSET_PLACEHOLDER_FOLDER))
        {
            ((ModelImporter)assetImporter).importMaterials = false;
        }
    }

    void OnPreprocessAudio()
    {
        if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
        {
            return;
        }

        if(!wasMoving)
        {
            GenericProcess(assetPath);
        }
    }

    // TODO: PostProcess assets in a single run 
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths)
    {
        if(!LoaderSaveData.IsLoaderDataAvailableAtDisk())
        {
            return;
        }

        float i = 0;
        bool clearBar = false;

        foreach(string path in AssetsToPostProcess)
        {
            EditorUtility.DisplayProgressBar("Processing", path, i / (float)AssetsToPostProcess.Count);
            GenericProcess(path, true);
            i++;
            clearBar = true;
        }

        i = 0;
        foreach(string path in imported)
        {
            EditorUtility.DisplayProgressBar("Processing", path, i / (float)moved.Length);
            if(!TypeUtility.IgnoredExtension.Contains(Path.GetExtension(path)))
            {
                GenericProcess(path, true, true);
            }
            i++;
            clearBar = true;
        }


        i = 0;
        foreach(string path in moved)
        {
            EditorUtility.DisplayProgressBar("Processing", path, i / (float)moved.Length);
            if(!TypeUtility.IgnoredExtension.Contains(Path.GetExtension(path)))
            {
                GenericProcess(path, true, true);
                wasMoving = true;
            }
            i++;
            clearBar = true;
        }

        if(clearBar)
        {
            EditorUtility.ClearProgressBar();
        }

        AssetsToPostProcess.Clear();
    }


    static void GenericProcess(string path, bool isPostProcessing = false, bool isMoving = false)
    {
        var loader = LoaderData.GetBestLoaderData(path);

        bool execute = false;

        if(loader != null)
        {
            if(loader.isPermanent)
            {
                execute = true;
            }
            else if(loader.isPreProcess)
            {
                if(!isMoving && isPostProcessing)
                {
                    execute = true;
                }
                else
                {
                    // if it is first import
                    execute = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null;
                }
            }
        }

        if(execute)
        {
            try
            {
                isPreProcessing = !isPostProcessing;
                var loaderNodeData = FullSaveData.Graph.Nodes.Find(x => x.Id == loader.id);
                var graph = FullSaveData.Graph.GetSubGraph(new NodeData[] { loaderNodeData });

                var currentCount = 0.00f;
                var totalCount = graph.Nodes.Count * 1f;
                Action<NodeData, float> updateHandler = (node, progress) =>
                {
                    var progressPercentage = ((currentCount / totalCount) * 100).ToString();
                    if(progressPercentage.Contains(".")) progressPercentage = progressPercentage.Split('.')[0];

                    if(0 < progress)
                    {
                        currentCount = currentCount + 1f;
                    }

                    EditorUtility.DisplayProgressBar("AssetBundleGraph Processing... ", "Processing " + node.Name + ": " + progressPercentage + "%", currentCount / totalCount);
                };
                List<NodeException> errors = new List<NodeException>();
                Action<NodeException> errorHandler = (NodeException e) =>
                {
                    errors.Add(e);
                };

                var target = EditorUserBuildSettings.activeBuildTarget;

                Dictionary<ConnectionData, Dictionary<string, List<Asset>>> streamMap = null;

                if(hadErrors || !isPostProcessing)
                {
                    // perform setup. Fails if any exception raises.
                    streamMap = AssetBundleGraphController.Perform(graph, target, false, errorHandler, null);
                }

                // if there is not error reported, then run
                if(errors.Count == 0)
                {
                    // run datas.                
                    streamMap = AssetBundleGraphController.Perform(graph, target, true, errorHandler, updateHandler, path, false, true);
                }

                if(errors.Count > 0)
                {
                    Debug.LogError(errors[0]);
                    hadErrors = true;
                }
                else
                {
                    hadErrors = false;
                }

                AssetBundleGraphController.Postprocess(graph, streamMap, true);
            }
            catch(Exception e)
            {
                hadErrors = true;
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isPreProcessing = false;
                wasMoving = false;
            }
        }
    }
}
