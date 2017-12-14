using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("TriCountValidator", typeof(ModelImporter))]
public class TriCountValidator : AssetBundleGraph.IValidator
{

    [SerializeField]
    public int maxTriangleCount;
    
    private int triangleCount;
    private List<string> offendingMeshes = new List<string>();


    // Tells the validator if this object should be validated or is an exception.	
    public bool ShouldValidate(object asset)
    {
        var target = (GameObject)asset;
        return target.GetComponentInChildren<MeshFilter>() != null || target.GetComponentInChildren<SkinnedMeshRenderer>() != null;
    }


    // Validate things. 
    public bool Validate(object asset)
    {
        var target = (GameObject)asset;
        offendingMeshes.Clear();
        
        foreach(SkinnedMeshRenderer skinnedMesh in target.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            triangleCount = skinnedMesh.sharedMesh.triangles.Length / 3;
            var exceedsMaximum = triangleCount > maxTriangleCount;

            if(exceedsMaximum)
            {
                offendingMeshes.Add(skinnedMesh.name);
            }
        }

        foreach(MeshFilter skinnedMesh in target.GetComponentsInChildren<MeshFilter>())
        {
            triangleCount = skinnedMesh.sharedMesh.triangles.Length / 3;
            var exceedsMaximum = triangleCount > maxTriangleCount;

            if(exceedsMaximum)
            {
                offendingMeshes.Add(skinnedMesh.name);
            }
        }

        return offendingMeshes.Count == 0;
    }


    //When the validation fails you can try to recover in here and return if it is recovered
    public bool TryToRecover(object asset)
    {
        return false;
    }


    // When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
    public string ValidationFailed(object asset)
    {
        var target = (GameObject)asset;

        var message = "[<color=yellow>";

        for(int i = 0; i < offendingMeshes.Count; i++)
        {
            if(i > 0)
            {
                message += ", ";
            }

            message += offendingMeshes[i];
        }

        message += "</color>]";

        return "The meshes " + message + " of " + AssetDatabase.GetAssetPath(target) + " exceeds the maximum triangle threshold of " + maxTriangleCount;
    }


    // Draw inspector gui 
    public void OnInspectorGUI(Action onValueChanged)
    {
        GUILayout.Label("Model Triangles Count Validator", EditorStyles.largeLabel);
        EditorGUILayout.Space();

        var newValue = EditorGUILayout.IntField("Max Triangles Count", maxTriangleCount);
        if(newValue != maxTriangleCount)
        {
            maxTriangleCount = newValue;
            onValueChanged();
        }
    }

    // serialize this class to JSON 
    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
