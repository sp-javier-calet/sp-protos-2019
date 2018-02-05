using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(InstantiatePlayableAsset))]
    public class InstantiatePlayableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var playableTarget = (InstantiatePlayableAsset)target;
            var template = playableTarget.Template;

            var scriptReference = serializedObject.FindProperty("m_Script");
            var prefabExposedRefence = serializedObject.FindProperty("Prefab");
            var parentExposedRefence = serializedObject.FindProperty("Parent");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptReference, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prefabExposedRefence, new GUIContent("Prefab:"), true);
            template.PrefabIs3DObject = EditorGUILayout.Toggle("Prefab Is 3D:", template.PrefabIs3DObject);

            EditorGUILayout.PropertyField(parentExposedRefence, new GUIContent("Parent:"), true);
            template.UsePooling = EditorGUILayout.Toggle("Use Pooling:", template.UsePooling);

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
