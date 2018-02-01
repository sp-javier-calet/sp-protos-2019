using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(OpacityPlayableData))]
    public class OpacityPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var alphaProp = property.FindPropertyRelative("Alpha");
            EditorGUILayout.Slider(alphaProp, 0f, 1f);
        }
    }
}
