using SocialPoint.TimelinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(ColorPlayableData))]
    public class ColorPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var colorProp = property.FindPropertyRelative("Color");
            EditorGUILayout.PropertyField(colorProp);
        }
    }
}
