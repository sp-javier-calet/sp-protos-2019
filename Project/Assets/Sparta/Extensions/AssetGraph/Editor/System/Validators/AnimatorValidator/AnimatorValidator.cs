using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;

namespace AssetBundleGraph
{
    [CustomValidator("Animator States", typeof(Animator))]
    public class AnimatorValidator : IValidator
    {
        private string errorMsg;
        private bool showAnyStateTransitions;

        public static GUIStyle removeButtonStyle;
        public static GUIStyle foldoutStyle;

        [SerializeField]
        public List<TransitionCheck> anyStateTransitions = new List<TransitionCheck>();

        [SerializeField]
        private List<StateCheck> checkList;


        private Vector2 scrollPos;

        // Tells the validator if this object should be validated or is an exception.	
        public bool ShouldValidate(object asset)
        {
            errorMsg = "";
            return true;
        }


        // Validate things. 
        public bool Validate(object asset)
        {
            var animController = (AnimatorController)asset;

            var baseLayerStateMachine = animController.layers[0].stateMachine;

            foreach(var anyStateTransition in anyStateTransitions)
            {
                AnimatorStateTransition animatorTransition = null;

                foreach(var animatorAnyStateTran in baseLayerStateMachine.anyStateTransitions)
                {
                    if(animatorAnyStateTran.destinationState.name == anyStateTransition.state)
                    {
                        animatorTransition = animatorAnyStateTran;
                    }
                }

                if(animatorTransition == null)
                {
                    errorMsg += "- AnyState to \"" + anyStateTransition.state + "\" transition not found";
                }
                else
                {
                    TransitionCheck.ValidateTransition("AnyState", animatorTransition, anyStateTransition, ref errorMsg);
                }
            }

            foreach(var statecheck in checkList)
            {
                AnimatorState animatorState = null;

                for(int i = 0; i < baseLayerStateMachine.states.Length; i++)
                {
                    if(baseLayerStateMachine.states[i].state.name == statecheck.stateName)
                    {
                        animatorState = baseLayerStateMachine.states[i].state;
                    }
                }

                if(animatorState == null)
                {
                    errorMsg += "- " + statecheck.stateName + " State not found\n";
                }
                else
                {
                    if(!string.IsNullOrEmpty(statecheck.speedParam))
                    {
                        if(!animatorState.speedParameterActive || animatorState.speedParameter != statecheck.speedParam)
                        {
                            errorMsg += "- \"" + statecheck.stateName + "\" hasn't \"" + statecheck.speedParam + "\" speed param active\n";
                        }
                    }

                    if(statecheck.outputs.Count > 0)
                    {
                        foreach(var transition in statecheck.outputs)
                        {
                            AnimatorStateTransition animatorTransition = null;
                            foreach(var stateTransition in animatorState.transitions)
                            {
                                if(stateTransition.destinationState.name == transition.state)
                                {
                                    animatorTransition = stateTransition;
                                }
                            }
                            if(animatorTransition == null)
                            {
                                errorMsg += "- \"" + statecheck.stateName + "\" to \"" + transition.state + "\" transition not found\n";
                            }
                            else
                            {
                                TransitionCheck.ValidateTransition(statecheck.stateName, animatorTransition, transition, ref errorMsg);
                            }
                        }
                    }


                    if(statecheck.events.Count > 0)
                    {
                        var clip = animController.animationClips.FirstOrDefault(x => x.name == animatorState.motion.name);

                        if(clip == null)
                        {
                            errorMsg += "- \"" + animatorState.name + "\" has no animation clip";
                        }
                        else
                        {
                            statecheck.events.Sort((x, y) => x.optionalGroup.CompareTo(y.optionalGroup));

                            List<int> groupPass = new List<int>();
                            string groupMsg = "";
                            int lastGroup = 0;

                            foreach(var eventCheck in statecheck.events)
                            {
                                if(lastGroup == 0 || (lastGroup != eventCheck.optionalGroup && !groupPass.Contains(lastGroup)))
                                {
                                    errorMsg += groupMsg;
                                    groupMsg = "";
                                }

                                if(eventCheck.optionalGroup == 0 || !groupPass.Contains(eventCheck.optionalGroup))
                                {
                                    AnimationEvent animationEvent = null;
                                    foreach(var animEvent in clip.events)
                                    {
                                        if(animEvent.functionName == eventCheck.functionName && animEvent.stringParameter == eventCheck.stringValue)
                                        {
                                            animationEvent = animEvent;
                                        }
                                    }

                                    if(animationEvent == null)
                                    {
                                        groupMsg += "- \"" + animatorState.name + "\" clip doesn't have \"" + eventCheck.functionName + "\" with \"" + eventCheck.stringValue + "\"\n";
                                    }
                                    else
                                    {
                                        groupPass.Add(eventCheck.optionalGroup);
                                    }
                                }
                                lastGroup = eventCheck.optionalGroup;
                            }

                            if(!groupPass.Contains(lastGroup))
                            {
                                errorMsg += groupMsg;
                            }
                        }
                    }
                }
            }
            return string.IsNullOrEmpty(errorMsg);
        }




        //When the validation fails you can try to recover in here and return if it is recovered
        public bool TryToRecover(object asset)
        {
            return false;
        }


        // When validation is failed and unrecoverable you may perform your own operations here but a message needs to be returned to be printed.
        public string ValidationFailed(object asset)
        {
            return ((UnityEngine.Object)asset).name + " had the following errors:\n\n" + errorMsg;
        }

        // Draw inspector gui 
        public void OnInspectorGUI(Action onValueChanged)
        {
            removeButtonStyle = new GUIStyle(EditorStyles.miniButton);
            removeButtonStyle.normal.textColor = Color.red;
            foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.margin.left += 10;

            GUILayout.Label("Animator State Validator");
            EditorGUILayout.Space();

            using(var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.ExpandHeight(false)))
            {
                scrollPos = scrollScope.scrollPosition;

                bool showButton = true;
                if(anyStateTransitions.Count > 0)
                {
                    GUIStyle style = new GUIStyle(EditorStyles.foldout);
                    style.margin.left += 10;
                    showAnyStateTransitions = EditorGUILayout.Foldout(showAnyStateTransitions, "AnyState Transitions", true, style);

                    if(showAnyStateTransitions)
                    {
                        for(int j = 0; j < anyStateTransitions.Count; j++)
                        {
                            var transition = anyStateTransitions[j];

                            transition.OnInspectorGUI(onValueChanged, () =>
                            {
                                anyStateTransitions.RemoveAt(j);
                                j--;
                                onValueChanged();
                            });
                        }
                    }

                    showButton = showAnyStateTransitions;
                }
                if(showButton && GUILayout.Button("Add AnyState Transition Check"))
                {
                    anyStateTransitions.Add(new TransitionCheck());
                    anyStateTransitions[anyStateTransitions.Count - 1].showTransition = true;
                    showAnyStateTransitions = true;
                    onValueChanged();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("States:");

                for(int i = 0; i < checkList.Count; i++)
                {
                    var check = checkList[i];
                    check.OnInspectorGUI(onValueChanged, () =>
                    {
                        checkList.RemoveAt(i);
                        i--;
                    });
                    if(check.showState)
                    {
                        EditorGUILayout.Space();
                    }
                }
            }
            if(GUILayout.Button("+"))
            {
                checkList.Add(new StateCheck());
                onValueChanged();
            }
        }

        // serialize this class to JSON 
        public string Serialize()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
