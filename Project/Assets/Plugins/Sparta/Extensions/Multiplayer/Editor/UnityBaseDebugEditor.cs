using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SocialPoint.Multiplayer
{
    public class UnityBaseDebugEditor : UnityEditor.Editor
    {
        protected GUIStyle _titleStyle;
        protected GUIStyle _subtitleStyle;
        Dictionary<object, bool> _foldouts;
        List<object> _behaviours = new List<object>(FinderSettings.DefaultListCapacity);

        public override void OnInspectorGUI()
        {
            if(_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label);
                _titleStyle.fontSize = 16;
                _titleStyle.fixedHeight = 36;
                _titleStyle.fontStyle = FontStyle.Bold;
            }
            if(_subtitleStyle == null)
            {
                _subtitleStyle = new GUIStyle(GUI.skin.label);
                _subtitleStyle.fontSize = 12;
                _subtitleStyle.fixedHeight = 22;
                _subtitleStyle.fontStyle = FontStyle.Bold;
            }
            if(_foldouts == null)
            {
                _foldouts = new Dictionary<object, bool>();
            }
        }

        protected void BehavioursInpectorGUI(INetworkBehaviourProvider provider)
        {
            provider.GetBehaviours<object>(_behaviours);
            for(var i = 0; i < _behaviours.Count; i++)
            {
                GenericInpectorGUI(_behaviours[i]);
            }
        }

        const BindingFlags PropBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        protected void GenericInpectorGUI(object behaviour)
        {
            if(!_foldouts.ContainsKey(behaviour))
            {
                _foldouts[behaviour] = true;
            }
            var shown = EditorGUILayout.Foldout(_foldouts[behaviour], behaviour.ToString());
            _foldouts[behaviour] = shown;

            if(shown)
            {
                var type = behaviour.GetType();
                var props = type.GetProperties(PropBindingFlags);
                var fields = type.GetFields(PropBindingFlags);
                for(var i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    GenericPropertyInpectorGUI(
                        prop.Name, prop.GetValue(behaviour, null));
                }
                for(var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    GenericPropertyInpectorGUI(
                        field.Name, field.GetValue(behaviour));
                }
            }
        }

        void GenericPropertyInpectorGUI(string name, object value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name.ToString());
            EditorGUILayout.TextField(value == null ? "null" : value.ToString());
            EditorGUILayout.EndHorizontal();
        }
    }
}
