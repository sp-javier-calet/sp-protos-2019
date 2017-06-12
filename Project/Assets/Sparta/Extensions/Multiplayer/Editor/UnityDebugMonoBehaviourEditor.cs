using UnityEngine;
using UnityEditor;
using SocialPoint.Multiplayer;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SocialPoint.Multiplayer
{    
    [CustomEditor(typeof(UnityDebugMonoBehaviour))]
    public class UnityDebugMonoBehaviourEditor : UnityBaseDebugEditor
    {
        UnityEditor.Editor _clientTransformEditor;
        UnityEditor.Editor _serverTransformEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var debug = (UnityDebugMonoBehaviour) target;
            if(debug == null)
            {
                return;
            }

            var t = debug.ClientTransform;
            if(t != null && _clientTransformEditor == null)
            {
                _clientTransformEditor = UnityEditor.Editor.CreateEditor(t);
            }
            t = debug.ServerTransform;
            if(t != null && _serverTransformEditor == null)
            {
                _serverTransformEditor = UnityEditor.Editor.CreateEditor(t);
            }

            EditorGUILayout.LabelField("Client", _titleStyle);

            if(_clientTransformEditor != null)
            {
                EditorGUILayout.LabelField("Transform", _subtitleStyle);
                _clientTransformEditor.OnInspectorGUI();
            }

            if(debug.ClientObject != null )
            {
                EditorGUILayout.LabelField("Behaviours", _subtitleStyle);
                BehavioursInpectorGUI(debug.ClientObject);
            }

            if(debug.ServerTransform != null)
            {
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Server", _titleStyle);

                if(_serverTransformEditor != null)
                {
                    EditorGUILayout.LabelField("Transform", _subtitleStyle);
                    _serverTransformEditor.OnInspectorGUI();
                }

                if(debug.ServerObject != null )
                {
                    EditorGUILayout.LabelField("Behaviours", _subtitleStyle);
                    BehavioursInpectorGUI(debug.ServerObject);
                }
            }
        }            

        void OnSceneGUI()
        {
            var behaviour = (UnityDebugMonoBehaviour) target;
            if(behaviour == null)
            {
                return;
            }
            Handles.Label(behaviour.gameObject.transform.position, behaviour.gameObject.name);
        }

        void OnDestroy()
        {
            DestroyImmediate(_serverTransformEditor);
            DestroyImmediate(_clientTransformEditor);
        }
    }
}
