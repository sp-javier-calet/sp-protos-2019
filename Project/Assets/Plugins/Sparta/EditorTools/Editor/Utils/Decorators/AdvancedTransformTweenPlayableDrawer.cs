using SocialPoint.TimeLinePlayables;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomPropertyDrawer(typeof(AdvancedTransformTweenBehaviour))]
    public class AdvancedTransformTweenPlayableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var AnimationProp = property.FindPropertyRelative("Animations");

            var AnimatePositionProp = property.FindPropertyRelative("AnimatePosition");
            var AnimateRotationProp = property.FindPropertyRelative("AnimateRotation");
            var AnimateScaleProp = property.FindPropertyRelative("AnimateScale");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(AnimatePositionProp);
            if(AnimatePositionProp.boolValue)
            {
                var AnimationPositionProp = AnimationProp.GetArrayElementAtIndex(AdvancedTransformTweenBehaviour.kAnimatePosition);
                OnGUIAnimatedProperty(AnimationPositionProp);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(AnimateRotationProp);
            if(AnimateRotationProp.boolValue)
            {
                var AnimationRotationProp = AnimationProp.GetArrayElementAtIndex(AdvancedTransformTweenBehaviour.kAnimateRotation);
                OnGUIAnimatedProperty(AnimationRotationProp);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                
            EditorGUILayout.PropertyField(AnimateScaleProp);
            if(AnimateScaleProp.boolValue)
            {
                var AnimationScaleProp = AnimationProp.GetArrayElementAtIndex(AdvancedTransformTweenBehaviour.kAnimateScale);
                OnGUIAnimatedProperty(AnimationScaleProp);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        static void OnGUIAnimatedProperty(SerializedProperty property)
        {
            EditorGUI.indentLevel++;

            if(property != null)
            {
                var UseCurrentFromValueProp = property.FindPropertyRelative("UseCurrentFromValue");
                var UseCurrentToValueProp = property.FindPropertyRelative("UseCurrentToValue");
                var AnimateFromProp = property.FindPropertyRelative("AnimateFrom");
                var AnimateToProp = property.FindPropertyRelative("AnimateTo");
                var AnimateFromReferenceProp = property.FindPropertyRelative("AnimateFromReference");
                var AnimateToReferenceProp = property.FindPropertyRelative("AnimateToReference");
                var HowToAnimateProp = property.FindPropertyRelative("HowToAnimate");
                var AnimationTypeProp = property.FindPropertyRelative("AnimationType");
                var EaseTypeProp = property.FindPropertyRelative("EaseType");
                var AnimationCurveProp = property.FindPropertyRelative("AnimationCurve");

                EditorGUILayout.PropertyField(HowToAnimateProp);
                if(HowToAnimateProp.enumValueIndex == (int)AdvancedTweenBehaviour.HowToAnimateType.UseAbsoluteValues)
                {
                    EditorGUILayout.PropertyField(UseCurrentFromValueProp);
                    if(!UseCurrentFromValueProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(AnimateFromProp);
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.PropertyField(UseCurrentToValueProp);
                    if(!UseCurrentToValueProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(AnimateToProp);
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    // TODO link this with exposed vars

                    EditorGUILayout.PropertyField(UseCurrentFromValueProp);
                    if(!UseCurrentFromValueProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(AnimateFromReferenceProp);
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.PropertyField(UseCurrentToValueProp);
                    if(!UseCurrentToValueProp.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(AnimateToReferenceProp);
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUILayout.PropertyField(AnimationTypeProp);
                if(AnimationTypeProp.enumValueIndex == (int)AdvancedTweenBehaviour.AnimateType.AnimationCurve)
                {
                    // TODO Show bigger curve
                    EditorGUILayout.PropertyField(AnimationCurveProp);
                }
                else
                {
                    EditorGUILayout.PropertyField(EaseTypeProp);
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}
