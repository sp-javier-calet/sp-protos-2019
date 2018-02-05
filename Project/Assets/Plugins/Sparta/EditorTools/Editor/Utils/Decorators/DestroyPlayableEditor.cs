using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(DestroyPlayableAsset))]
    public class DestroyPlayableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var playableTarget = (DestroyPlayableAsset)target;
            var template = playableTarget.Template;

            var scriptReference = serializedObject.FindProperty("m_Script");
            var gameObjectExposedRefence = serializedObject.FindProperty("GameObject");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptReference, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(gameObjectExposedRefence, new GUIContent("GameObject:"), true);
            template.UsePooling = EditorGUILayout.Toggle("Use Pooling:", template.UsePooling);

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
