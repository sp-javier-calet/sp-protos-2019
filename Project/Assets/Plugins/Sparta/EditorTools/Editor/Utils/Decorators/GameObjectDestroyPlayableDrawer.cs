using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

//namespace SpartaTools.Editor.Utils.Decorators
//{
//    [CustomEditor(typeof(DestroyPlayableAsset))]
//    public class DestroyGameObjectPlayableInspector : UnityEditor.Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//            var scriptReference = serializedObject.FindProperty("m_Script");
//            var gameObjectExposedRefence = serializedObject.FindProperty("GameObject");

//            GUI.enabled = false;
//            EditorGUILayout.PropertyField(scriptReference, true);
//            GUI.enabled = true;

//            EditorGUILayout.Space();
//            EditorGUILayout.Space();

//            EditorGUI.BeginChangeCheck();
//            EditorGUILayout.PropertyField(gameObjectExposedRefence, new GUIContent("GameObject:"), true);
//            if(EditorGUI.EndChangeCheck())
//            {
//                serializedObject.ApplyModifiedProperties();
//            }
//        }
//    }
//}
