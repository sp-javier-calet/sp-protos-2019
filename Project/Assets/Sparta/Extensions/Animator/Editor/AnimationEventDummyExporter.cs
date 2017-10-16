using System;
using Jitter.LinearMath;
using SocialPoint.Base;
using SocialPoint.Exporter;
using SocialPoint.IO;
using UnityEditor.Animations;
using UnityEngine;
using SocialPoint.Multiplayer;

namespace SocialPoint.Animations
{
    public class AnimationEventDummyExporter : BaseExporter
    {
        [Serializable]
        public struct DummyToExportDefinition
        {
            public string DummyType;
            public string DummyObjectName;
            public string StateNameContains;
            public string EventNameContains;
        }

        public UnityEngine.GameObject Prefab;
        public DummyToExportDefinition[] DummiesToExport;

        public override void Export(IFileManager files, Log.ILogger log)
        {
            string name = Prefab.name;
            var dummyData = GetPrefabDummySet(Prefab);

            string exportName = "Server/DummyOffsets/"+name+"_offsets";
            using(var fh = files.Write(exportName))
            {
                fh.Writer.Write(dummyData);
            }
        }

        PrefabDummySet GetPrefabDummySet(UnityEngine.GameObject prefab)
        {
            var dummySet = new PrefabDummySet();

            for(int i = 0; i < DummiesToExport.Length; ++i)
            {
                var dummyToExport = DummiesToExport[i];
                if(prefab.transform.FindChildRecursive(dummyToExport.DummyObjectName) != null)
                {
                    var animator = prefab.GetComponentInChildren<Animator>();
                    var animatorCtrl = animator.runtimeAnimatorController as AnimatorController;

                    // TODO: Support more than one layer
                    var states = animatorCtrl.layers[0].stateMachine.states;
                    var possibleStateNameMatches = dummyToExport.StateNameContains.Split(',');

                    for(int j = 0; j < states.Length; ++j)
                    {
                        var state = states[j].state;
                        string stateName = state.name;
                        if(string.IsNullOrEmpty(dummyToExport.StateNameContains) || IsMatchingStateName(stateName, possibleStateNameMatches))
                        {
                            float stateEventTime = 0f;
                            bool eventFound = false;

                            // TODO: Support blend trees and other kind of states
                            var clipMotion = state.motion as AnimationClip;
                            if(clipMotion != null)
                            {
                                for(int k = 0; k < clipMotion.events.Length && !eventFound; ++k)
                                {
                                    var clipEvent = clipMotion.events[k];
                                    if(clipEvent.functionName.Contains(dummyToExport.EventNameContains) || clipEvent.stringParameter.Contains(dummyToExport.EventNameContains))
                                    {
                                        eventFound = true;
                                        stateEventTime = clipEvent.time;
                                    }
                                }
                            }

                            if(eventFound)
                            {
                                JVector offset = GetPosedPrefabOffset(prefab, clipMotion, stateEventTime, dummyToExport.DummyObjectName);
                                dummySet.AddDummyOffset(stateName, dummyToExport.DummyType, offset);
                            }
                        }
                    }
                }
            }

            return dummySet;
        }

        bool IsMatchingStateName(string stateName, string[] possibleMatchingNames)
        {
            for(int i = 0; i < possibleMatchingNames.Length; ++i)
            {
                if(stateName.Contains(possibleMatchingNames[i]))
                {
                    return true;
                }
            }
            return false;
        }

        JVector GetPosedPrefabOffset(UnityEngine.GameObject prefab, AnimationClip clip, float stateEventTime, string dummyName)
        {
            var instance = Instantiate(prefab);
            var animator = instance.GetComponentInChildren<Animator>();

            UnityEditor.AnimationMode.BeginSampling();
            UnityEditor.AnimationMode.SampleAnimationClip(animator.gameObject, clip, stateEventTime);
            UnityEditor.AnimationMode.EndSampling();

            UnityEngine.Transform dummy = instance.transform.FindChildRecursive(dummyName);

            // This works because the instance transform axis are aligned with the world axis
            var offset = (dummy.position - instance.transform.position).ToJitter();

            DestroyImmediate(instance);

            return offset;
        }

        public override string ToString()
        {
            return "AnimEvent dummy exporter " + (Prefab != null ? ("[" + Prefab.name + "]") : "");
        }
    }
}