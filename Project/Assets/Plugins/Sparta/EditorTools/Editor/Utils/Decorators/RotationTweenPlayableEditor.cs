using SocialPoint.TimeLinePlayables;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(RotationTweenPlayableClip))]
    public class RotationTweenPlayableInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var playableTarget = (RotationTweenPlayableClip)target;
            var template = playableTarget.Template;

            var scriptReference = serializedObject.FindProperty("m_Script");
            var fromExposedRefence = serializedObject.FindProperty("TransformFrom");
            var toExposedRefence = serializedObject.FindProperty("TransformTo");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptReference, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            template.HowToAnimate = (BaseTweenPlayableBehaviour.HowToAnimateType)EditorGUILayout.EnumPopup("How To Animate", template.HowToAnimate);
            if(template.HowToAnimate == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms)
            {
                EditorGUILayout.PropertyField(fromExposedRefence, new GUIContent("Reference From"), true);
                EditorGUILayout.PropertyField(toExposedRefence, new GUIContent("Reference To"), true);
            }
            else
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Animate From:", EditorStyles.boldLabel);

                template.UseCurrentFromValue = EditorGUILayout.Toggle("Use Current Value", template.UseCurrentFromValue);
                if(!template.UseCurrentFromValue)
                {
                    EditorGUI.indentLevel++;
                    template.AnimateFrom = EditorGUILayout.Vector3Field("Value From", template.AnimateFrom);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animate To:", EditorStyles.boldLabel);
                template.UseCurrentToValue = EditorGUILayout.Toggle("Use Current Value", template.UseCurrentToValue);
                if(!template.UseCurrentToValue)
                {
                    EditorGUI.indentLevel++;
                    template.AnimateTo = EditorGUILayout.Vector3Field("Value To", template.AnimateTo);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            template.AnimationType = (BaseTweenPlayableBehaviour.AnimateType)EditorGUILayout.EnumPopup("Animation Type", template.AnimationType);
            if(template.AnimationType == BaseTweenPlayableBehaviour.AnimateType.AnimationCurve)
            {
                template.AnimationCurve = EditorGUILayout.CurveField(" ", template.AnimationCurve, GUILayout.MinHeight(80));
            }
            else
            {
                template.EaseType = (EaseType)EditorGUILayout.EnumPopup("Easing", template.EaseType);
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
