using SocialPoint.GUIControl;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SPTooltipTrigger))]
    public class SPTooltipTriggerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GameObject.FindObjectOfType<CanvasGroup>();

            SerializedProperty scriptValue = serializedObject.FindProperty("m_Script");
            SerializedProperty prefabValue = serializedObject.FindProperty("Prefab");
            SerializedProperty pressTypeValue = serializedObject.FindProperty("PressType");
            SerializedProperty spikePositionValue = serializedObject.FindProperty("SpikePosition");
            SerializedProperty holdTimeValue = serializedObject.FindProperty("HoldTime");
            SerializedProperty offsetValue = serializedObject.FindProperty("Offset");
            SerializedProperty timeToCloseValue = serializedObject.FindProperty("TimeToClose");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptValue, true);
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(prefabValue, true);
            EditorGUILayout.PropertyField(offsetValue, true);
            EditorGUILayout.PropertyField(spikePositionValue, true);
            EditorGUILayout.PropertyField(timeToCloseValue, true);

            EditorGUILayout.PropertyField(pressTypeValue, true);

            if(pressTypeValue.enumValueIndex == (int)SPTooltipTrigger.TriggerType.Hold)
            {
                EditorGUILayout.PropertyField(holdTimeValue, true);
            }
                
            serializedObject.ApplyModifiedProperties();
        }
    }
}