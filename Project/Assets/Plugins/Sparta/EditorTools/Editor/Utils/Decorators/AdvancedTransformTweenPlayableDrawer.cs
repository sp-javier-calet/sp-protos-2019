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
            // TODO missing referenced transforms

            var AnimationsProp = property.FindPropertyRelative("Animations");
            EditorGUILayout.PropertyField(AnimationsProp);

            if(AnimationsProp != null)
            {
                for(int i = 0; i < AnimationsProp.arraySize; ++i)
                {
                    var anim = AnimationsProp.GetArrayElementAtIndex(i);
                    if(anim != null)
                    {
                        var AnimateProp = anim.FindPropertyRelative("Animate");
                        var AnimateLabelProp = anim.FindPropertyRelative("AnimateLabel");
                        EditorGUILayout.PropertyField(AnimateProp, new GUIContent(AnimateLabelProp.stringValue));
                        if(AnimateProp.boolValue)
                        {
                            OnGUIAnimatedProperty(anim);
                        }
                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                }
            }
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
                if(HowToAnimateProp.enumValueIndex == (int)BaseAdvancedTweenBehaviour.HowToAnimateType.UseAbsoluteValues)
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
                if(AnimationTypeProp.enumValueIndex == (int)BaseAdvancedTweenBehaviour.AnimateType.AnimationCurve)
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
