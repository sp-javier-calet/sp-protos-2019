using UnityEngine;
using UnityEditor;
using SocialPoint.Multiplayer;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SocialPoint.Multiplayer
{
    [CustomEditor(typeof(UnityDebugSceneMonoBehaviour))]
    public class UnityDebugSceneMonoBehaviourEditor : UnityBaseDebugEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var debug = (UnityDebugSceneMonoBehaviour) target;
            if(debug == null)
            {
                return;
            }
            if(debug.ClientScene != null )
            {
                EditorGUILayout.LabelField("Client", _titleStyle);
                EditorGUILayout.LabelField("Behaviours", _subtitleStyle);
                BehavioursInpectorGUI(debug.ClientScene);
            }
                
            if(debug.ServerScene != null )
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Server", _titleStyle);
                EditorGUILayout.LabelField("Behaviours", _subtitleStyle);
                BehavioursInpectorGUI(debug.ServerScene);
            }
        }
    }
}
