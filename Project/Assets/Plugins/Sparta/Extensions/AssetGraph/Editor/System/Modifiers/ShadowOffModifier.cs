using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomModifier("ShadowOffModifier", typeof(GameObject))]
public class ShadowOffModifierGO : ShadowOffModifier { }
[AssetBundleGraph.CustomModifier("ShadowOffModifier", typeof(ModelImporter))]
public class ShadowOffModifier : AssetBundleGraph.IModifier
{

    // Test if asset is different from intended configuration 
    public bool IsModified(object asset)
    {
        var target = (GameObject)asset;
        return target.GetComponentInChildren<MeshFilter>() != null || target.GetComponentInChildren<SkinnedMeshRenderer>() != null;
    }

    // Actually change asset configurations. 
    public void Modify(object asset)
    {
        var target = (GameObject)asset;

        foreach(SkinnedMeshRenderer skinnedMesh in target.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            DisableShadows(skinnedMesh);
        }

        foreach(MeshRenderer skinnedMesh in target.GetComponentsInChildren<MeshRenderer>())
        {
            DisableShadows(skinnedMesh);
        }
    }

    private void DisableShadows(Renderer renderer)
    {
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

#if UNITY_5_5_OR_NEWER
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#else
        renderer.useLightProbes = false;
#endif
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    // Draw inspector gui 
    public void OnInspectorGUI(Action onValueChanged)
    {
        EditorGUILayout.HelpBox("This modifier disables shadows in the model GameObject", MessageType.Info);
    }

    // serialize this class to JSON 
    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
