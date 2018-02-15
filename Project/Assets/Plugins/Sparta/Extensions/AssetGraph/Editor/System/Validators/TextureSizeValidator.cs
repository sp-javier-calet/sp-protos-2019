using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("TextureSizeValidator", typeof(TextureImporter))]
public class TextureSizeValidator : AssetBundleGraph.IValidator
{

    [SerializeField]
    private int maxWidth;
    [SerializeField]
    private int maxHeight;


    // Tells the validator if this object should be validated or is an exception.	
    public bool ShouldValidate(object asset)
    {
        return true;
    }


    // Validate things. 
    public bool Validate(object asset)
    {
        var target = (Texture)asset;

        return target.width <= maxWidth && target.height <= maxHeight;
    }


    //When the validation fails you can try to recover in here and return if it is recovered
    public bool TryToRecover(object asset)
    {
        return false;
    }


    // When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
    public string ValidationFailed(object asset)
    {
        var target = (Texture)asset;
        return AssetDatabase.GetAssetPath(target) + " size " + target.width + " x " + target.height + " exceeds max size " + maxWidth + " x " + maxHeight;
    }


    // Draw inspector gui 
    public void OnInspectorGUI(Action onValueChanged)
    {
        GUILayout.Label("Texture Size Validator", EditorStyles.largeLabel);
        EditorGUILayout.Space();

        using(var horizontalScope = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel("Max Texture Size");
            var newWidth = EditorGUILayout.IntField(maxWidth);
            GUILayout.Label(" x ");
            var newHeight = EditorGUILayout.IntField(maxHeight);

            if(newWidth != maxWidth || newHeight != maxHeight)
            {
                maxWidth = newWidth;
                maxHeight = newHeight;
                onValueChanged();
            }
        }
    }

    // serialize this class to JSON 
    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
