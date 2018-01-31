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

            var oldHowToAnimateFrom = template.HowToAnimateFrom;
            var oldHowToAnimateTo = template.HowToAnimateTo;
            var oldAnimationType = template.AnimationType;

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptReference, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Animate From", EditorStyles.boldLabel);
            template.HowToAnimateFrom = (BaseTweenPlayableBehaviour.HowToAnimateType)EditorGUILayout.EnumPopup(new GUIContent("How To Animate:"), template.HowToAnimateFrom);
            if(template.HowToAnimateFrom == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferenceTransform)
            {
                EditorGUILayout.PropertyField(fromExposedRefence, new GUIContent("Reference Transform:"), true);
            }
            else if(template.HowToAnimateFrom == BaseTweenPlayableBehaviour.HowToAnimateType.UseAbsoluteValues)
            {
                template.AnimateFrom = EditorGUILayout.Vector3Field("Values:", template.AnimateFrom);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animate To", EditorStyles.boldLabel);
            template.HowToAnimateTo = (BaseTweenPlayableBehaviour.HowToAnimateType)EditorGUILayout.EnumPopup(new GUIContent("How To Animate:"), template.HowToAnimateTo);
            if(template.HowToAnimateTo == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferenceTransform)
            {
                EditorGUILayout.PropertyField(toExposedRefence, new GUIContent("Reference Transform:"), true);
            }
            else if(template.HowToAnimateTo == BaseTweenPlayableBehaviour.HowToAnimateType.UseAbsoluteValues)
            {
                template.AnimateTo = EditorGUILayout.Vector3Field("Values:", template.AnimateTo);
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

            // We need to focre gui changing if some enum popup has changed
            if(oldHowToAnimateFrom != template.HowToAnimateFrom ||
               oldHowToAnimateTo != template.HowToAnimateTo ||
               oldAnimationType != template.AnimationType)
            {
                GUI.changed = true;
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
