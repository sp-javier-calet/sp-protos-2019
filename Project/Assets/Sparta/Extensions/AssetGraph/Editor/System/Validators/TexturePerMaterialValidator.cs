using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("TexturePerMaterial", typeof(Material))]
public class TexturePerMaterialValidator : AssetBundleGraph.IValidator {

	[SerializeField] private int maxTexturePerMaterial;
	
	// Tells the validator if this object should be validated or is an exception.	
	public bool ShouldValidate(object asset) {
        Debug.LogError("ShouldValidate object: "+ ((UnityEngine.Object)asset).name);
        var target = (GameObject)asset;
        return target != null;
	}


	// Validate things. 
	public bool Validate (object asset) {
        var mat = (Material)asset;
        var count = ShaderUtil.GetPropertyCount(mat.shader);
        int numTextures = 0;
        Debug.LogError("Validate");
        for(var i = 0; i < count; ++i)
        {
            Debug.LogError("Validate - in for");
            if(ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                numTextures++;
            }
        }

        return numTextures <= maxTexturePerMaterial;
	}

	
	//When the validation fails you can try to recover in here and return if it is recovered
	public bool TryToRecover(object asset) {
        Debug.LogError("TryToRecover");
		return false;
	}

	
	// When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
	public string ValidationFailed(object asset) {
        Debug.LogError("ValidationFailed");
		return ((UnityEngine.Object)asset).name + " invalid";
	}


	// Draw inspector gui 
	public void OnInspectorGUI (Action onValueChanged) {
		GUILayout.Label("Maximum textures per Material");

        var newValue = EditorGUILayout.IntField("Max Textures Count", maxTexturePerMaterial);
        if(newValue != maxTexturePerMaterial) {
            maxTexturePerMaterial = newValue;
			onValueChanged();
		}
	}

	// serialize this class to JSON 
	public string Serialize() {
        Debug.LogError("Serialize");
		return JsonUtility.ToJson(this);
	}
}
