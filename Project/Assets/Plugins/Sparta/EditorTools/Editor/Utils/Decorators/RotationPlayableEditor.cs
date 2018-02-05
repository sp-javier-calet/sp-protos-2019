using SocialPoint.TimeLinePlayables;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(RotationPlayableAsset))]
    public class RotationPlayableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var playableTarget = (RotationPlayableAsset)target;
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
            template.HowToAnimateFrom = (BaseTransformPlayableData.HowToAnimateType)EditorGUILayout.EnumPopup(new GUIContent("How To Animate:"), template.HowToAnimateFrom);
            if(template.HowToAnimateFrom == BaseTransformPlayableData.HowToAnimateType.UseReferenceTransform)
            {
                EditorGUILayout.PropertyField(fromExposedRefence, new GUIContent("Reference Transform:"), true);

                var obj = ((Transform)fromExposedRefence.objectReferenceValue);
                template.AnimateFrom = obj != null ? obj.rotation : Quaternion.identity;
            }
            else if(template.HowToAnimateFrom == BaseTransformPlayableData.HowToAnimateType.UseAbsoluteValues)
            {
                template.AnimateFrom = ConvertToQuaternion(EditorGUILayout.Vector3Field("Values:", template.AnimateFrom.eulerAngles));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animate To", EditorStyles.boldLabel);
            template.HowToAnimateTo = (BaseTransformPlayableData.HowToAnimateType)EditorGUILayout.EnumPopup(new GUIContent("How To Animate:"), template.HowToAnimateTo);
            if(template.HowToAnimateTo == BaseTransformPlayableData.HowToAnimateType.UseReferenceTransform)
            {
                EditorGUILayout.PropertyField(toExposedRefence, new GUIContent("Reference Transform:"), true);

                var obj = ((Transform)toExposedRefence.objectReferenceValue);
                template.AnimateTo = obj != null ? obj.rotation : Quaternion.identity;
            }
            else if(template.HowToAnimateTo == BaseTransformPlayableData.HowToAnimateType.UseAbsoluteValues)
            {
                template.AnimateTo = ConvertToQuaternion(EditorGUILayout.Vector3Field("Values:", template.AnimateTo.eulerAngles));
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            template.AnimationType = (BaseTransformPlayableData.AnimateType)EditorGUILayout.EnumPopup("Animation Type", template.AnimationType);
            if(template.AnimationType == BaseTransformPlayableData.AnimateType.AnimationCurve)
            {
                template.AnimationCurve = EditorGUILayout.CurveField(" ", template.AnimationCurve, GUILayout.MinHeight(80));
            }
            else
            {
                template.EaseType = (EaseType)EditorGUILayout.EnumPopup("Easing", template.EaseType);
            }

            // We need to focre gui changing if some enum popup has changed
            if(oldHowToAnimateFrom != template.HowToAnimateFrom || oldHowToAnimateTo != template.HowToAnimateTo || oldAnimationType != template.AnimationType)
            {
                GUI.changed = true;
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        Quaternion ConvertToQuaternion(Vector3 eulerAngles)
        {
            return Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }
    }
}
