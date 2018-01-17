using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(PositionTweenPlayableBehaviour))]
    public class PositionTweenPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var AnimateFromProp = property.FindPropertyRelative("AnimateFrom");
            var AnimateToProp = property.FindPropertyRelative("AnimateTo");
            var EaseTypeProp = property.FindPropertyRelative("EaseType");
            var AnimationCurveProp = property.FindPropertyRelative("AnimationCurve");
            var AnimationTypeProp = property.FindPropertyRelative("AnimationType");
            var AnimPositionTypeProp = property.FindPropertyRelative("AnimPositionType");
            var StartLocationProp = property.FindPropertyRelative("StartLocation");
            var EndLocationProp = property.FindPropertyRelative("EndLocation");

            EditorGUILayout.PropertyField(AnimPositionTypeProp);
            if(AnimPositionTypeProp.enumValueIndex == (int)PositionTweenPlayableBehaviour.AnimatePositionType.UseReferencedTransforms)
            {
                EditorGUILayout.PropertyField(StartLocationProp);
                EditorGUILayout.PropertyField(EndLocationProp);
            }
            else
            {
                EditorGUILayout.PropertyField(AnimateFromProp);
                EditorGUILayout.PropertyField(AnimateToProp);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(AnimationTypeProp);

            if(AnimationTypeProp.enumValueIndex == (int)BaseTweenPlayableBehaviour.TweeningType.AnimationCurve)
            {
                EditorGUILayout.PropertyField(AnimationCurveProp);
            }
            else
            {
                EditorGUILayout.PropertyField(EaseTypeProp);
            }
        }
    }
}
