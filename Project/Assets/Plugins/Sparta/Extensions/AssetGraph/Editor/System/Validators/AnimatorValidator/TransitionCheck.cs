using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AssetBundleGraph
{
    [Serializable]
    public class TransitionCheck
    {
        [SerializeField]
        public string state = "";
        [SerializeField]
        public List<ConditionCheck> conditions = new List<ConditionCheck>();

        [NonSerialized]
        public bool showTransition = false;
        [NonSerialized]
        public bool showConditions = false;

        public void OnInspectorGUI(Action onValueChanged, Action OnRemoved)
        {
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    var foldoutText = string.IsNullOrEmpty(state) ? "Unnamed Transition" : "--> " + state;
                    showTransition = EditorGUILayout.Foldout(showTransition, foldoutText, true, AnimatorValidator.foldoutStyle);

                    if(GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        OnRemoved();
                        onValueChanged();
                    }
                }

                if(showTransition)
                {
                    var newState = EditorGUILayout.TextField("State", state);

                    if(newState != state)
                    {
                        state = newState;
                        onValueChanged();
                    }

                    bool showButton = true;
                    if(conditions.Count > 0)
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.foldout);
                        style.margin.left += 10;
                        showConditions = EditorGUILayout.Foldout(showConditions, "Conditions", true, style);

                        if(showConditions)
                        {
                            for(int j = 0; j < conditions.Count; j++)
                            {
                                var condition = conditions[j];

                                condition.OnInspectorGUI(onValueChanged, () =>
                                {
                                    conditions.RemoveAt(j);
                                    j--;
                                    onValueChanged();
                                });
                            }
                        }

                        showButton = showConditions;
                    }

                    if(showButton && GUILayout.Button("Add Condition Check"))
                    {
                        conditions.Add(new ConditionCheck());
                        showConditions = true;
                        onValueChanged();
                    }

                }
            }
        }



        public static void ValidateTransition(string sourceStateName, AnimatorStateTransition animatorTransition, TransitionCheck transitionCheck, ref string errorMsg)
        {
            foreach(var condition in transitionCheck.conditions)
            {
                AnimatorCondition? animatorCondition = null;
                foreach(var animCond in animatorTransition.conditions)
                {
                    if(animCond.parameter == condition.parameter)
                    {
                        animatorCondition = animCond;
                    }
                }
                if(animatorCondition == null)
                {
                    errorMsg += "- \"" + sourceStateName + "\" to \"" + transitionCheck.state + "\" doesn't have the \"" + condition.parameter + "\" condition\n";
                }
                else
                {
                    if(condition.mode != 0)
                    {
                        if(condition.mode != animatorCondition.Value.mode)
                        {
                            errorMsg += "- \"" + sourceStateName + "\" to \"" + transitionCheck.state + "\" doesn't have the \"" + condition.mode.ToString() + "\" mode\n";
                        }

                        if(condition.threshold != animatorCondition.Value.threshold)
                        {
                            errorMsg += "- \"" + sourceStateName + "\" to \"" + transitionCheck.state + "\" doesn't have the \"" + condition.threshold + "\" value\n";
                        }
                    }
                }
            }

        }
    }
}
