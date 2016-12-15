using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomModifier("ShadowOffModifier", typeof(ModelImporter))]
public class ShadowOffModifier : AssetBundleGraph.IModifier {

	private bool isSkinned;
	// Test if asset is different from intended configuration 
	public bool IsModified(object asset) {
		var target = (GameObject)asset;

		var staticMesh = target.GetComponentInChildren<MeshRenderer>();

		isSkinned = staticMesh == null;

		return !isSkinned || target.GetComponentInChildren<SkinnedMeshRenderer>() != null;
	}

	// Actually change asset configurations. 
	public void Modify(object asset) {
		var target = (GameObject)asset;

		if(isSkinned) {
			var skinnedMeshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach(SkinnedMeshRenderer skinnedMesh in skinnedMeshes) {
				DisableShadows(skinnedMesh);
			}
		}else {
			var skinnedMeshes = target.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer skinnedMesh in skinnedMeshes) {
				DisableShadows(skinnedMesh);
			}
		}
	}

	private void DisableShadows(Renderer renderer) {
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.useLightProbes = false;
		renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
	}

	// Draw inspector gui 
	public void OnInspectorGUI(Action onValueChanged) {
		EditorGUILayout.HelpBox("This modifier disables shadows in the model GameObject", MessageType.Info);
	}

	// serialize this class to JSON 
	public string Serialize() {
		return JsonUtility.ToJson(this);
	}
}
