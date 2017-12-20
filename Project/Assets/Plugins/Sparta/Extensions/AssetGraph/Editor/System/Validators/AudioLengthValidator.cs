using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("AudioLengthValidator", typeof(AudioImporter))]
public class AudioLengthValidator : AssetBundleGraph.IValidator
{

    [SerializeField]
    private int maxLength;


    // Tells the validator if this object should be validated or is an exception.	
    public bool ShouldValidate(object asset)
    {
        return true;
    }


    // Validate things. 
    public bool Validate(object asset)
    {
        var target = (AudioClip)asset;
        return target.length <= maxLength;
    }


    //When the validation fails you can try to recover in here and return if it is recovered
    public bool TryToRecover(object asset)
    {
        return false;
    }


    // When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
    public string ValidationFailed(object asset)
    {
        var target = (AudioClip)asset;
        return AssetDatabase.GetAssetPath(target) + " length " + maxLength + " exceeds maximum length " + maxLength;
    }


    // Draw inspector gui 
    public void OnInspectorGUI(Action onValueChanged)
    {
        GUILayout.Label("Audio Length Validator", EditorStyles.largeLabel);
        EditorGUILayout.Space();

        var newValue = EditorGUILayout.IntField("Max audio length", maxLength);
        if(newValue != maxLength)
        {
            maxLength = newValue;
            onValueChanged();
        }
    }

    // serialize this class to JSON 
    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
