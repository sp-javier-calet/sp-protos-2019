using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SocialPoint.Utils
{
    public sealed class ReorderableArrayProperty
    {
        ReorderableList _list;

        public ReorderableArrayProperty(SerializedObject obj, string name, string desc = null)
        {
            var prop = obj.FindProperty(name);

            CreateReorderableArray(obj, prop, name, desc);
        }

        public ReorderableArrayProperty(SerializedObject obj, SerializedProperty prop, string name, string desc = null)
        {
            CreateReorderableArray(obj, prop, name, desc);
        }

        void CreateReorderableArray(SerializedObject obj, SerializedProperty prop, string name, string desc = null)
        {
            _list = new ReorderableList(obj, prop, true, true, true, true);

            _list.drawHeaderCallback += rect => GUI.Label(rect, new GUIContent(name, desc));
            _list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.width -= 40;
                rect.x += 20;
                EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
            };
        }

        public void OnInspectorGUI()
        {
            GUI.enabled &= !Application.isPlaying;
            _list.DoLayoutList();
            GUI.enabled = true;
        }
    }
}
