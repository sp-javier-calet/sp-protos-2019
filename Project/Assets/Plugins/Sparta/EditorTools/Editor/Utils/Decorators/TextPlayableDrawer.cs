using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(TextPlayableData))]
    public class TextPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colorProp = property.FindPropertyRelative("Color");
            var fontSizeProp = property.FindPropertyRelative("FontSize");
            var textProp = property.FindPropertyRelative("Text");
            var useLocalizedDataProp = property.FindPropertyRelative("UseLocalizedData");

            EditorGUILayout.PropertyField(colorProp);
            EditorGUILayout.PropertyField(fontSizeProp);
            EditorGUILayout.PropertyField(useLocalizedDataProp);
            EditorGUILayout.PropertyField(textProp);
        }
    }
}
