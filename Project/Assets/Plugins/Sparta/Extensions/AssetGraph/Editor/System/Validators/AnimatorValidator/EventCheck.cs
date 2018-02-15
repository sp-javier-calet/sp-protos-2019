using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace AssetBundleGraph
{
    [Serializable]
    public class EventCheck
    {
        public int optionalGroup;
        public string functionName;
        public string stringValue;

        [NonSerialized]
        public bool showEvent = false;

        public void OnInspectorGUI(Action onValueChanged, Action OnRemoved)
        {

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    var foldoutText = string.IsNullOrEmpty(stringValue) ? "Event without value" : stringValue;
                    showEvent = EditorGUILayout.Foldout(showEvent, foldoutText, true, AnimatorValidator.foldoutStyle);

                    if(GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        OnRemoved();
                        onValueChanged();
                    }
                }

                if(showEvent)
                {
                    var newOptional = EditorGUILayout.IntField("Optional Group", optionalGroup);

                    if(newOptional != optionalGroup)
                    {
                        optionalGroup = newOptional;
                        onValueChanged();
                    }

                    var newFunction = EditorGUILayout.TextField("Function Name", functionName);

                    if(newFunction != functionName)
                    {
                        functionName = newFunction;
                        onValueChanged();
                    }
                    var newValue = EditorGUILayout.TextField("String value", stringValue);

                    if(newValue != stringValue)
                    {
                        stringValue = newValue;
                        onValueChanged();
                    }
                }
            }
        }
    }

}
