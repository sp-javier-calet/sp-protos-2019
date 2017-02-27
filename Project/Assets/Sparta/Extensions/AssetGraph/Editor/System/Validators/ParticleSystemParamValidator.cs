using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

[AssetBundleGraph.CustomValidator("ParticleSystemParamValidator", typeof(GameObject))]
public class ParticleSystemParamValidator : AssetBundleGraph.IValidator
{

    [SerializeField]
    private int emissionRate;
    [SerializeField]
    private bool autoclamp;

    private List<ParticleSystem> offendingParticleSystems = new List<ParticleSystem>();

    // Tells the validator if this object should be validated or is an exception.	
    public bool ShouldValidate(object asset)
    {
        return ((GameObject)asset).GetComponentsInChildren<ParticleSystem>().Length > 0;
    }


    // Validate things. 
    public bool Validate(object asset)
    {
        var particleSystems = ((GameObject)asset).GetComponentsInChildren<ParticleSystem>();

        foreach(var particle in particleSystems)
        {
            if(particle.emission.rate.constantMax > (float)emissionRate)
            {
                offendingParticleSystems.Add(particle);
            }
        }

        return offendingParticleSystems.Count == 0;
    }


    //When the validation fails you can try to recover in here and return if it is recovered
    public bool TryToRecover(object asset)
    {
        if(autoclamp)
        {
            for(int i = 0; i < offendingParticleSystems.Count; i++)
            {
                var emission = offendingParticleSystems[i].emission;
                var rate = emission.rate;
                rate.constantMax = emissionRate;
                emission.rate = rate;
                Debug.Log("ParticleSystem " + offendingParticleSystems[i] + " emission rate was auto clamped to " + emissionRate);
            }

            return true;
        }

        return false;
    }


    // When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
    public string ValidationFailed(object asset)
    {
        var target = (GameObject)asset;

        var message = "[<color=yellow>";

        for(int i = 0; i < offendingParticleSystems.Count; i++)
        {
            if(i > 0)
            {
                message += ", ";
            }

            message += offendingParticleSystems[i].name;
        }

        message += "</color>]";

        return "The particle systems " + message + " of " + AssetDatabase.GetAssetPath(target) + " exceeds the maximum emission rate of " + emissionRate;
    }


    // Draw inspector gui 
    public void OnInspectorGUI(Action onValueChanged)
    {
        GUILayout.Label("ParticleSystemParamValidator");

        var newValue = EditorGUILayout.IntField("Emission Rate", emissionRate);
        var newAutoclamp = EditorGUILayout.Toggle("Auto Clamp Max Value", autoclamp);
        if(newValue != emissionRate)
        {
            emissionRate = newValue;
            onValueChanged();
        }
        if(newAutoclamp != autoclamp)
        {
            autoclamp = newAutoclamp;
            onValueChanged();
        }
    }

    // serialize this class to JSON 
    public string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
}
