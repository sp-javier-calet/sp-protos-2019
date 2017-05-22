using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    [CustomValidator("ParticleSystemParamValidator", typeof(GameObject))]
    public class ParticleSystemParamValidator : IValidator
    {

        [SerializeField]
        private bool checkEmissionRateOverTime;
        [SerializeField]
        private bool checkEmissionRateOverDistance;
        [SerializeField]
        private int emissionRateOverTime;
        [SerializeField]
        private int emissionRateOverDistance;
        [SerializeField]
        private bool autoclampOverTime;
        [SerializeField]
        private bool autoclampOverDistance;

        private List<ParticleSystem> offendingParticleSystems;

        // Tells the validator if this object should be validated or is an exception.	
        public bool ShouldValidate(object asset)
        {
            return ((GameObject)asset).GetComponentsInChildren<ParticleSystem>().Length > 0;
        }


        // Validate things. 
        public bool Validate(object asset)
        {
            offendingParticleSystems = new List<ParticleSystem>();
            var particleSystems = ((GameObject)asset).GetComponentsInChildren<ParticleSystem>();

            foreach(var particle in particleSystems)
            {
                if(checkEmissionRateOverTime && particle.emission.rateOverTime.constantMax > (float)emissionRateOverTime)
                {
                    offendingParticleSystems.Add(particle);
                }
                if(checkEmissionRateOverDistance && particle.emission.rateOverDistance.constantMax > (float)emissionRateOverDistance)
                {
                    offendingParticleSystems.Add(particle);
                }
            }

            return offendingParticleSystems.Count == 0;
        }


        //When the validation fails you can try to recover in here and return if it is recovered
        public bool TryToRecover(object asset)
        {
            if(autoclampOverDistance || autoclampOverTime)
            {
                for(int i = 0; i < offendingParticleSystems.Count; i++)
                {
                    var emission = offendingParticleSystems[i].emission;

                    if(autoclampOverTime && emission.rateOverTime.constant > emissionRateOverTime)
                    {
                        var rateOverTime = emission.rateOverTime;
                        rateOverTime.constant = emissionRateOverTime;
                        emission.rateOverTime = rateOverTime;
                        Debug.Log("ParticleSystem " + offendingParticleSystems[i] + " emission rate over time was auto clamped to " + emissionRateOverTime);
                    }

                    if(autoclampOverDistance && emission.rateOverDistance.constant > emissionRateOverDistance)
                    {
                        var rateOverDistance = emission.rateOverDistance;
                        rateOverDistance.constant = emissionRateOverDistance;
                        emission.rateOverDistance = rateOverDistance;
                        Debug.Log("ParticleSystem " + offendingParticleSystems[i] + " emission rate over distance was auto clamped to " + emissionRateOverDistance);
                    }
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


            var msg = "The particle systems " + message + " of " + AssetDatabase.GetAssetPath(target) + " exceeds ";

            if(checkEmissionRateOverTime)
            {
                msg += "the emission rateOverTime of " + emissionRateOverTime;
            }
            if(checkEmissionRateOverDistance)
            {
                if(checkEmissionRateOverTime)
                {
                    msg += " or ";
                }
                msg += "the emission rateOverDistance of " + emissionRateOverDistance;
            }

            return msg;
        }


        // Draw inspector gui 
        public void OnInspectorGUI(Action onValueChanged)
        {
            GUILayout.Label("Particle System Emission Validator");

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var newCOT = EditorGUILayout.Toggle("Check Emission Over Time", checkEmissionRateOverTime);
                if(newCOT != checkEmissionRateOverTime)
                {
                    checkEmissionRateOverTime = newCOT;
                    onValueChanged();
                }
                if(newCOT)
                {
                    var newOTValue = EditorGUILayout.IntField("Emission Rate Over Time", emissionRateOverTime);
                    if(newOTValue != emissionRateOverTime)
                    {
                        emissionRateOverTime = newOTValue;
                        onValueChanged();
                    }

                    var newAutoclampOT = EditorGUILayout.Toggle("Auto Clamp Max Value", autoclampOverTime);
                    if(newAutoclampOT != autoclampOverTime)
                    {
                        autoclampOverTime = newAutoclampOT;
                        onValueChanged();
                    }
                }
            }

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var newCOD = EditorGUILayout.Toggle("Check Emission Over Distance", checkEmissionRateOverDistance);
                if(newCOD != checkEmissionRateOverDistance)
                {
                    checkEmissionRateOverDistance = newCOD;
                    onValueChanged();
                }

                if(newCOD)
                {
                    var newODValue = EditorGUILayout.IntField("Emission Rate Over Distance", emissionRateOverDistance);
                    if(newODValue != emissionRateOverDistance)
                    {
                        emissionRateOverDistance = newODValue;
                        onValueChanged();
                    }

                    var newAutoclampOD = EditorGUILayout.Toggle("Auto Clamp Max Value", autoclampOverDistance);
                    if(newAutoclampOD != autoclampOverDistance)
                    {
                        autoclampOverDistance = newAutoclampOD;
                        onValueChanged();
                    }
                }
            }
        }

        // serialize this class to JSON 
        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
