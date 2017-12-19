using UnityEditor;
using UnityEngine;
using SocialPoint.GUIControl;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SPText))]
    public class SPTextEditor : UnityEditor.UI.TextEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUI.color = new Color(0f, 0.8f, 0f);
            EditorGUILayout.LabelField("Localization info", EditorStyles.boldLabel);
            GUI.color = Color.white;

            SerializedProperty keyValue = serializedObject.FindProperty("Key");
            SerializedProperty paramsValue = serializedObject.FindProperty("Parameters");
            SerializedProperty effectValue = serializedObject.FindProperty("Effect");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(keyValue, true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(paramsValue, true);
            EditorGUILayout.PropertyField(effectValue);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}