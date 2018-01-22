using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(RotationTweenPlayableBehaviour))]
    public class RotationTweenPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var UseCurrentFromValueProp = property.FindPropertyRelative("UseCurrentFromValue");
            var UseCurrentToValueProp = property.FindPropertyRelative("UseCurrentToValue");
            var AnimateFromProp = property.FindPropertyRelative("AnimateFrom");
            var AnimateToProp = property.FindPropertyRelative("AnimateTo");
            var EaseTypeProp = property.FindPropertyRelative("EaseType");
            var AnimationCurveProp = property.FindPropertyRelative("AnimationCurve");
            var AnimationTypeProp = property.FindPropertyRelative("AnimationType");
            var AnimPositionTypeProp = property.FindPropertyRelative("AnimPositionType");
//            var TransformFromProp = property.FindPropertyRelative("TransformFrom");
//            var TransformToProp = property.FindPropertyRelative("TransformTo");

            EditorGUILayout.PropertyField(AnimPositionTypeProp);
            if(AnimPositionTypeProp.enumValueIndex == (int)BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms)
            {
//                EditorGUILayout.PropertyField(TransformFromProp);
//                EditorGUILayout.PropertyField(TransformToProp);
            }
            else
            {
                EditorGUILayout.PropertyField(UseCurrentFromValueProp);
                if(!UseCurrentFromValueProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(AnimateFromProp);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(UseCurrentToValueProp);
                if(!UseCurrentToValueProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(AnimateToProp);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(AnimationTypeProp);

            if(AnimationTypeProp.enumValueIndex == (int)BaseTweenPlayableBehaviour.AnimateType.AnimationCurve)
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
