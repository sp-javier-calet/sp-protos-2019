using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace SocialPoint.Utils
{
    public class ReorderableArrayProperty
    {
        ReorderableList _list;
        SerializedProperty _prop;

        public ReorderableArrayProperty(SerializedObject obj, string name, string desc=null)
        {
            _prop = obj.FindProperty(name);
            _list = new ReorderableList(obj, _prop, true, true, true, true);

            _list.drawHeaderCallback += rect =>
            {                
                GUI.Label(rect, new GUIContent(name, desc));
            };
            _list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.width -= 40;
                rect.x += 20;
                EditorGUI.PropertyField(rect, _prop.GetArrayElementAtIndex(index), GUIContent.none, true);
            };
        }

        public void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            _list.DoLayoutList();
            GUI.enabled = true;
        }
    }
}
