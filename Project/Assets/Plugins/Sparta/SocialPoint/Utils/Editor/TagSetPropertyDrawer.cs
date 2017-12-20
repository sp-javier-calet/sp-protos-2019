using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public abstract class TagSetPropertyDrawer : PropertyDrawer
    {
        List<string> _tags;

        const float LineSeparation = 5.0f;

        protected abstract IEnumerable<string> GetAllTags();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tags = property.FindPropertyRelative("_tags");

            if(_tags == null)
            {
                _tags = new List<string>(tags.arraySize);
                for(var i=0; i<tags.arraySize; i++)
                {
                    var tag = tags.GetArrayElementAtIndex(i).stringValue;
                    _tags.Add(tag);
                }
            }

            var notUsedTags = new List<string>(GetAllTags());
            notUsedTags.RemoveAll(t => _tags.Contains(t));
            notUsedTags.Insert(0, "");

            var w = position.size.x;
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            for(var i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                EditorGUI.LabelField(position, tag);
                var buttonPosition = position;
                buttonPosition.size = new Vector2(30.0f, EditorGUIUtility.singleLineHeight);
                buttonPosition.x = w - buttonPosition.size.x;
                if(GUI.Button(buttonPosition, "-"))
                {
                    notUsedTags.Add(tag);
                }

                position.y += EditorGUIUtility.singleLineHeight + LineSeparation;
            }
            for(var i = 0; i < notUsedTags.Count; i++)
            {
                _tags.Remove(notUsedTags[i]);
            }

            EditorGUIUtility.labelWidth = 40.0f;
            var idx = EditorGUI.Popup(position, "Add", 0, notUsedTags.ToArray());
            if(idx != 0)
            {
                _tags.Add(notUsedTags[idx]);
                notUsedTags.RemoveAt(idx);
            }

            tags.ClearArray();
            tags.arraySize = _tags.Count;
            for(var i = 0; i < _tags.Count; i++)
            {
                tags.GetArrayElementAtIndex(i).stringValue = _tags[i];
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var tagsCount = _tags == null ? 0 : _tags.Count;
            return EditorGUIUtility.singleLineHeight * (tagsCount + 1) + (LineSeparation * tagsCount);
        }
    }
}
