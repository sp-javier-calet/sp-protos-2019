using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(TimeScalePlayableBehaviour))]
    public class TimeDilationDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var timeScaleProp = property.FindPropertyRelative("TimeScale");
            EditorGUILayout.PropertyField(timeScaleProp);
        }
    }
}
