
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using SocialPoint.IO;
using SocialPoint.Exporter;
using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Animations
{
    public class AnimatorExporter : BaseExporter
    {
        public UnityEngine.GameObject Prefab;
        public string ExportName = "Animator";

        public string DummyName = null;

        public override void Export(IFileManager files, Log.ILogger log)
        {
            var animator = Prefab.GetComponentInChildren<Animator>();
            var animatorCtrl = animator.runtimeAnimatorController as AnimatorController;
            var animatorData = animatorCtrl.ToStandalone();
            using(var fh = files.Write(ExportName))
            {
                fh.Writer.Write(animatorData);
            }

            if(!string.IsNullOrEmpty(DummyName))
            {
                var animatorDummy = AnimatorController.CreateAnimatorControllerAtPath(DummyName);
                var animatorDataUnity = animatorData.ToUnity();
                animatorDummy.parameters = animatorDataUnity.parameters;
                animatorDummy.layers = animatorDataUnity.layers;
                CopyAnimatorControllerState(animatorCtrl, animatorDummy);
            }
        }

        public override string ToString()
        {
            return "Animator exporter " + (Prefab != null ? ("[" + Prefab.name + "]") : "");
        }

        static void CopyAnimatorControllerState(AnimatorController original, AnimatorController created)
        {
            for(int i = 0; i < original.layers.Length && i < created.layers.Length; i++)
            {
                var originalLayer = original.layers[i];
                var createdLayer = created.layers[i];

                createdLayer.stateMachine.entryPosition = originalLayer.stateMachine.entryPosition;
                createdLayer.stateMachine.exitPosition = originalLayer.stateMachine.exitPosition;
                createdLayer.stateMachine.anyStatePosition = originalLayer.stateMachine.anyStatePosition;

                var positionedStates = createdLayer.stateMachine.states;
                for(int j = 0; j < positionedStates.Length; j++)
                {
                    var state = positionedStates[j].state;
                    var originalChildState = UnityEditorAnimatorExtensions.FindChildState(originalLayer.stateMachine, state.name);
                    positionedStates[j] = new ChildAnimatorState {
                        state = state,
                        position = originalChildState.position,
                    };
                }
                createdLayer.stateMachine.states = positionedStates;
            }
        }
    }
}
