using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(RectTransform)), CanEditMultipleObjects]
    public class RectTransformInspector : TransformResetter
    {
        SerializedProperty m_position;
        SerializedProperty m_positionZ;
        SerializedProperty m_rotation;
        SerializedProperty m_scale;

        public RectTransformInspector() : base("RectTransformEditor")
        {
        }

        void OnEnable()
        {
            m_position = serializedObject.FindProperty("m_AnchoredPosition");
            m_positionZ = serializedObject.FindProperty("m_LocalPosition.z");
            m_rotation = serializedObject.FindProperty("m_LocalRotation");
            m_scale = serializedObject.FindProperty("m_LocalScale");
        }

        protected override void ResetPosition()
        {
            m_position.vector2Value = m_resetPosition;
            m_positionZ.floatValue = m_resetPosition.z;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected override void ResetRotation()
        {
            m_rotation.quaternionValue = m_resetRotation;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
            EditorGUI.showMixedValue = true;
        }

        protected override void ResetScale()
        {
            m_scale.vector3Value = m_resetScale;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }
    }
}
