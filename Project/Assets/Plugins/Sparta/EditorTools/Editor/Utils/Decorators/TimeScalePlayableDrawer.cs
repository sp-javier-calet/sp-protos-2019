using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(TimeScalePlayableData))]
    public class TimeScalePlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var timeScaleProp = property.FindPropertyRelative("_timeScale");
            EditorGUILayout.PropertyField(timeScaleProp);
        }
    }
}
